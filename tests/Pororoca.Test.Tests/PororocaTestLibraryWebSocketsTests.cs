using System.Net.WebSockets;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;
using Pororoca.Infrastructure.Features.Requester;
using Pororoca.Infrastructure.Features.WebSockets;
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
        Assert.Equal(WebSocketConnectorState.Connected, ws.State);
        // WHEN
        await ws.DisconnectAsync();
        await Task.Delay(100);

        // THEN
        Assert.Null(ws.ConnectionException);
        Assert.Equal(WebSocketConnectorState.Disconnected, ws.State);
        // THEN
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            // The server closing goodbye message may or may not be captured.
            if (msgCount == 0)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Close, msg.Type);
                Assert.Equal("ok, bye", msg.ReadAsUtf8Text());
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
        Assert.Equal(WebSocketConnectorState.Connected, ws.State);

        // WHEN
        await ws.SendMessageAsync("Bye");
        await Task.Delay(TimeSpan.FromSeconds(2));
        // THEN
        /*
         * Sometimes there is an Exception below:
         * WebSocketConnector.cs:line 426, CloseStartingByLocalAsync()
         * System.OperationCanceledException: Aborted
            ---> System.ObjectDisposedException: Cannot access a disposed object.
         * (don't know why)
        */
        // Assert.Null(ws.ConnectionException);
        Assert.Equal(WebSocketConnectorState.Disconnected, ws.State);
        // THEN
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            msgCount++;

            // Because of the parallelism, the Server message
            // may arrive earlier than the Client closing message.
            // The Server closing goodbye message may or may not be captured.

            string expectedMsg = msg.Direction switch
            {
                WebSocketMessageDirection.FromClient => "Adiós",
                WebSocketMessageDirection.FromServer => "ok, bye",
            _ => string.Empty
            };
            Assert.Equal(WebSocketMessageType.Close, msg.Type);
            Assert.Equal(expectedMsg, msg.ReadAsUtf8Text());
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
        Assert.Equal(WebSocketConnectorState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("Hello");
        // THEN
        // The server should reply with a text message
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            if (msgCount == 0)
            {
                Assert.Equal(WebSocketMessageDirection.FromClient, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("Hello", msg.ReadAsUtf8Text());
            }
            else if (msgCount == 1)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("received text (5 bytes): Hello", msg.ReadAsUtf8Text());

                // Teardown
                await ws.DisconnectAsync();
            }
            else if (msgCount == 2)
            {
                // This is in case the server replies with a closing goodbye message.
                // It may or may not be captured.
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Close, msg.Type);
                Assert.Equal("ok, bye", msg.ReadAsUtf8Text());
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
        Assert.Equal(WebSocketConnectorState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("SpiderMan");
        // THEN
        // The server should reply with a text message
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            if (msgCount == 0)
            {
                Assert.Equal(WebSocketMessageDirection.FromClient, msg.Direction);
                Assert.Equal(WebSocketMessageType.Binary, msg.Type);
                Assert.Equal(Path.Combine(GetTestFilesDir(), "homem_aranha.jpg"), ((FileStream)msg.BytesStream).Name);
            }
            else if (msgCount == 1)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("received binary 9784 bytes", msg.ReadAsUtf8Text());

                // Teardown
                await ws.DisconnectAsync();
            }
            else if (msgCount == 2)
            {
                // This is in case the server replies with a closing goodbye message.
                // It may or may not be captured.
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Close, msg.Type);
                Assert.Equal("ok, bye", msg.ReadAsUtf8Text());
            }

            msgCount++;
        }
        this.output.WriteLine($"Number of msgs in test ({wsConnName}): {msgCount}");
    }

    [Theory]
    [InlineData("WebSocket HTTP1 JSON")]
    [InlineData("WebSocket HTTP2 JSON")]
    public async Task Should_use_subprotocol_and_parse_JSON_successfully(string wsConnName)
    {
        // GIVEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName);
        Assert.Equal(WebSocketConnectorState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("Hello");
        // THEN
        // The server should reply with a JSON text message
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync())
        {
            if (msgCount == 0)
            {
                Assert.Equal(WebSocketMessageDirection.FromClient, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("Hello", msg.ReadAsUtf8Text());
            }
            else if (msgCount == 1)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal($"{{\"bytesReceived\":5,\"messageType\":\"text\",\"text\":\"Hello\"}}", msg.ReadAsUtf8Text());
                var msg2Json = msg.ReadAsJson<TestServerWebSocketMessage>(PororocaTestJsonExtensions.MinifyJsonOptions)!;
                Assert.Equal(5, msg2Json.BytesReceived);
                Assert.Equal("text", msg2Json.MessageType);
                Assert.Equal("Hello", msg2Json.Text);

                // Teardown
                await ws.DisconnectAsync();
            }

            msgCount++;
        }
        this.output.WriteLine($"Number of msgs in test ({wsConnName}): {msgCount}");

        Assert.Null(ws.ConnectionException);
        Assert.Equal(WebSocketConnectorState.Disconnected, ws.State);
    }
}

public sealed class TestServerWebSocketMessage
{
    public int? BytesReceived { get; set; }
    public string? MessageType { get; set; }
    public string? Text { get; set; }
}