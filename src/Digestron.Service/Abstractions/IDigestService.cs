using Digestron.Domain;

namespace Digestron.Service.Abstractions;

public interface IDigestService
{
    Task<DigestResult> GenerateDigestAsync(IReadOnlyList<EmailMessage> emails, CancellationToken ct = default);
    Task ReloadPrompt();
}
