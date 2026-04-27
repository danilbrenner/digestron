using Digestron.Domain;
using Digestron.Infra.Options;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Digestron.Hosting;

public sealed class ScheduledDigestService(
    IEmailService emailService,
    IOptions<ScheduleOptions> scheduleOptions,
    ILogger<ScheduledDigestService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var deliveryTimes = ParseDeliveryTimes(scheduleOptions.Value.DeliveryTimesUtc);
        if (deliveryTimes.Length == 0)
        {
            logger.LogWarning("No delivery times configured");
            return;
        }

        var ix = 0;
        while (!ct.IsCancellationRequested)
        {
            ix = (ix + 1) % deliveryTimes.Length;
            var delay = GetDelayUntil(deliveryTimes[ix]);

            logger.LogInformation("Next scheduled digest delivery in {Delay}", delay);

            await Task.Delay(delay, ct);

            if (ct.IsCancellationRequested)
                return;

            await emailService.HandleDigestToAllAsync(ct);
        }
    }

    private static TimeSpan GetDelayUntil(TimeOnly target)
    {
        var delay = target - TimeOnly.FromDateTime(DateTime.UtcNow);
        return delay <= TimeSpan.Zero ? delay.Add(TimeSpan.FromHours(24)) : delay;
    }

    private static TimeOnly[] ParseDeliveryTimes(string[] rawTimes) =>
        rawTimes
            .Select(raw => TimeOnly.TryParseExact(raw, "HH:mm", out var t) ? t : (TimeOnly?)null)
            .Where(t => t != null)
            .Select(t => t!.Value)
            .ToArray();
}