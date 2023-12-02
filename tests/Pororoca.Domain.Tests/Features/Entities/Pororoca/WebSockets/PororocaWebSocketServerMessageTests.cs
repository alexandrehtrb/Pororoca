using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.WebSockets;

public static class PororocaWebSocketServerMessageTests
{
    private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

    [Fact]
    public static void Should_construct_ws_server_binary_msg_correctly()
    {
        // GIVEN
        byte[] bytes = "hello"u8.ToArray();

        // WHEN
        PororocaWebSocketServerMessage msg = new(PororocaWebSocketMessageType.Binary, bytes);

        // THEN
        Assert.Equal(PororocaWebSocketMessageDirection.FromServer, msg.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Binary, msg.MessageType);
        Assert.Equal(bytes, msg.Bytes);
        Assert.Null(msg.Text);
        Assert.Null(msg.TextSyntax);
        Assert.Equal(DateTimeOffset.UtcNow, (DateTimeOffset)msg.ReceivedAtUtc!, oneSecond);
    }

    [Fact]
    public static void Should_construct_ws_server_closing_msg_correctly()
    {
        // GIVEN
        byte[] bytes = "ok bye"u8.ToArray();

        // WHEN
        PororocaWebSocketServerMessage msg = new("ok bye");

        // THEN
        Assert.Equal(PororocaWebSocketMessageDirection.FromServer, msg.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Close, msg.MessageType);
        Assert.Equal(bytes, msg.Bytes);
        Assert.Equal("ok bye", msg.Text);
        Assert.Equal(PororocaWebSocketMessageRawContentSyntax.Other, msg.TextSyntax);
        Assert.Equal(DateTimeOffset.UtcNow, (DateTimeOffset)msg.ReceivedAtUtc!, oneSecond);
    }

    [Fact]
    public static void Should_construct_ws_server_general_text_msg_correctly()
    {
        // GIVEN
        byte[] bytes = "hey man"u8.ToArray();

        // WHEN
        PororocaWebSocketServerMessage msg = new(PororocaWebSocketMessageType.Text, bytes);

        // THEN
        Assert.Equal(PororocaWebSocketMessageDirection.FromServer, msg.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Text, msg.MessageType);
        Assert.Equal(bytes, msg.Bytes);
        Assert.Equal("hey man", msg.Text);
        Assert.Equal(PororocaWebSocketMessageRawContentSyntax.Other, msg.TextSyntax);
        Assert.Equal(DateTimeOffset.UtcNow, (DateTimeOffset)msg.ReceivedAtUtc!, oneSecond);
    }

    [Fact]
    public static void Should_construct_ws_server_json_text_msg_correctly()
    {
        // GIVEN
        byte[] bytes = "[]"u8.ToArray();

        // WHEN
        PororocaWebSocketServerMessage msg = new(PororocaWebSocketMessageType.Text, bytes);

        // THEN
        Assert.Equal(PororocaWebSocketMessageDirection.FromServer, msg.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Text, msg.MessageType);
        Assert.Equal(bytes, msg.Bytes);
        Assert.Equal("[]", msg.Text);
        Assert.Equal(PororocaWebSocketMessageRawContentSyntax.Json, msg.TextSyntax);
        Assert.Equal(DateTimeOffset.UtcNow, (DateTimeOffset)msg.ReceivedAtUtc!, oneSecond);
    }
}