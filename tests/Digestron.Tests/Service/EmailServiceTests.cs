using Digestron.Service.Abstractions;
using Digestron.Service.Services;
using Microsoft.Extensions.Logging;

namespace Digestron.Tests.Service;

public class EmailServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IEmailProvider> _emailProvider = new();
    private readonly Mock<IMessageResponder> _messageResponder = new();
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _sut = new EmailService(
            Mock.Of<ILogger<EmailService>>(),
            _messageResponder.Object,
            _emailProvider.Object);
    }

    [Fact]
    public async Task HandleGetUnreadEmailCountAsync_WithEmails_SendsCorrectCount()
    {
        var context = BuildContext();
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
        var context = BuildContext();

        _emailProvider
            .Setup(p => p.GetUnreadEmailsAsync(context, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.HandleGetUnreadEmailCountAsync(context);

        _messageResponder.Verify(
            r => r.SendUnreadCountMessageAsync(context, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private MessageContext BuildContext() => new()
    {
        ChatId = _fixture.Create<long>(),
        UserId = _fixture.Create<long>(),
        UserName = _fixture.Create<string>(),
        Content = new CommandMessageContent("/unread")
    };
}
