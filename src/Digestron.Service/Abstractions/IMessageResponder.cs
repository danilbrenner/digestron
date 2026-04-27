using Digestron.Domain;

namespace Digestron.Service.Abstractions;

public interface IMessageResponder
{
    Task SendStartMessageAsync(CommandContext context, CancellationToken ct);
    Task SendHelpMessageAsync(CommandContext context, CancellationToken ct);
    Task SendDigestLoadingMessageAsync(CommandContext context, CancellationToken ct);
    Task SendUnreadCountMessageAsync(CommandContext context, int count, CancellationToken ct);
    Task SendUnknownCommandMessageAsync(CommandContext context, CancellationToken ct);
    Task SendDeviceAuthenticationRequestAsync(CommandContext context, Uri verificationUri, string userCode, DateTimeOffset expiresOn, CancellationToken ct);
    Task SendDigestAsync(CommandContext context, string markdownText, int totalTokens, CancellationToken ct);
    Task SendPromptReloadedMessageAsync(CommandContext context, CancellationToken ct);
}

