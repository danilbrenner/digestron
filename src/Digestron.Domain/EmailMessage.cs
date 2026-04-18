namespace Digestron.Domain;

public sealed record EmailMessage(
    string Id,
    string Subject,
    string Sender,
    DateTimeOffset ReceivedAt,
    string BodyPreview);
