using System.Net.WebSockets;
using Pororoca.Infrastructure.Features.WebSockets;
using Xunit;

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
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName, cancellationToken: TestContext.Current.CancellationToken);
        // THEN
        Assert.NotNull(ws);
        Assert.Null(ws.ConnectionException);
        Assert.Equal(WebSocketConnectionState.Connected, ws.State);
        // WHEN
        await ws.DisconnectAsync(TestContext.Current.CancellationToken);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        // THEN
        Assert.Equal(WebSocketConnectionState.Disconnected, ws.State);
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            msgCount++;
        }
        Assert.Equal(0, msgCount);
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_connect_and_disconnect_with_client_closing_message_successfully(string wsConnName)
    {
        // GIVEN AND WHEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName, cancellationToken: TestContext.Current.CancellationToken);
        // THEN
        Assert.NotNull(ws);
        Assert.Null(ws.ConnectionException);
        Assert.Equal(WebSocketConnectionState.Connected, ws.State);

        // WHEN
        await ws.SendMessageAsync("Bye");
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        // THEN
        Assert.Equal(WebSocketConnectionState.Disconnected, ws.State);
        // THEN
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            msgCount++;
            Assert.Equal(WebSocketMessageDirection.FromClient, msg.Direction);
            Assert.Equal(WebSocketMessageType.Close, msg.Type);
            Assert.Equal("Adiós", msg.ReadAsUtf8Text());
        }
        Assert.Equal(1, msgCount);
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_send_and_receive_text_messages_successfully(string wsConnName)
    {
        // GIVEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(WebSocketConnectionState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("Hello");
        // THEN
        // The server should reply with a text message
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            msgCount++;
            if (msgCount == 1)
            {
                Assert.Equal(WebSocketMessageDirection.FromClient, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("Hello", msg.ReadAsUtf8Text());
            }
            else if (msgCount == 2)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("received text (5 bytes): Hello", msg.ReadAsUtf8Text());

                // Teardown
                await ws.DisconnectAsync(TestContext.Current.CancellationToken);
            }
        }
        Assert.Equal(2, msgCount);
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_optionally_collect_only_server_side_messages(string wsConnName)
    {
        // GIVEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName, collectOnlyServerSideMessages: true, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(WebSocketConnectionState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("Hello");
        // THEN
        // The server should reply with a text message
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            msgCount++;
            if (msgCount == 1)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("received text (5 bytes): Hello", msg.ReadAsUtf8Text());

                // Teardown
                await ws.DisconnectAsync(TestContext.Current.CancellationToken);
            }
        }
        Assert.Equal(1, msgCount);
    }

    [Theory]
    [InlineData("WebSocket HTTP1")]
    [InlineData("WebSocket HTTP2")]
    public async Task Should_send_and_receive_binary_messages_successfully(string wsConnName)
    {
        // GIVEN
        this.pororocaTest.SetEnvironmentVariable("Local", "TestFilesDir", GetTestFilesDir());
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(WebSocketConnectionState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("SpiderMan");
        // THEN
        // The server should reply with a text message
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            msgCount++;
            if (msgCount == 1)
            {
                Assert.Equal(WebSocketMessageDirection.FromClient, msg.Direction);
                Assert.Equal(WebSocketMessageType.Binary, msg.Type);
                //Assert.Equal(Path.Combine(GetTestFilesDir(), "homem_aranha.jpg"), ((FileStream)msg.BytesStream).Name);
            }
            else if (msgCount == 2)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("received binary 9784 bytes", msg.ReadAsUtf8Text());

                // Teardown
                await ws.DisconnectAsync(TestContext.Current.CancellationToken);
            }
        }
        Assert.Equal(2, msgCount);
    }

    [Theory]
    [InlineData("WebSocket HTTP1 JSON")]
    [InlineData("WebSocket HTTP2 JSON")]
    public async Task Should_use_subprotocol_and_parse_JSON_successfully(string wsConnName)
    {
        // GIVEN
        var ws = await this.pororocaTest.ConnectWebSocketAsync(wsConnName, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(WebSocketConnectionState.Connected, ws.State);
        // WHEN
        await ws.SendMessageAsync("Hello");
        // THEN
        // The server should reply with a JSON text message
        int msgCount = 0;
        await foreach (var msg in ws.ExchangedMessagesCollector!.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            msgCount++;
            if (msgCount == 1)
            {
                Assert.Equal(WebSocketMessageDirection.FromClient, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal("Hello", msg.ReadAsUtf8Text());
            }
            else if (msgCount == 2)
            {
                Assert.Equal(WebSocketMessageDirection.FromServer, msg.Direction);
                Assert.Equal(WebSocketMessageType.Text, msg.Type);
                Assert.Equal($"{{\"bytesReceived\":5,\"messageType\":\"text\",\"text\":\"Hello\"}}", msg.ReadAsUtf8Text());
                var msg2Json = msg.ReadAsUtf8Json<TestServerWebSocketMessage>(PororocaTestJsonExtensions.MinifyJsonOptions)!;
                Assert.Equal(5, msg2Json.BytesReceived);
                Assert.Equal("text", msg2Json.MessageType);
                Assert.Equal("Hello", msg2Json.Text);

                // Teardown
                await ws.DisconnectAsync(TestContext.Current.CancellationToken);
            }
        }
        Assert.Equal(WebSocketConnectionState.Disconnected, ws.State);
        Assert.Equal(2, msgCount);
    }
}

public sealed class TestServerWebSocketMessage
{
    public int? BytesReceived { get; set; }
    public string? MessageType { get; set; }
    public string? Text { get; set; }
}