using Digestron.Hosting.Handler;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Digestron.Tests.Hosting;

public class UpdateHandlerTests
{
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IDigestService> _digestService = new();
    private readonly Mock<IMessageResponder> _messageResponder = new();
    private readonly UpdateHandler _sut;

    public UpdateHandlerTests()
    {
        _sut = new UpdateHandler(
            _emailService.Object,
            _digestService.Object,
            _messageResponder.Object,
            Mock.Of<ILogger<UpdateHandler>>());
    }

    [Fact]
    public async Task HandleUpdateAsync_StartCommand_CallsSendStartMessage()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("/start"), default);

        _messageResponder.Verify(r => r.SendStartMessageAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_HelpCommand_CallsSendHelpMessage()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("/help"), default);

        _messageResponder.Verify(r => r.SendHelpMessageAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_DigestCommand_CallsHandleDigestAsync()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("/digest"), default);

        _emailService.Verify(s => s.HandleDigestAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_UnreadCommand_CallsEmailService()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("/unread"), default);

        _emailService.Verify(s => s.HandleGetUnreadEmailCountAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_UnknownCommand_CallsSendUnknownCommandMessage()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("/unknown"), default);

        _messageResponder.Verify(r => r.SendUnknownCommandMessageAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_PlainText_DoesNothing()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("hello"), default);

        _messageResponder.VerifyNoOtherCalls();
        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleUpdateAsync_UpdateWithoutMessage_DoesNothing()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), new Update(), default);

        _messageResponder.VerifyNoOtherCalls();
        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleUpdateAsync_MessageWithoutText_DoesNothing()
    {
        var update = new Update { Message = new Message { Id = 1, Date = DateTime.UtcNow, Chat = new Chat { Id = 1 } } };

        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), update, default);

        _messageResponder.VerifyNoOtherCalls();
        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleUpdateAsync_ReloadPromptCommand_CallsReloadPromptAndSendsConfirmation()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("/reloadprompt"), default);

        _digestService.Verify(s => s.ReloadPrompt(), Times.Once);
        _messageResponder.Verify(r => r.SendPromptReloadedMessageAsync(It.IsAny<CommandContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_UnknownCommand_DoesNotCallReloadPrompt()
    {
        await _sut.HandleUpdateAsync(Mock.Of<ITelegramBotClient>(), BuildTextUpdate("/unknown"), default);

        _digestService.Verify(s => s.ReloadPrompt(), Times.Never);
    }

    private static Update BuildTextUpdate(string text) => new()
    {
        Message = new Message
        {
            Id = 1,
            Date = DateTime.UtcNow,
            Chat = new Chat { Id = 123 },
            From = new User { Id = 42, FirstName = "Test" },
            Text = text
        }
    };
}
