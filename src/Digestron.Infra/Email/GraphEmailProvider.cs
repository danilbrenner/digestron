using System.Collections.Concurrent;
using Azure.Identity;
using Digestron.Domain;
using Digestron.Infra.Options;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Digestron.Infra.Email;

public sealed class GraphEmailProvider(
    IOptions<GraphOptions> options,
    IMessageResponder responder,
    ILogger<GraphEmailProvider> logger) : IEmailProvider
{
    private const int MaxBodyPreviewLength = 300;

    private static readonly ConcurrentDictionary<long, GraphServiceClient> GraphClientsContainer = new();

    private GraphServiceClient CreateGraphClient(MessageContext context)
    {
        var credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
        {
            TenantId = "common",
            ClientId = options.Value.ClientId,
            DeviceCodeCallback = async (ci, ct) =>
                await responder.SendDeviceAuthenticationRequestAsync(
                    context,
                    ci.VerificationUri,
                    ci.UserCode,
                    ci.ExpiresOn,
                    ct)
        });
        return new GraphServiceClient(credential);
    }

    private GraphServiceClient GetGraphClient(MessageContext context)
        => GraphClientsContainer.GetOrAdd(context.ChatId, _ => CreateGraphClient(context));

    public async Task<IReadOnlyList<EmailMessage>> GetUnreadEmailsAsync(
        MessageContext context,
        int max,
        CancellationToken ct = default)
    {
        var graphClient = GetGraphClient(context);

        logger.LogInformation("Fetching up to {Max} unread emails", max);

        var response = await graphClient
            .Me
            .MailFolders["inbox"]
            .Messages
            .GetAsync(config =>
            {
                config.QueryParameters.Filter = "isRead eq false";
                config.QueryParameters.Select = ["id", "subject", "from", "receivedDateTime", "bodyPreview"];
                config.QueryParameters.Top = max;
            }, ct);

        if (response?.Value is null)
            return [];

        var emails = response.Value
            .Select(m => new EmailMessage(
                Id: m.Id ?? string.Empty,
                Subject: m.Subject ?? "(no subject)",
                Sender: m.From?.EmailAddress?.Address ?? "(unknown)",
                ReceivedAt: m.ReceivedDateTime ?? DateTimeOffset.MinValue,
                BodyPreview: Truncate(m.BodyPreview ?? string.Empty, MaxBodyPreviewLength)))
            .ToList();

        logger.LogInformation("Fetched {Count} unread emails", emails.Count);
        return emails;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}