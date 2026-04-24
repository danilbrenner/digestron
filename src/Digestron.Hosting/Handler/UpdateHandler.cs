using System.Diagnostics.CodeAnalysis;
using Digestron.Domain;
using Digestron.Service.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Digestron.Hosting.Handler;

public sealed class UpdateHandler(
    IEmailService emailService,
    IMessageResponder messageResponder,
    ILogger<UpdateHandler> logger)
{
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        if (!update.ParseUpdate(out var context))
            return;

        logger.LogInformation("Received message from {UserId} in chat {ChatId}", context.UserId, context.ChatId);

        await (context.Content switch
        {
            CommandMessageContent { Command: "/start" } => messageResponder.SendStartMessageAsync(context, ct),
            CommandMessageContent { Command: "/help" } => messageResponder.SendHelpMessageAsync(context, ct),
            CommandMessageContent { Command: "/digest" } => emailService.HandleDigestAsync(context, ct),
            CommandMessageContent { Command: "/unread" } => emailService.HandleGetUnreadEmailCountAsync(context, ct),
            CommandMessageContent => messageResponder.SendUnknownCommandMessageAsync(context, ct),
            _ => Task.CompletedTask
        });
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Telegram polling error");
        return Task.CompletedTask;
    }
}

public static class UpdateHandlerExtensions
{
    public static bool ParseUpdate(
        this Update update,
        [NotNullWhen(true)] out MessageContext? context)
    {
        context = null;

        if (update.Message is not { } message)
            return false;

        if (message.Text is not { } messageText)
            return false;

        context =
            update.ToContext(
                messageText.StartsWith("/")
                    ? new CommandMessageContent(messageText.ToLowerInvariant())
                    : new TextMessageContent(messageText)
            );
        return true;
    }

    private static MessageContext ToContext(this Update update, MessageContent content)
    {
        var message = update.Message;
        var from = message?.From;
        return new MessageContext
        {
            ChatId = message?.Chat.Id ?? 0,
            UserId = from?.Id ?? 0,
            UserName = from?.Username ?? from?.FirstName ?? "unknown",
            Content = content
        };
    }
}