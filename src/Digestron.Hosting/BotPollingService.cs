using Digestron.Hosting.Handler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Digestron.Hosting;

public sealed class BotPollingService(
    ITelegramBotClient botClient,
    UpdateHandler updateHandler,
    ILogger<BotPollingService> logger) : IHostedService
{
    private readonly CancellationTokenSource _cts = new ();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message],
            DropPendingUpdates = true
        };

        logger.LogInformation("Starting Telegram bot long-polling");

        botClient.StartReceiving(
            updateHandler: updateHandler.HandleUpdateAsync,
            errorHandler: updateHandler.HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Telegram bot long-polling");
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
