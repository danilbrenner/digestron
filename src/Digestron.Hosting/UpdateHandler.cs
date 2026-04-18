using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Digestron.Hosting;

public sealed class UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger)
{
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } text)
            return;

        logger.LogInformation("Received message from {UserId} in chat {ChatId}: {Text}",
            message.From?.Id, message.Chat.Id, text);

        var command = text.Split(' ')[0].ToLowerInvariant();

        await (command switch
        {
            "/start" => HandleStartAsync(message, ct),
            "/help"  => HandleHelpAsync(message, ct),
            "/digest" => HandleDigestAsync(message, ct),
            "/unread" => HandleUnreadAsync(message, ct),
            _ => HandleUnknownAsync(message, ct)
        });
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Telegram polling error");
        return Task.CompletedTask;
    }

    private Task HandleStartAsync(Message message, CancellationToken ct) =>
        botClient.SendMessage(
            message.Chat.Id,
            $"👋 *Welcome to Digestron!*\n\nI help you stay on top of your inbox. Use /help to see what I can do.\n\nYou're chat ID is: `{message.Chat.Id}`",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

    private Task HandleHelpAsync(Message message, CancellationToken ct) =>
        botClient.SendMessage(
            message.Chat.Id,
            """
            *Available commands:*
            /start — Welcome message
            /help — Show this help
            /digest — Get an AI\-powered digest of your unread emails
            /unread — Show the count of unread emails
            """,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: ct);

    private Task HandleDigestAsync(Message message, CancellationToken ct) =>
        botClient.SendMessage(
            message.Chat.Id,
            "⏳ Fetching digest...",
            cancellationToken: ct);

    private Task HandleUnreadAsync(Message message, CancellationToken ct) =>
        botClient.SendMessage(
            message.Chat.Id,
            "⏳ Fetching unread count...",
            cancellationToken: ct);

    private Task HandleUnknownAsync(Message message, CancellationToken ct) =>
        botClient.SendMessage(
            message.Chat.Id,
            "❓ Unknown command. Use /help to see available commands.",
            cancellationToken: ct);
}
