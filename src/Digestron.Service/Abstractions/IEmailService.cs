using Digestron.Domain;

namespace Digestron.Service.Abstractions;

public interface IEmailService
{
    Task HandleGetUnreadEmailCountAsync(MessageContext context, CancellationToken ct = default);
}

