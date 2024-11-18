using System.Buffers;
using System.Threading.Channels;
using System.Net.WebSockets;

namespace Pororoca.Infrastructure.Features.WebSockets;

public enum WebSocketConnectorState
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Disconnecting = 3
}

public abstract class WebSocketConnector
{
    private const int bufferSize = 1440; // because Ethernet's MTU is around 1500 bytes
    private const int messagesToSendChannelCapacity = 20;

    protected abstract WebSocketMessageDirection DirectionFromThis { get; }

    private WebSocketMessageDirection OppositeDirection =>
        DirectionFromThis == WebSocketMessageDirection.FromClient ?
        WebSocketMessageDirection.FromServer :
        WebSocketMessageDirection.FromClient;

    public WebSocketConnectorState ConnectionState { get; protected set; }

    public Exception? ConnectionException { get; private set; }

    private ChannelWriter<WebSocketMessage>? exchangedMessagesCollectorWriter;

    public ChannelReader<WebSocketMessage>? ExchangedMessagesCollector { get; private set; }

    private Channel<WebSocketMessage>? MessagesToSendChannel { get; set; }

    private CancellationTokenSource? cancelSendingAndReceivingTokenSource;

    public Action<WebSocketConnectorState, Exception?>? OnConnectionChanged { get; set; }

    protected WebSocket? ws;

    #region STATE SETTERS

    protected void SetIsConnecting() =>
        SetChangedState(WebSocketConnectorState.Connecting, null);

    protected void SetIsConnected() =>
        SetChangedState(WebSocketConnectorState.Connected, null);

    protected void SetIsDisconnecting() =>
        SetChangedState(WebSocketConnectorState.Disconnecting, null);

    protected void SetIsDisconnected(Exception? ex) =>
        SetChangedState(WebSocketConnectorState.Disconnected, ex);

    private void SetChangedState(WebSocketConnectorState state, Exception? connectionException)
    {
        ConnectionState = state;
        ConnectionException = connectionException;
        OnConnectionChanged?.Invoke(ConnectionState, ConnectionException);
    }

    #endregion

    #region CONNECTION

    protected void SetupAfterConnected(CancellationTokenSource cts)
    {
        ClearAfterConnect();
        MessagesToSendChannel = BuildMessagesToSendChannel();
        SetupExchangedMessagesCollectorChannel();

        this.cancelSendingAndReceivingTokenSource = cts;
        StartMessagesExchangeInBackground(cts.Token);
    }

