using Digestron.Domain;

namespace Digestron.Service.Abstractions;

public interface IMessageResponder
{
    Task SendStartMessageAsync(MessageContext context, CancellationToken ct);
    Task SendHelpMessageAsync(MessageContext context, CancellationToken ct);
    Task SendDigestLoadingMessageAsync(MessageContext context, CancellationToken ct);
    Task SendUnreadCountMessageAsync(MessageContext context, int count, CancellationToken ct);
    Task SendUnknownCommandMessageAsync(MessageContext context, CancellationToken ct);
    Task SendDeviceAuthenticationRequestAsync(MessageContext context, Uri verificationUri, string userCode, DateTimeOffset expiresOn, CancellationToken ct);
    Task SendDigestAsync(MessageContext context, string markdownText, CancellationToken ct);
}

