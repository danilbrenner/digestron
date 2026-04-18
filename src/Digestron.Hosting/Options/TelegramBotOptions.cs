namespace Digestron.Hosting.Options;

public sealed class TelegramBotOptions
{
    public const string SectionName = "TelegramBot";

    public string BotToken { get; set; } = string.Empty;
}
