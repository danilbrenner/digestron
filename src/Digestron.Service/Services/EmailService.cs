using Digestron.Domain;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Logging;

namespace Digestron.Service.Services;

public class EmailService(
    ILogger<EmailService> logger,
    IMessageResponder messageResponder,
    IEmailProvider emailProvider
    ) : IEmailService
{
    public async Task HandleGetUnreadEmailCountAsync(MessageContext context, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching unread email count for user {UserId}", context.UserId);
        
        var count = await emailProvider.GetUnreadEmailsAsync(context, 100, ct);
        logger.LogInformation("Found {UnreadCount} unread email(s) for user {UserId}", count.Count, context.UserId);
        
        logger.LogInformation("Sending unread count message to chat {ChatId}", context.ChatId);
        await messageResponder.SendUnreadCountMessageAsync(context, count.Count, ct);
        logger.LogInformation("Successfully sent unread count message to chat {ChatId}", context.ChatId);
    }
}