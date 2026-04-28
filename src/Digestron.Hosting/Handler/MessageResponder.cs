using Digestron.Service.Abstractions;
using Digestron.Domain;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace Digestron.Hosting.Handler;

public sealed class MessageResponder(ITelegramBotClient botClient) : IMessageResponder
{
    public async Task SendStartMessageAsync(CommandContext context, CancellationToken ct) =>
        await SendMessageAsync(context,
            $"👋 *Welcome to Digestron!*\n\nI help you stay on top of your inbox. Use /help to see what I can do.\n\nYou're chat ID is: `{context.ChatId}`",
            ParseMode.MarkdownV2, ct);

    public async Task SendHelpMessageAsync(CommandContext context, CancellationToken ct) =>
        await SendMessageAsync(context,
            """
            *Available commands:*
            /start — Welcome message
            /help — Show this help
            /digest — Get an AI\-powered digest of your unread emails
            /unread — Show the count of unread emails
            /reloadprompt — Reload the AI system prompt from disk
            """,
            ParseMode.MarkdownV2, ct);

    public async Task SendDigestLoadingMessageAsync(CommandContext context, CancellationToken ct) =>
        await SendMessageAsync(context, "⏳ Fetching digest...", ParseMode.None, ct);

    public async Task SendUnreadCountMessageAsync(CommandContext context, int count, CancellationToken ct) =>
        await SendMessageAsync(context, $"📬 You have *{count}* unread email(s).", ParseMode.Markdown, ct);

    public async Task SendUnknownCommandMessageAsync(CommandContext context, CancellationToken ct) =>
        await SendMessageAsync(context, "❓ Unknown command. Use /help to see available commands.", ParseMode.None, ct);

    public async Task SendDeviceAuthenticationRequestAsync(CommandContext context, Uri verificationUri,
        string userCode, DateTimeOffset expiresOn, CancellationToken ct) =>
        await SendMessageAsync(context,
            $"""
             🔐 *Device Code Authentication*

             To sign in and grant access to your emails, visit:
             {verificationUri}

             Enter this code: `{userCode}`

             The code will expire in {expiresOn.ToLocalTime():HH:mm:ss}.
             """,
            ParseMode.Markdown, ct);

    public async Task SendDigestAsync(CommandContext context, string markdownText, int totalTokens,
        CancellationToken ct)
    {
        var text = totalTokens > 0 ? $"{markdownText}\n\n_🔢 Tokens used: {totalTokens}_" : markdownText;
        await SendMessageAsync(context, text, ParseMode.Markdown, ct);
    }

    public async Task SendPromptReloadedMessageAsync(CommandContext context, CancellationToken ct) =>
        await SendMessageAsync(context, "✅ System prompt reloaded.", ParseMode.None, ct);

    private async Task SendMessageAsync(CommandContext context, string text, ParseMode parseMode, CancellationToken ct)
    {
        try
        {
            var message =
                context switch
                {
                    { ResponseMessageId: { } messageId } =>
                        await botClient.EditMessageText(context.ChatId, messageId, text, parseMode: parseMode,
                            cancellationToken: ct),
                    _ =>
                        await botClient.SendMessage(context.ChatId, text, parseMode: parseMode,
                            cancellationToken: ct)
                };

            context.ResponseMessageId = message.MessageId;
        }
        catch (ApiRequestException ex) when (parseMode != ParseMode.None &&
                                             ex.Message.Contains("can't parse entities",
                                                 StringComparison.OrdinalIgnoreCase))
        {
            await botClient.SendMessage(
                context.ChatId,
                "⚠️ Failed to render the digest — the response contained characters that couldn't be formatted.",
                parseMode: ParseMode.None,
                cancellationToken: ct);
        }
    }
}