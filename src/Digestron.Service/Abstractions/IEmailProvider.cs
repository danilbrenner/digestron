using Digestron.Domain;

namespace Digestron.Service.Abstractions;

public interface IEmailProvider
{
    Task<IReadOnlyList<EmailMessage>> GetUnreadEmailsAsync(CommandContext context, int max, CancellationToken ct = default);
}
