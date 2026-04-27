using Digestron.Domain;

namespace Digestron.Service.Abstractions;

public interface IEmailService
{
    Task HandleGetUnreadEmailCountAsync(CommandContext context, CancellationToken ct = default);
    Task HandleDigestAsync(CommandContext context, CancellationToken ct = default);
    Task HandleDigestToAllAsync(CancellationToken ct = default);
}

