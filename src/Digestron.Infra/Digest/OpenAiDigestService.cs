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

    private volatile string? _systemPrompt;

    public async Task ReloadPrompt()
    {
        _systemPrompt = await LoadPrompt();
        logger.LogInformation("System prompt reloaded");
    }

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
                new SystemChatMessage(_systemPrompt ?? await LoadPrompt()),
                new UserChatMessage(userPrompt)
            ],
            new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() },
            ct);

        var totalTokens = completion.Usage?.TotalTokenCount ?? 0;
        logger.LogInformation("Received AI digest response. Total tokens used: {TotalTokens}", totalTokens);

        var responseText = completion.Content[0].Text;
        return ParseResponse(responseText, totalTokens);
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

    private static DigestResult ParseResponse(string responseText, int totalTokens)
    {
        var response = JsonSerializer.Deserialize<OpenAiDigestResponse>(responseText, JsonOptions);

        return new DigestResult(
            response?.MarkdownText ?? responseText,
            response?.SuggestedReadIds ?? [],
            totalTokens);
    }

    private Task<string> LoadPrompt()
    {
        return File.ReadAllTextAsync("system-prompt.md");
    }

    private sealed record OpenAiDigestResponse(
        [property: JsonPropertyName("markdownText")]
        string MarkdownText,
        [property: JsonPropertyName("suggestedReadIds")]
        List<string> SuggestedReadIds);
}