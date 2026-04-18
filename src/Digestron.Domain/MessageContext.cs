namespace Digestron.Domain;

public sealed class MessageContext
{
    public required long ChatId { get; init; }
    public required long UserId { get; init; }
    public required string UserName { get; init; }
}
