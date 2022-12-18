using Xunit;
using Pororoca.Infrastructure.Features.Requester;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryWebSocketsHttp1Tests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryWebSocketsHttp1Tests()
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
        var waitForSend = Task.Delay(TimeSpan.FromSeconds(1));
        var sending = ws.SendMessageAsync("Bye").AsTask();
        await Task.WhenAll(waitForSend, sending);  
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
        var waitForSendAndReply = Task.Delay(TimeSpan.FromSeconds(2));
        var sending = ws.SendMessageAsync("Hello").AsTask();
        await Task.WhenAll(waitForSendAndReply, sending);        
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
        this.pororocaTest.SetEnvironmentVariable("Local", "TestFilesDir", GetTestFilesFolderPath());
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName);
        Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);
        // WHEN
        var waitForSendAndReply = Task.Delay(TimeSpan.FromSeconds(2));
        var sending = ws.SendMessageAsync("SpiderMan").AsTask();
        await Task.WhenAll(waitForSendAndReply, sending);        
        // THEN
        // The server should reply with a text message
        Assert.Equal(2, ws.ExchangedMessages.Count);

        var sentMsg = Assert.IsType<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0]);
        Assert.Equal(PororocaWebSocketMessageType.Binary, sentMsg.MessageType);
        Assert.Equal(Path.Combine(GetTestFilesFolderPath(), "homem_aranha.jpg"), ((FileStream)sentMsg.BytesStream).Name);
        Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));

        var replyMsg = Assert.IsType<PororocaWebSocketServerMessage>(ws.ExchangedMessages[1]);
        Assert.Equal(PororocaWebSocketMessageType.Text, replyMsg.MessageType);
        Assert.Equal("received binary 9784 bytes", replyMsg.Text);
        Assert.Equal(DateTime.Now, replyMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
        
        // Teardown
        await ws.DisconnectAsync();
    }

    private static string GetTestFilesFolderPath() =>
        Path.Combine(GetTestFolderPath(), "TestFiles");

    private static string GetTestCollectionFilePath() =>
        Path.Combine(GetTestFolderPath(), "PororocaIntegrationTestCollection.pororoca_collection.json");

    private static string GetTestFolderPath()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return testDataDirInfo.FullName;
    }
}