    private void ClearAfterConnect()
    {
        ConnectionException = null;
        this.exchangedMessagesCollectorWriter = null;
        ExchangedMessagesCollector = null;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        ConnectionState == WebSocketConnectorState.Connected ?
        CloseStartingByLocalAsync(null, cancellationToken) :
        Task.CompletedTask;  // Not throwing exception if user tried to disconnect whilst WebSocket is not connected

    private void ClearAfterClosure()
    {
        this.ws?.Dispose();
        this.ws = null;
        MessagesToSendChannel?.Writer?.TryComplete();
        MessagesToSendChannel = null;
        // To cancel the emission and reception threads:
        this.cancelSendingAndReceivingTokenSource?.Cancel();
        // Then, to clear this cancellation token source:
        this.cancelSendingAndReceivingTokenSource = null;

        // Closing ExchangedMessagesCollector channel.
        // The reader will remain, so its messages can still be drained and read.
        this.exchangedMessagesCollectorWriter?.TryComplete();
    }

    private static Channel<WebSocketMessage> BuildMessagesToSendChannel()
    {
        BoundedChannelOptions sendChannelOpts = new(messagesToSendChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        return Channel.CreateBounded<WebSocketMessage>(sendChannelOpts);
    }

    private void SetupExchangedMessagesCollectorChannel()
    {
        UnboundedChannelOptions sendChannelOpts = new()
        {
            SingleReader = true,
            SingleWriter = false
        };
        var channel = Channel.CreateUnbounded<WebSocketMessage>(sendChannelOpts);
        this.exchangedMessagesCollectorWriter = channel.Writer;
        ExchangedMessagesCollector = channel.Reader;
    }

    #endregion

    #region MESSAGES EXCHANGE

    private void StartMessagesExchangeInBackground(CancellationToken disconnectToken)
    {
        // We can receive and send at the same time,
        // according to ManagedWebSocket class comment:
        // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.WebSockets/src/System/Net/WebSockets/ManagedWebSocket.cs

        // One thread will send outbound messages 
        // and another will receive inbound messages.

        // The "Receival" thread will take care of connection lifecycle,
        // such as finish remote closures and close starting by local.

        // The "Sender" thread will send messages, as long as:
        // 1) DisconnectToken not invoked;
        // 2) The MessagesToSendChannel is not completed;
        // 3) The inner WebSocket can send messages.
        // When the connection closure happens in the Receival thread,
        // conditions above will cease and Sender thread will finish too.

        _ = Task.Factory.StartNew(() => ProcessOutgoingMessagesAsync(disconnectToken),
                                  TaskCreationOptions.LongRunning);

        _ = Task.Factory.StartNew(() => ProcessIncomingMessagesAsync(disconnectToken),
                                  TaskCreationOptions.LongRunning);
    }

    #endregion

    #region SEND MESSAGES

    // They have the same name as the method below, but they enqueue in the channel;
    // they are not responsible for the real sending
    public ValueTask SendMessageAsync(WebSocketMessageType type, Stream bytesStream, bool disableCompression) =>
        EnqueueMessageToSendAsync(new(DirectionFromThis, type, bytesStream, disableCompression));

    public ValueTask SendMessageAsync(WebSocketMessageType type, byte[] bytes, bool disableCompression) =>
        EnqueueMessageToSendAsync(new(DirectionFromThis, type, bytes, disableCompression));

    public ValueTask SendMessageAsync(WebSocketMessageType type, string text, bool disableCompression) =>
        EnqueueMessageToSendAsync(new(DirectionFromThis, type, text, disableCompression));

    private ValueTask EnqueueMessageToSendAsync(WebSocketMessage msg) =>
        ConnectionState == WebSocketConnectorState.Connected && MessagesToSendChannel is not null ?
        MessagesToSendChannel!.Writer.WriteAsync(msg) :
        ValueTask.CompletedTask; // Not throwing exception if user tried to send message whilst WebSocket is not connected

    private async Task ProcessOutgoingMessagesAsync(CancellationToken disconnectToken)
    {
        bool CanSendMessages() =>
            (this.ws!.State == WebSocketState.Open || this.ws.State == WebSocketState.Connecting || this.ws.State == WebSocketState.CloseReceived)
         && MessagesToSendChannel is not null;

        // We are using a queue (channel) here because two different messages
        // should not be sent at the same time through a WebSocket,
        // as the other party cannot distinguish which message part corresponds to which message.
        await foreach (var msg in MessagesToSendChannel!.Reader.ReadAllAsync(disconnectToken))
        {
            if (CanSendMessages())
            {
                await SendMessageAsync(msg, disconnectToken);
            }
            else
            {
                return; // exits the reception thread
            }
        }
    }

    private async Task SendMessageAsync(WebSocketMessage msg, CancellationToken cancellationToken = default)
    {
        byte[]? buffer = null;
        try
        {
            if (msg.Type == WebSocketMessageType.Close)
            {
                await CloseStartingByLocalAsync(msg, cancellationToken);
                return;
            }
            else
            {
                buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                Array.Clear(buffer);
                msg.BytesStream.Seek(0, SeekOrigin.Begin);
#pragma warning disable CA1835
                while (!cancellationToken.IsCancellationRequested
                    && await msg.BytesStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken) > 0)
                {
#pragma warning restore CA1835
                    var trimmedBuffer = buffer.AsMemory().TrimEnd((byte)0);
                    var flags = msg.DetermineFlags();
                    await this.ws!.SendAsync(trimmedBuffer, msg.Type, flags, cancellationToken);
                    Array.Clear(buffer);
                }
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = null;
                // Dispose() below is necessary for freeing FileStreams in case of file-based ws client messages.
                // If it's a MemoryStream, calling.ToArray() later won't cause exceptions.
                msg.BytesStream.Dispose();
            }

            if (!cancellationToken.IsCancellationRequested && msg.ReachedEndOfStream())
            {
                this.exchangedMessagesCollectorWriter?.TryWrite(msg);
            }
        }
        catch
        {
            // We will not attempt to close connection if an exception happens
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    #endregion

    #region RECEIVE MESSAGES

    private async Task ProcessIncomingMessagesAsync(CancellationToken disconnectToken)
    {
        async Task<ValueWebSocketReceiveResult> HasMessageToReceiveAsync()
        {
            try
            {
                // This method NEEDS to be async Task instead of just Task,
                // elsewise, the CancellationToken will not work (I don't know why)
                return await this.ws!.ReceiveAsync(Memory<byte>.Empty, disconnectToken);
            }
            catch (OperationCanceledException)
            {
                return await ValueTask.FromResult(default(ValueWebSocketReceiveResult));
            }
        }

        async Task HasRemoteDisconnectingAsync()
        {
            try
            {
                while (!IsRemoteClosingConnection())
                {
                    // 200ms interval check
                    await Task.Delay(200, disconnectToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        bool IsRemoteClosingConnection() =>
            this.ws!.State == WebSocketState.Aborted || this.ws.State == WebSocketState.CloseReceived;

        bool CanReceiveMessages() =>
            this.ws!.State == WebSocketState.Open || this.ws.State == WebSocketState.Connecting || this.ws.State == WebSocketState.CloseSent;

        while (CanReceiveMessages())
        {
            var beganReceiving = HasMessageToReceiveAsync();
            var beganRemoteDisconnect = HasRemoteDisconnectingAsync();
            // TODO: Is there a better way to check for remote disconnect 
            // other than periodically check status?

            var task = await Task.WhenAny(beganReceiving, beganRemoteDisconnect);

            if (disconnectToken.IsCancellationRequested)
            {
                await CloseStartingByLocalAsync(null, CancellationToken.None);
                return; // exits the reception thread
            }
            else if (IsRemoteClosingConnection())
            {
                await FinishClosureStartedByRemoteAsync();
                return; // exits the reception thread
            }
            else if (task == beganReceiving)
            {
                bool isClosing = beganReceiving.Result.MessageType == WebSocketMessageType.Close;
                if (isClosing)
                {
                    // closure message already received                    
                    await FinishClosureStartedByRemoteAsync();
                    return; // exits the reception thread
                }
                else if (CanReceiveMessages())
                {
                    var receivedMsg = await ReceiveMessageAsync(disconnectToken);
                    if (receivedMsg?.Type == WebSocketMessageType.Close)
                    {
                        await FinishClosureStartedByRemoteAsync();
                        return; // exits the reception thread
                    }
                }
            }
        }

        if (IsRemoteClosingConnection())
        {
            await FinishClosureStartedByRemoteAsync();
            return; // exits the reception thread
        }
    }

    private async Task<WebSocketMessage?> ReceiveMessageAsync(CancellationToken disconnectToken)
    {
        using MemoryStream accumulator = new();
        byte[]? buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        Array.Clear(buffer);
        ValueWebSocketReceiveResult receivalResult;

        try
        {
            do
            {
                var mem = buffer.AsMemory();
                receivalResult = await this.ws!.ReceiveAsync(mem, disconnectToken);
                var trimmedBuffer = mem.TrimEnd((byte)0);
                await accumulator.WriteAsync(trimmedBuffer, disconnectToken);
                Array.Clear(buffer);
            }
            while (IsReceivingMessage(receivalResult) && !disconnectToken.IsCancellationRequested);

            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;

            var msgType = receivalResult.MessageType;
            WebSocketMessage? msg =
                (msgType == WebSocketMessageType.Close && !string.IsNullOrWhiteSpace(this.ws.CloseStatusDescription)) ?
                new(OppositeDirection, msgType, this.ws.CloseStatusDescription!, false) :
                accumulator.Length > 0 ?
                new(OppositeDirection, msgType, accumulator.ToArray(), false) :
                null;

            if (msg is not null)
            {
                this.exchangedMessagesCollectorWriter?.TryWrite(msg);
            }
            return msg;
        }
        catch
        {
            // We will not attempt to close connection if an exception happens
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            return null;
        }
    }

    private static bool IsReceivingMessage(ValueWebSocketReceiveResult vwsrr) =>
        vwsrr.MessageType != WebSocketMessageType.Close
        && vwsrr.EndOfMessage == false;

    private async Task FinishClosureStartedByRemoteAsync()
    {
        Exception? closingException = null;
        try
        {
            SetIsDisconnecting();

            if (!string.IsNullOrWhiteSpace(this.ws!.CloseStatusDescription))
            {
                WebSocketMessage closingMsg = new(OppositeDirection, WebSocketMessageType.Close, this.ws.CloseStatusDescription, false);
                this.exchangedMessagesCollectorWriter?.TryWrite(closingMsg);
            }

            // These are the valid states for calling client.CloseOutputAsync()
            if (this.ws?.State == WebSocketState.Open || this.ws?.State == WebSocketState.CloseReceived)
            {
                await this.ws!.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, default);
            }
        }
        catch (Exception ex)
        {
            closingException = ex;
        }
        finally
        {
            ClearAfterClosure();
            SetIsDisconnected(closingException);
        }
    }

    protected async Task CloseStartingByLocalAsync(WebSocketMessage? closingMsg, CancellationToken cancellationToken = default)
    {
        Exception? closingException = null;
        try
        {
            SetIsDisconnecting();
            // These are the valid states for calling client.CloseAsync()
            if (this.ws?.State == WebSocketState.Open || this.ws?.State == WebSocketState.CloseReceived || this.ws?.State == WebSocketState.CloseSent)
            {
                if (closingMsg is null)
                {
                    await this.ws!.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                }
                else
                {
                    await this.ws!.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, closingMsg.ReadAsUtf8Text(), cancellationToken);
                    this.exchangedMessagesCollectorWriter?.TryWrite(closingMsg);
                }
            }
        }
        catch (Exception ex)
        {
            closingException = ex;
        }
        finally
        {
            ClearAfterClosure();
            SetIsDisconnected(closingException);
        }
    }

    #endregion

}