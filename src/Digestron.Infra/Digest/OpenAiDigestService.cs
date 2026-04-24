using System.Text.Json;
using System.Text.Json.Serialization;
using Digestron.Domain;
using Digestron.Infra.Options;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Digestron.Infra.Digest;

public sealed class OpenAiDigestService(
    IOptions<OpenAiOptions> options,
    ILogger<OpenAiDigestService> logger) : IDigestService
{
    private const int MaxBodyPreviewLength = 300;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DigestResult> GenerateDigestAsync(
        IReadOnlyList<EmailMessage> emails,
        CancellationToken ct = default)
    {
        logger.LogInformation("Generating AI digest for {EmailCount} emails", emails.Count);

        var opts = options.Value;
        var client = new OpenAIClient(opts.ApiKey);
        var chatClient = client.GetChatClient(opts.Model);

        var userPrompt = BuildUserPrompt(emails);

        ChatCompletion completion = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(userPrompt)
            ],
            new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() },
            ct);

        var responseText = completion.Content[0].Text;
        logger.LogInformation("Received AI digest response");

        return ParseResponse(responseText);
    }

    private static string BuildUserPrompt(IReadOnlyList<EmailMessage> emails)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Here are my unread emails. Please analyze them and return the JSON digest:");
        sb.AppendLine();

        foreach (var email in emails)
        {
            var preview = email.BodyPreview.Length > MaxBodyPreviewLength
                ? email.BodyPreview[..MaxBodyPreviewLength]
                : email.BodyPreview;

            sb.AppendLine($"ID: {email.Id}");
            sb.AppendLine($"Subject: {email.Subject}");
            sb.AppendLine($"From: {email.Sender}");
            sb.AppendLine($"Received: {email.ReceivedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Preview: {preview}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static DigestResult ParseResponse(string responseText)
    {
        var response = JsonSerializer.Deserialize<OpenAiDigestResponse>(responseText, JsonOptions);

        return new DigestResult(
            response?.MarkdownText ?? responseText,
            response?.SuggestedReadIds ?? []);
    }

    private const string SystemPrompt = """
        You are an email digest assistant. Analyze the provided emails and create a concise, actionable digest.
        Return a JSON object with exactly these two fields:
        1. "markdownText": A markdown-formatted digest that groups emails into sections:
           - 🔴 *Action Required* — emails needing a response or action
           - 📌 *FYI / Important* — informational emails worth reading
           - 📰 *Newsletters & Low Priority* — bulk mail, newsletters, promotions
           Each section lists relevant emails with their subject and sender.
           Keep it concise and scannable. Use Telegram-compatible Markdown (no HTML, no tables).
        2. "suggestedReadIds": An array of email ID strings for low-priority emails that can be safely marked as read (newsletters, promotions, notifications).
        """;

    private sealed record OpenAiDigestResponse(
        [property: JsonPropertyName("markdownText")] string MarkdownText,
        [property: JsonPropertyName("suggestedReadIds")] List<string> SuggestedReadIds);
}
