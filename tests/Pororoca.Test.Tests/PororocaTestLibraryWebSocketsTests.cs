using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;
using Pororoca.Infrastructure.Features.Requester;
using Xunit;
using Xunit.Abstractions;

namespace Pororoca.Test.Tests;

public sealed class PororocaTestLibraryWebSocketsTests
{
    private readonly ITestOutputHelper output;
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryWebSocketsTests(ITestOutputHelper output)
    {
        this.output = output;
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
        await Task.Delay(100);

        // THEN
        Assert.Null(ws.ConnectionException);
        Assert.Equal(PororocaWebSocketConnectorState.Disconnected, ws.State);
        // THEN
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            // The server closing goodbye message may or may not be captured.
            if (msgCount == 0)
            {
                var srvMsg = Assert.IsType<PororocaWebSocketServerMessage>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Close, srvMsg.MessageType);
                Assert.Equal("ok, bye", srvMsg.Text);
                Assert.Equal(DateTime.Now, srvMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
            }

            msgCount++;
        }
        this.output.WriteLine($"Number of msgs in test ({wsConnName}): {msgCount}");
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
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            if (msgCount == 0)
            {
                var sentMsg = Assert.IsType<PororocaWebSocketClientMessageToSend>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Close, sentMsg.MessageType);
                Assert.Equal("Adiós", sentMsg.Text);
                Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(2));
            }
            // The server closing goodbye message may or may not be captured.
            else if (msgCount == 1)
            {
                var srvMsg = Assert.IsType<PororocaWebSocketServerMessage>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Close, srvMsg.MessageType);
                Assert.Equal("ok, bye", srvMsg.Text);
                Assert.Equal(DateTime.Now, srvMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
            }

            msgCount++;
        }
        this.output.WriteLine($"Number of msgs in test ({wsConnName}): {msgCount}");
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
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            if (msgCount == 0)
            {
                var sentMsg = Assert.IsType<PororocaWebSocketClientMessageToSend>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Text, sentMsg.MessageType);
                Assert.Equal("Hello", sentMsg.Text);
                Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
            }
            else if (msgCount == 1)
            {
                var replyMsg = Assert.IsType<PororocaWebSocketServerMessage>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Text, replyMsg.MessageType);
                Assert.Equal("received text (5 bytes): Hello", replyMsg.Text);
                Assert.Equal(DateTime.Now, replyMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));

                // Teardown
                await ws.DisconnectAsync();
            }
            else if (msgCount == 2)
            {
                // This is in case the server replies with a closing goodbye message.
                // It may or may not be captured.
                var srvByeMsg = Assert.IsType<PororocaWebSocketServerMessage>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Close, srvByeMsg.MessageType);
                Assert.Equal("ok, bye", srvByeMsg.Text);
                Assert.Equal(DateTime.Now, srvByeMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
            }

            msgCount++;
        }
        this.output.WriteLine($"Number of msgs in test ({wsConnName}): {msgCount}");
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
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            if (msgCount == 0)
            {
                var sentMsg = Assert.IsType<PororocaWebSocketClientMessageToSend>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Binary, sentMsg.MessageType);
                Assert.Equal(Path.Combine(GetTestFilesDir(), "homem_aranha.jpg"), ((FileStream)sentMsg.BytesStream).Name);
                Assert.Equal(DateTime.Now, sentMsg.SentAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
            }
            else if (msgCount == 1)
            {
                var replyMsg = Assert.IsType<PororocaWebSocketServerMessage>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Text, replyMsg.MessageType);
                Assert.Equal("received binary 9784 bytes", replyMsg.Text);
                Assert.Equal(DateTime.Now, replyMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));

                // Teardown
                await ws.DisconnectAsync();
            }
            else if (msgCount == 2)
            {
                // This is in case the server replies with a closing goodbye message.
                // It may or may not be captured.
                var srvByeMsg = Assert.IsType<PororocaWebSocketServerMessage>(msg);
                Assert.Equal(PororocaWebSocketMessageType.Close, srvByeMsg.MessageType);
                Assert.Equal("ok, bye", srvByeMsg.Text);
                Assert.Equal(DateTime.Now, srvByeMsg.ReceivedAtUtc.GetValueOrDefault().DateTime, TimeSpan.FromSeconds(3));
            }

            msgCount++;
        }
        this.output.WriteLine($"Number of msgs in test ({wsConnName}): {msgCount}");
    }
}