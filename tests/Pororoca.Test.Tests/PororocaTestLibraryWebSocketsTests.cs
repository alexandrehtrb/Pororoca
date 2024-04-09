using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;
using Pororoca.Infrastructure.Features.Requester;
using Xunit;

namespace Pororoca.Test.Tests;

public sealed class PororocaTestLibraryWebSocketsTests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryWebSocketsTests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local")
                                        .AndDontCheckTlsCertificate();
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_connect_and_disconnect_successfully(string wsConnName)
    {
        // GIVEN AND WHEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName);
        // THEN
        Assert.NotNull(ws);
        Assert.Null(ws.ConnectionException);
        Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);
        // WHEN
        await ws.DisconnectAsync();

        // THEN
        Assert.Null(ws.ConnectionException);
        Assert.Equal(PororocaWebSocketConnectorState.Disconnected, ws.State);
        // THEN
        // The server should reply with a closing message
        await Task.Delay(200);
        Assert.Single(ws.ExchangedMessages);

        var srvMsg = Assert.IsType<PororocaWebSocketServerMessage>(ws.ExchangedMessages[0]);
        Assert.Equal(PororocaWebSocketMessageType.Close, srvMsg.MessageType);
        Assert.Equal("ok, bye", srvMsg.Text);
        Assert.Equal(DateTime.Now, srvMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_connect_and_disconnect_with_client_closing_message_successfully(string wsConnName)
    {
        // GIVEN AND WHEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName);
        // THEN
        Assert.NotNull(ws);
        Assert.Null(ws.ConnectionException);
        Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);

        // WHEN
        await ws.SendMessageAsync("Bye");
        // THEN
        Assert.Null(ws.ConnectionException);
        Assert.Equal(PororocaWebSocketConnectorState.Disconnected, ws.State);
        // THEN
        // The server should not reply
        Assert.Single(ws.ExchangedMessages);

        var msg = Assert.IsType<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0]);
        Assert.Equal(PororocaWebSocketMessageType.Close, msg.MessageType);
        Assert.Equal("Adiós", msg.Text);
        Assert.Equal(DateTime.Now, msg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_send_and_receive_text_messages_successfully(string wsConnName)
    {
        // GIVEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName);
        Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("Hello", waitingTimeInSeconds: 2.0f);
        // THEN
        // The server should reply with a text message
        Assert.Equal(2, ws.ExchangedMessages.Count);

        var sentMsg = Assert.IsType<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0]);
        Assert.Equal(PororocaWebSocketMessageType.Text, sentMsg.MessageType);
        Assert.Equal("Hello", sentMsg.Text);
        Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));

        var replyMsg = Assert.IsType<PororocaWebSocketServerMessage>(ws.ExchangedMessages[1]);
        Assert.Equal(PororocaWebSocketMessageType.Text, replyMsg.MessageType);
        Assert.Equal("received text (5 bytes): Hello", replyMsg.Text);
        Assert.Equal(DateTime.Now, replyMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));

        // Teardown
        await ws.DisconnectAsync();
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_send_and_receive_binary_messages_successfully(string wsConnName)
    {
        // GIVEN
        this.pororocaTest.SetEnvironmentVariable("Local", "TestFilesDir", GetTestFilesDir());
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName);
        Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("SpiderMan", waitingTimeInSeconds: 2.0f);
        // THEN
        // The server should reply with a text message
        Assert.Equal(2, ws.ExchangedMessages.Count);

        var sentMsg = Assert.IsType<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0]);
        Assert.Equal(PororocaWebSocketMessageType.Binary, sentMsg.MessageType);
        Assert.Equal(Path.Combine(GetTestFilesDir(), "homem_aranha.jpg"), ((FileStream)sentMsg.BytesStream).Name);
        Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));

        var replyMsg = Assert.IsType<PororocaWebSocketServerMessage>(ws.ExchangedMessages[1]);
        Assert.Equal(PororocaWebSocketMessageType.Text, replyMsg.MessageType);
        Assert.Equal("received binary 9784 bytes", replyMsg.Text);
        Assert.Equal(DateTime.Now, replyMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));

        // Teardown
        await ws.DisconnectAsync();
    }
}