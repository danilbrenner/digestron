using Digestron.Domain;

namespace Digestron.Service.Abstractions;

public interface IEmailProvider
{
    Task<IReadOnlyList<EmailMessage>> GetUnreadEmailsAsync(MessageContext context, int max, CancellationToken ct = default);
}
