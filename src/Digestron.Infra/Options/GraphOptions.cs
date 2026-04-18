namespace Digestron.Infra.Options;

public sealed class GraphOptions
{
    public const string SectionName = "Graph";

    /// <summary>
    /// Microsoft Entra ID application (client) ID.
    /// This is the only value needed for Device Code flow.
    /// No client secret is stored — authentication happens via browser sign-in.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
}

