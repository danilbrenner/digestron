using Digestron.Service.Abstractions;
using Digestron.Domain;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Digestron.Hosting.Handler;

public sealed class MessageResponder(ITelegramBotClient botClient) : IMessageResponder
{
    public Task SendStartMessageAsync(CommandContext context, CancellationToken ct) =>
        botClient.SendMessage(
            context.ChatId,
            $"👋 *Welcome to Digestron!*\n\nI help you stay on top of your inbox. Use /help to see what I can do.\n\nYou're chat ID is: `{context.ChatId}`",
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: ct);

    public Task SendHelpMessageAsync(CommandContext context, CancellationToken ct) =>
        botClient.SendMessage(
            context.ChatId,
            """
            *Available commands:*
            /start — Welcome message
            /help — Show this help
            /digest — Get an AI\-powered digest of your unread emails
            /unread — Show the count of unread emails
            /reloadprompt — Reload the AI system prompt from disk
            """,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: ct);

    public async Task SendDigestLoadingMessageAsync(CommandContext context, CancellationToken ct)
    {
        var message = await botClient.SendMessage(
            context.ChatId,
            "⏳ Fetching digest...",
            cancellationToken: ct);

        context.ResponseMessageId = message.Id;
    }

    public Task SendUnreadCountMessageAsync(CommandContext context, int count, CancellationToken ct) =>
        botClient.SendMessage(
            context.ChatId,
            $"📬 You have *{count}* unread email(s).",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

    public Task SendUnknownCommandMessageAsync(CommandContext context, CancellationToken ct) =>
        botClient.SendMessage(
            context.ChatId,
            "❓ Unknown command. Use /help to see available commands.",
            cancellationToken: ct);

    public async Task SendDeviceAuthenticationRequestAsync(CommandContext context, Uri verificationUri, string userCode,
        DateTimeOffset expiresOn, CancellationToken ct)
    {
        var message =
            $"""
             🔐 *Device Code Authentication*

             To sign in and grant access to your emails, visit:
             {verificationUri}

             Enter this code: `{userCode}`

             The code will expire in {expiresOn.ToLocalTime():HH:mm:ss}.
             """;


        await botClient.SendMessage(
            context.ChatId,
            message,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    public Task SendDigestAsync(CommandContext context, string markdownText, int totalTokens, CancellationToken ct)
    {
        var text = totalTokens > 0
            ? $"{markdownText}\n\n_🔢 Tokens used: {totalTokens}_"
            : markdownText;

        return botClient.SendMessage(
            context.ChatId,
            text,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    public Task EditDigestMessageAsync(CommandContext context, string markdownText, int totalTokens, CancellationToken ct)
    {
        var text = totalTokens > 0
            ? $"{markdownText}\n\n_🔢 Tokens used: {totalTokens}_"
            : markdownText;

        return botClient.EditMessageText(
            context.ChatId,
            context.ResponseMessageId!.Value,
            text,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    public Task SendPromptReloadedMessageAsync(CommandContext context, CancellationToken ct) =>
        botClient.SendMessage(
            context.ChatId,
            "✅ System prompt reloaded.",
            cancellationToken: ct);
}