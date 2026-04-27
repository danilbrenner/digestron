namespace Digestron.Domain;

public sealed class CommandContext
{
    public required long ChatId { get; init; }
    public required long UserId { get; init; }
    public required string UserName { get; init; }
    public required MessageContent Content { get; init; }
    public int? ResponseMessageId { get; set; }
}

public abstract record MessageContent;

public sealed record TextMessageContent(string Text) : MessageContent;

public sealed record CommandMessageContent(string Command) : MessageContent;
