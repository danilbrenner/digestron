namespace Digestron.Domain;

public sealed record DigestResult(
    string MarkdownText,
    IReadOnlyList<string> SuggestedReadIds,
    int TotalTokens = 0);
