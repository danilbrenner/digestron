using Digestron.Domain;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Logging;

namespace Digestron.Service.Services;

public class EmailService(
    ILogger<EmailService> logger,
    IMessageResponder messageResponder,
    IEmailProvider emailProvider,
    IDigestService digestService
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

    public async Task HandleDigestAsync(MessageContext context, CancellationToken ct = default)
    {
        logger.LogInformation("Handling digest request for user {UserId}", context.UserId);

        await messageResponder.SendDigestLoadingMessageAsync(context, ct);

        var emails = await emailProvider.GetUnreadEmailsAsync(context, 50, ct);
        logger.LogInformation("Fetched {EmailCount} unread emails for digest", emails.Count);

        if (emails.Count == 0)
        {
            await messageResponder.SendDigestAsync(context, "✅ Your inbox is empty — no unread emails!", ct);
            return;
        }

        var result = await digestService.GenerateDigestAsync(emails, ct);
        logger.LogInformation("Generated digest with {SuggestedCount} low-priority IDs", result.SuggestedReadIds.Count);

        await messageResponder.SendDigestAsync(context, result.MarkdownText, ct);
    }
}