using Xunit;
using System.Net;
using Pororoca.Infrastructure.Features.Requester;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;
using System.Text;
using static Pororoca.Test.Tests.AssertExtensions;

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

    [Fact]
    public async Task Should_connect_and_disconnect_successfully()
    {
        // GIVEN AND WHEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync("WebSocket HTTP1");
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

        AssertTypeAndCast<PororocaWebSocketServerMessage>(ws.ExchangedMessages[0], out var srvMsg);
        Assert.Equal(PororocaWebSocketMessageType.Close, srvMsg.MessageType);
        Assert.Equal("ok, bye", srvMsg.Text);
        Assert.Equal(DateTime.Now, srvMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Should_connect_and_disconnect_with_client_closing_message_successfully()
    {
        // GIVEN AND WHEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync("WebSocket HTTP1");
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

        AssertTypeAndCast<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0], out var msg);
        Assert.Equal(PororocaWebSocketMessageType.Close, msg.MessageType);
        Assert.Equal("Adiós", msg.Text);
        Assert.Equal(DateTime.Now, msg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Should_send_and_receive_text_messages_successfully()
    {
        // GIVEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync("WebSocket HTTP1");
        Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);
        // WHEN
        var waitForSendAndReply = Task.Delay(TimeSpan.FromSeconds(2));
        var sending = ws.SendMessageAsync("Hello").AsTask();
        await Task.WhenAll(waitForSendAndReply, sending);        
        // THEN
        // The server should reply with a text message
        Assert.Equal(2, ws.ExchangedMessages.Count);

        AssertTypeAndCast<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0], out var sentMsg);
        Assert.Equal(PororocaWebSocketMessageType.Text, sentMsg.MessageType);
        Assert.Equal("Hello", sentMsg.Text);
        Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));


        AssertTypeAndCast<PororocaWebSocketServerMessage>(ws.ExchangedMessages[1], out var replyMsg);
        Assert.Equal(PororocaWebSocketMessageType.Text, replyMsg.MessageType);
        Assert.Equal("received text (5 bytes): Hello", replyMsg.Text);
        Assert.Equal(DateTime.Now, replyMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
        
        // Teardown
        await ws.DisconnectAsync();
    }

    [Fact]
    public async Task Should_send_and_receive_binary_messages_successfully()
    {
        // GIVEN
        this.pororocaTest.SetEnvironmentVariable("Local", "TestFilesDir", GetTestFilesFolderPath());
        var ws = await this.pororocaTest.ConnectWebSocketAsync("WebSocket HTTP1");
        Assert.Equal(PororocaWebSocketConnectorState.Connected, ws.State);
        // WHEN
        var waitForSendAndReply = Task.Delay(TimeSpan.FromSeconds(2));
        var sending = ws.SendMessageAsync("SpiderMan").AsTask();
        await Task.WhenAll(waitForSendAndReply, sending);        
        // THEN
        // The server should reply with a text message
        Assert.Equal(2, ws.ExchangedMessages.Count);

        AssertTypeAndCast<PororocaWebSocketClientMessageToSend>(ws.ExchangedMessages[0], out var sentMsg);
        Assert.Equal(PororocaWebSocketMessageType.Binary, sentMsg.MessageType);
        Assert.Equal(Path.Combine(GetTestFilesFolderPath(), "homem_aranha.jpg"), ((FileStream)sentMsg.BytesStream).Name);
        Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));


        AssertTypeAndCast<PororocaWebSocketServerMessage>(ws.ExchangedMessages[1], out var replyMsg);
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