using Digestron.Hosting.Handler;
using Telegram.Bot.Types;

namespace Digestron.Tests.Hosting;

public class ParseUpdateTests
{
    [Fact]
    public void ParseUpdate_SlashText_ReturnsTrueWithCommandContent()
    {
        var update = BuildTextUpdate("/unread");

        var result = update.ParseUpdate(out var context);

        Assert.True(result);
        Assert.NotNull(context);
        Assert.IsType<CommandMessageContent>(context.Content);
    }

    [Fact]
    public void ParseUpdate_CommandIsCaseFolded()
    {
        var update = BuildTextUpdate("/UNREAD");

        update.ParseUpdate(out var context);

        Assert.Equal("/unread", ((CommandMessageContent)context!.Content).Command);
    }

    [Fact]
    public void ParseUpdate_PlainText_ReturnsTrueWithTextContent()
    {
        var update = BuildTextUpdate("hello world");

        var result = update.ParseUpdate(out var context);

        Assert.True(result);
        Assert.IsType<TextMessageContent>(context!.Content);
        Assert.Equal("hello world", ((TextMessageContent)context.Content).Text);
    }

    [Fact]
    public void ParseUpdate_ExtractsContextFromUpdate()
    {
        var update = BuildTextUpdate("/start", chatId: 100, userId: 200, username: "alice");

        update.ParseUpdate(out var context);

        Assert.Equal(100, context!.ChatId);
        Assert.Equal(200, context.UserId);
        Assert.Equal("alice", context.UserName);
    }

    [Fact]
    public void ParseUpdate_UpdateWithoutMessage_ReturnsFalse()
    {
        var result = new Update().ParseUpdate(out var context);

        Assert.False(result);
        Assert.Null(context);
    }

    [Fact]
    public void ParseUpdate_MessageWithoutText_ReturnsFalse()
    {
        var update = new Update
        {
            Message = new Message { Id = 1, Date = DateTime.UtcNow, Chat = new Chat { Id = 1 } }
        };

        var result = update.ParseUpdate(out _);

        Assert.False(result);
    }

    private static Update BuildTextUpdate(string text, long chatId = 1, long userId = 1, string username = "user") => new()
    {
        Message = new Message
        {
            Id = 1,
            Date = DateTime.UtcNow,
            Chat = new Chat { Id = chatId },
            From = new User { Id = userId, FirstName = username },
            Text = text
        }
    };
}
