using Digestron.Domain;
using Digestron.Infra.Options;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Digestron.Hosting;

public sealed class ScheduledDigestService : IHostedService
{
    private static readonly TimeOnly[] DefaultDeliveryTimes = [new(8, 0), new(18, 0)];

    private readonly IEmailProvider _emailProvider;
    private readonly IEmailService _emailService;
    private readonly ILogger<ScheduledDigestService> _logger;
    private readonly TimeOnly[] _deliveryTimes;

    private Task? _runTask;
    private CancellationTokenSource? _cts;

    public ScheduledDigestService(
        IEmailProvider emailProvider,
        IEmailService emailService,
        IOptions<ScheduleOptions> scheduleOptions,
        ILogger<ScheduledDigestService> logger)
    {
        _emailProvider = emailProvider;
        _emailService = emailService;
        _logger = logger;
        _deliveryTimes = ParseDeliveryTimes(scheduleOptions.Value.DeliveryTimesUtc);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runTask = RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
            await _cts.CancelAsync();

        if (_runTask is not null)
            await Task.WhenAny(_runTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    internal async Task DeliverToAllChatsAsync(CancellationToken ct)
    {
        var chatIds = _emailProvider.GetAuthenticatedChatIds();

        if (chatIds.Count == 0)
        {
            _logger.LogInformation("No authenticated chats — skipping scheduled digest delivery");
            return;
        }

        foreach (var chatId in chatIds)
        {
            try
            {
                var context = new CommandContext
                {
                    ChatId = chatId,
                    UserId = 0,
                    UserName = "Scheduled",
                    Content = new CommandMessageContent("/digest")
                };

                await _emailService.HandleDigestAsync(context, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering scheduled digest to chat {ChatId}", chatId);
            }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextDelivery();
                _logger.LogInformation("Next scheduled digest delivery in {Delay}", delay);

                await Task.Delay(delay, ct);

                if (!ct.IsCancellationRequested)
                    await DeliverToAllChatsAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }

    private TimeSpan GetDelayUntilNextDelivery()
    {
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        var today = DateTime.UtcNow.Date;

        var nextTime = _deliveryTimes
            .Select(t => today.Add(t.ToTimeSpan()))
            .Concat(_deliveryTimes.Select(t => today.AddDays(1).Add(t.ToTimeSpan())))
            .Where(dt => dt > DateTime.UtcNow)
            .Min();

        return nextTime - DateTime.UtcNow;
    }

    private TimeOnly[] ParseDeliveryTimes(string[] rawTimes)
    {
        if (rawTimes.Length == 0)
        {
            _logger.LogInformation("No delivery times configured, using defaults: 08:00, 18:00 UTC");
            return DefaultDeliveryTimes;
        }

        var parsed = new List<TimeOnly>(rawTimes.Length);

        foreach (var raw in rawTimes)
        {
            if (TimeOnly.TryParseExact(raw, "HH:mm", out var time))
            {
                parsed.Add(time);
            }
            else
            {
                _logger.LogError("Invalid delivery time '{Value}' — falling back to defaults 08:00, 18:00 UTC", raw);
                return DefaultDeliveryTimes;
            }
        }

        return [.. parsed];
    }
}
