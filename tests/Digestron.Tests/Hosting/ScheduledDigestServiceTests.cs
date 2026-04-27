using Digestron.Domain;
using Digestron.Hosting;
using Digestron.Infra.Options;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Digestron.Tests.Hosting;

public class ScheduledDigestServiceTests
{
    private readonly Mock<IEmailProvider> _emailProvider = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly ILogger<ScheduledDigestService> _logger = Mock.Of<ILogger<ScheduledDigestService>>();

    private ScheduledDigestService CreateSut(string[]? deliveryTimes = null)
    {
        var options = Options.Create(new ScheduleOptions
        {
            DeliveryTimesUtc = deliveryTimes ?? ["08:00", "18:00"]
        });

        return new ScheduledDigestService(_emailProvider.Object, _emailService.Object, options, _logger);
    }

    [Fact]
    public async Task DeliverToAllChatsAsync_CallsGetAuthenticatedChatIds()
    {
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns([]);

        var sut = CreateSut();
        await sut.DeliverToAllChatsAsync(CancellationToken.None);

        _emailProvider.Verify(p => p.GetAuthenticatedChatIds(), Times.Once);
    }

    [Fact]
    public async Task DeliverToAllChatsAsync_CallsHandleDigestAsyncOncePerChat()
    {
        var chatIds = new long[] { 1001L, 1002L, 1003L };
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns(chatIds);
        _emailService.Setup(s => s.HandleDigestAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.DeliverToAllChatsAsync(CancellationToken.None);

        foreach (var chatId in chatIds)
        {
            var id = chatId;
            _emailService.Verify(
                s => s.HandleDigestAsync(It.Is<CommandContext>(ctx => ctx.ChatId == id), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task DeliverToAllChatsAsync_ExceptionOnOneChat_ContinuesToOtherChats()
    {
        var chatIds = new long[] { 1001L, 1002L, 1003L };
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns(chatIds);

        _emailService
            .Setup(s => s.HandleDigestAsync(It.Is<CommandContext>(ctx => ctx.ChatId == 1002L), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Graph error"));
        _emailService
            .Setup(s => s.HandleDigestAsync(It.Is<CommandContext>(ctx => ctx.ChatId != 1002L), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.DeliverToAllChatsAsync(CancellationToken.None);

        _emailService.Verify(
            s => s.HandleDigestAsync(It.Is<CommandContext>(ctx => ctx.ChatId == 1001L), It.IsAny<CancellationToken>()),
            Times.Once);
        _emailService.Verify(
            s => s.HandleDigestAsync(It.Is<CommandContext>(ctx => ctx.ChatId == 1003L), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeliverToAllChatsAsync_NoChatIds_DoesNotCallHandleDigestAsync()
    {
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns(Array.Empty<long>());

        var sut = CreateSut();
        await sut.DeliverToAllChatsAsync(CancellationToken.None);

        _emailService.VerifyNoOtherCalls();
    }
}
