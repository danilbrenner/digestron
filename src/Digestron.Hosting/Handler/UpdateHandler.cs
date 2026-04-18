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
        if (!update.ParseUpdate(out var context, out var command, out var text))
            return;

        logger.LogInformation("Received message from {UserId} in chat {ChatId}: {Text}",
            context.UserId, context.ChatId, text);

        await (command switch
        {
            "/start" => messageResponder.SendStartMessageAsync(context, ct),
            "/help" => messageResponder.SendHelpMessageAsync(context, ct),
            "/digest" => messageResponder.SendDigestLoadingMessageAsync(context, ct),
            "/unread" => emailService.HandleGetUnreadEmailCountAsync(context, ct),
            _ => messageResponder.SendUnknownCommandMessageAsync(context, ct)
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
        [NotNullWhen(true)] out MessageContext? context,
        [NotNullWhen(true)] out string? command,
        [NotNullWhen(true)] out string? text)
    {
        context = null;
        command = null;
        text = null;

        if (update.Message is not { } message)
            return false;

        if (message.Text is not { } messageText)
            return false;

        context = update.ToContext();
        command = messageText.Split(' ')[0].ToLowerInvariant();
        text = messageText;

        return true;
    }

    private static MessageContext ToContext(this Update update)
    {
        var message = update.Message;
        var from = message?.From;
        return new MessageContext
        {
            ChatId = message?.Chat.Id ?? 0,
            UserId = from?.Id ?? 0,
            UserName = from?.Username ?? from?.FirstName ?? "unknown"
        };
    }
}