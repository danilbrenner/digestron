using Digestron.Domain;
using Digestron.Service.Abstractions;
using Digestron.Service.Services;
using Microsoft.Extensions.Logging;

namespace Digestron.Tests.Service;

public class EmailServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IEmailProvider> _emailProvider = new();
    private readonly Mock<IMessageResponder> _messageResponder = new();
    private readonly Mock<IDigestService> _digestService = new();
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _sut = new EmailService(
            Mock.Of<ILogger<EmailService>>(),
            _messageResponder.Object,
            _emailProvider.Object,
            _digestService.Object);
    }

    [Fact]
    public async Task HandleGetUnreadEmailCountAsync_WithEmails_SendsCorrectCount()
    {
        var context = BuildContext("/unread");
        var emails = _fixture.CreateMany<EmailMessage>(3).ToList();

        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(context, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emails);

        await _sut.HandleGetUnreadEmailCountAsync(context);

        _messageResponder.Verify(
            r => r.SendUnreadCountMessageAsync(context, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleGetUnreadEmailCountAsync_EmptyList_SendsZeroCount()
    {
        var context = BuildContext("/unread");

        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(context, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.HandleGetUnreadEmailCountAsync(context);

        _messageResponder.Verify(
            r => r.SendUnreadCountMessageAsync(context, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleDigestAsync_WithEmails_SendsDigestText()
    {
        var context = BuildContext("/digest");
        var emails = _fixture.CreateMany<EmailMessage>(3).ToList();
        var digestResult = new DigestResult("*Digest*", [emails[0].Id]);
        const int fakeMessageId = 42;

        _messageResponder
            .Setup(r => r.SendDigestLoadingMessageAsync(context, It.IsAny<CancellationToken>()))
            .Callback(() => context.ResponseMessageId = fakeMessageId)
            .Returns(Task.CompletedTask);
        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(context, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emails);
        _digestService
            .Setup(d => d.GenerateDigestAsync(emails, It.IsAny<CancellationToken>()))
            .ReturnsAsync(digestResult);

        await _sut.HandleDigestAsync(context);

        _messageResponder.Verify(
            r => r.SendDigestLoadingMessageAsync(context, It.IsAny<CancellationToken>()),
            Times.Once);
        _messageResponder.Verify(
            r => r.SendDigestAsync(context, digestResult.MarkdownText, digestResult.TotalTokens, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleDigestAsync_NoEmails_SendsEmptyInboxMessage()
    {
        var context = BuildContext("/digest");
        const int fakeMessageId = 99;

        _messageResponder
            .Setup(r => r.SendDigestLoadingMessageAsync(context, It.IsAny<CancellationToken>()))
            .Callback(() => context.ResponseMessageId = fakeMessageId)
            .Returns(Task.CompletedTask);
        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(context, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.HandleDigestAsync(context);

        _digestService.VerifyNoOtherCalls();
        _messageResponder.Verify(
            r => r.SendDigestAsync(context, It.IsAny<string>(), 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private CommandContext BuildContext(string command) => new()
    {
        ChatId = _fixture.Create<long>(),
        UserId = _fixture.Create<long>(),
        UserName = _fixture.Create<string>(),
        Content = new CommandMessageContent(command)
    };

    [Fact]
    public async Task HandleDigestToAllAsync_CallsGetAuthenticatedChatIds()
    {
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns([]);

        await _sut.HandleDigestToAllAsync();

        _emailProvider.Verify(p => p.GetAuthenticatedChatIds(), Times.Once);
    }

    [Fact]
    public async Task HandleDigestToAllAsync_SendsDigestToEachAuthenticatedChat()
    {
        var chatIds = new long[] { 1001L, 1002L };
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns(chatIds);
        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(It.IsAny<CommandContext>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.HandleDigestToAllAsync();

        foreach (var chatId in chatIds)
        {
            var id = chatId;
            _messageResponder.Verify(
                r => r.SendDigestLoadingMessageAsync(It.Is<CommandContext>(ctx => ctx.ChatId == id), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task HandleDigestToAllAsync_ExceptionOnOneChat_ContinuesToOtherChats()
    {
        var chatIds = new long[] { 1001L, 1002L, 1003L };
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns(chatIds);
        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(It.Is<CommandContext>(ctx => ctx.ChatId == 1002L), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Graph error"));
        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(It.Is<CommandContext>(ctx => ctx.ChatId != 1002L), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.HandleDigestToAllAsync();

        _messageResponder.Verify(
            r => r.SendDigestLoadingMessageAsync(It.Is<CommandContext>(ctx => ctx.ChatId == 1001L), It.IsAny<CancellationToken>()),
            Times.Once);
        _messageResponder.Verify(
            r => r.SendDigestLoadingMessageAsync(It.Is<CommandContext>(ctx => ctx.ChatId == 1003L), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleDigestToAllAsync_NoChatIds_DoesNotDeliverAnyDigest()
    {
        _emailProvider.Setup(p => p.GetAuthenticatedChatIds()).Returns(Array.Empty<long>());

        await _sut.HandleDigestToAllAsync();

        _messageResponder.Verify(
            r => r.SendDigestLoadingMessageAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
