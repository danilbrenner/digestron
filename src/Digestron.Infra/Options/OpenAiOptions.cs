namespace Digestron.Infra.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    /// <summary>
    /// OpenAI API key. Store in Azure Key Vault or environment variables — never in source code.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-4o-mini";
}
