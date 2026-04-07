using System.Buffers;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;

namespace Pororoca.Infrastructure.Features.WebSockets;

// Source code taken from:
// https://github.com/alexandrehtrb/AlexandreHtrb.WebSocketExtensions
// 
// The MIT License(MIT)
// Copyright(c) 2026 Alexandre H.T.R. Bonfitto
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

public enum WebSocketConnectionState
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Disconnecting = 3
}

public abstract class WebSocketConnector
{
    // because Ethernet's MTU is around 1500 bytes
    public const int DefaultBufferSize = 1440;

    // 20 messages waiting to be send should be enough for most applications.
    private const int messagesToSendChannelCapacity = 20;

    protected abstract WebSocketMessageDirection DirectionFromThis { get; }

    private WebSocketMessageDirection OppositeDirection =>
        DirectionFromThis == WebSocketMessageDirection.FromClient ?
        WebSocketMessageDirection.FromServer :
        WebSocketMessageDirection.FromClient;

    public WebSocketConnectionState ConnectionState { get; protected set; }

    public Exception? ConnectionException { get; private set; }

    private ChannelWriter<WebSocketMessage>? exchangedMessagesCollectorWriter;

    public ChannelReader<WebSocketMessage>? ExchangedMessagesCollector { get; private set; }

    private Channel<WebSocketMessage>? MessagesToSendChannel { get; set; }

    private CancellationTokenSource? cancelSendingAndReceivingTokenSource;

    public Action<WebSocketConnectionState, Exception?>? OnConnectionChanged { get; set; }

    protected WebSocket? ws;

    private readonly bool collectOnlyReceivedMessages;
    private readonly int bufferSize;

    public WebSocketConnector(bool collectOnlyReceivedMessages, int bufferSize = DefaultBufferSize)
    {
        this.collectOnlyReceivedMessages = collectOnlyReceivedMessages;
        this.bufferSize = bufferSize;
        this.ConnectionState = WebSocketConnectionState.Disconnected;
    }

    #region STATE SETTERS

    protected void SetIsConnecting() =>
        SetChangedState(WebSocketConnectionState.Connecting, null);

    protected void SetIsConnected() =>
        SetChangedState(WebSocketConnectionState.Connected, null);

    protected void SetIsDisconnecting() =>
        SetChangedState(WebSocketConnectionState.Disconnecting, null);

    protected void SetIsDisconnected(Exception? ex) =>
        SetChangedState(WebSocketConnectionState.Disconnected, ex);

    private void SetChangedState(WebSocketConnectionState state, Exception? connectionException)
    {
        ConnectionState = state;
        ConnectionException = connectionException;
        OnConnectionChanged?.Invoke(ConnectionState, ConnectionException);
    }

    #endregion

    #region CONNECTION

    protected virtual void SetupAfterConnected(WebSocket ws)
    {
        this.ws = ws;
        ClearAfterConnect();
        MessagesToSendChannel = BuildMessagesToSendChannel();
        SetupExchangedMessagesCollectorChannel();

        this.cancelSendingAndReceivingTokenSource = new();
        StartMessagesExchangeInBackground(this.cancelSendingAndReceivingTokenSource.Token);
    }

    private void ClearAfterConnect()
    {
        ConnectionException = null;
        this.exchangedMessagesCollectorWriter = null;
        ExchangedMessagesCollector = null;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        ConnectionState == WebSocketConnectionState.Connected ?
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
            SingleWriter = true
        };
        var channel = Channel.CreateUnbounded<WebSocketMessage>(sendChannelOpts);
        this.exchangedMessagesCollectorWriter = channel.Writer;
        ExchangedMessagesCollector = channel.Reader;
    }

    #endregion

    #region MESSAGES EXCHANGE

    private void StartMessagesExchangeInBackground(CancellationToken disconnectToken) =>
        Task.Factory.StartNew(() => ProcessMessagesExchangesAsync(disconnectToken),
                              TaskCreationOptions.LongRunning);

    private async Task ProcessMessagesExchangesAsync(CancellationToken disconnectToken)
    {
        // This is a quasi-parallel communication,
        // because the shift between sending and receiving
        // is fast enough to avoid competition between those tasks.
        // Pros: one thread instead of two, less memory and CPU usage.
        // Cons: rare competition could (in theory?) happen
        // between intensive simultaneous sending and receiving.

        // We are using a channel as a queue here because two different messages
        // should not be sent at the same time through a WebSocket,
        // as the other party cannot distinguish which message part corresponds to which message.
        Task HasMessageToSendAsync() =>
            MessagesToSendChannel!.Reader.WaitToReadAsync(disconnectToken).AsTask();

        Task HasMessageToReceiveAsync() =>
            this.ws!.ReceiveAsync(Memory<byte>.Empty, disconnectToken).AsTask();

        bool IsRemoteClosingConnection() =>
            this.ws?.State == WebSocketState.Aborted || this.ws?.State == WebSocketState.CloseReceived;

        bool CanSendMessages() =>
            (this.ws?.State == WebSocketState.Open || this.ws?.State == WebSocketState.Connecting || this.ws?.State == WebSocketState.CloseReceived)
         && MessagesToSendChannel is not null;

        bool CanReceiveMessages() =>
            this.ws?.State == WebSocketState.Open || this.ws?.State == WebSocketState.Connecting || this.ws?.State == WebSocketState.CloseSent;

        ValueTask<WebSocketMessage> DequeueMessageToSendAsync() =>
            MessagesToSendChannel!.Reader.ReadAsync(CancellationToken.None);

        while (CanSendMessages() || CanReceiveMessages())
        {
            var beganSending = HasMessageToSendAsync();
            var beganReceiving = HasMessageToReceiveAsync();
            var firstOperation = await Task.WhenAny(beganSending, beganReceiving);

            if (disconnectToken.IsCancellationRequested)
            {
                // This means ClearAfterClosure() has
                // been called, after CloseByLocal().
                return; // exits the reception thread
            }
            else if (IsRemoteClosingConnection())
            {
                await FinishClosureStartedByRemoteAsync();
                return; // exits the reception thread
            }
            else if (firstOperation == beganSending && CanSendMessages())
            {
                var msgToSend = await DequeueMessageToSendAsync();
                await SendMessageAsync(msgToSend!, disconnectToken);
            }
            else if (firstOperation == beganReceiving && CanReceiveMessages())
            {
                await ReceiveMessageAsync(disconnectToken);
            }
        }

        if (IsRemoteClosingConnection())
        {
            await FinishClosureStartedByRemoteAsync();
            return; // exits the reception thread
        }
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

    public ValueTask SendMessageAsync<T>(WebSocketMessageType type, T tObj, JsonTypeInfo<T> jsonTypeInfo, bool disableCompression) =>
        EnqueueMessageToSendAsync(new(DirectionFromThis, type, JsonSerializer.SerializeToUtf8Bytes(tObj, jsonTypeInfo), disableCompression));

    private ValueTask EnqueueMessageToSendAsync(WebSocketMessage msg) =>
        // Not throwing exception if user tried to send message whilst WebSocket is not connected
        ConnectionState == WebSocketConnectionState.Connected && MessagesToSendChannel is not null ?
        MessagesToSendChannel!.Writer.WriteAsync(msg) :
        ValueTask.CompletedTask;

    private Task SendMessageAsync(WebSocketMessage msg, CancellationToken cancellationToken = default) =>
        msg.IsStreamBased ?
        SendStreamedMessageAsync(msg, cancellationToken) :
        SendByteArrayMessageAsync(msg, cancellationToken);

    private async Task SendByteArrayMessageAsync(WebSocketMessage msg, CancellationToken cancellationToken = default)
    {
        try
        {
            if (msg.Type == WebSocketMessageType.Close)
            {
                await CloseStartingByLocalAsync(msg, cancellationToken);
                return;
            }
            else
            {
                await this.ws!.SendAsync(msg.Bytes.AsMemory(), msg.Type, msg.DetermineFlags(), cancellationToken);
            }

            if (!this.collectOnlyReceivedMessages)
            {
                this.exchangedMessagesCollectorWriter?.TryWrite(msg);
            }
        }
        catch
        {
            // We will not attempt to close connection if an exception happens
        }
    }

    private async Task SendStreamedMessageAsync(WebSocketMessage msg, CancellationToken cancellationToken = default)
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
                msg.BytesStream!.Seek(0, SeekOrigin.Begin);

                while (!cancellationToken.IsCancellationRequested)
                {
                    int bufferBytesRead = await msg.BytesStream.ReadAsync(buffer.AsMemory(), cancellationToken);
                    if (bufferBytesRead == 0)
                    {
                        break;
                    }

                    await this.ws!.SendAsync(buffer.AsMemory(0, bufferBytesRead), msg.Type, msg.DetermineFlags(), cancellationToken);
                }
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = null;
                // Dispose() below is necessary for freeing FileStreams in case of file-based ws client messages.
                // If it's a MemoryStream, calling.ToArray() later won't cause exceptions.
                msg.BytesStream.Dispose();
            }

            if (!this.collectOnlyReceivedMessages && !cancellationToken.IsCancellationRequested && msg.ReachedEndOfStream())
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

    private async Task<WebSocketMessage?> ReceiveMessageAsync(CancellationToken disconnectToken)
    {
        MemoryStream accumulator = new();
        byte[]? buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        var bufferAsMemory = buffer.AsMemory();
        ValueWebSocketReceiveResult receivalResult;

        try
        {
            do
            {
                receivalResult = await this.ws!.ReceiveAsync(bufferAsMemory, disconnectToken);
                await accumulator.WriteAsync(bufferAsMemory.Slice(0, receivalResult.Count), disconnectToken);
            }
            while (IsReceivingMessage(receivalResult) && !disconnectToken.IsCancellationRequested);

            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;

            var msgType = receivalResult.MessageType;

            WebSocketMessage? msg = null;

            if (msgType == WebSocketMessageType.Close && !string.IsNullOrWhiteSpace(this.ws.CloseStatusDescription))
            {
                msg = new(OppositeDirection, msgType, this.ws.CloseStatusDescription!, false);
            }
            else if (accumulator.Length > 0)
            {
                accumulator.Seek(0, SeekOrigin.Begin);
                msg = new(OppositeDirection, msgType, accumulator, false);
            }

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

    /*
     From GitHub dotnet/runtime repo issue
     https://github.com/dotnet/runtime/issues/81762#issuecomment-1421047699

     Difference between ws.CloseAsync() and ws.CloseOutputAsync():

     CloseOutputAsync
      - Send a close frame
      - If a close frame has already been received, transition to a closed state and clean up / dispose.
     
     CloseAsync
      - Send a close frame if we haven't already sent one
      - Read all incoming messages until a close frame is received
      - Transition to a closed state and clean up / dispose

     In our WebSocketConnector, let's always use CloseOutputAsync(), 
     so the connector doesn't waste time waiting for remote's close response frame.
     */

    private async Task FinishClosureStartedByRemoteAsync()
    {
        Exception? closingException = null;
        try
        {
            SetIsDisconnecting();

            if (!string.IsNullOrWhiteSpace(this.ws?.CloseStatusDescription))
            {
                WebSocketMessage closingMsg = new(OppositeDirection, WebSocketMessageType.Close, this.ws!.CloseStatusDescription, false);
                this.exchangedMessagesCollectorWriter?.TryWrite(closingMsg);
            }

            // These are the valid states for calling client.CloseOutputAsync()
            // If the ws state is WebSocketState.CloseSent,
            // it means CloseStartingByLocalAsync() already has been called.
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
            // These are the valid states for calling client.CloseOutputAsync()
            // If the ws state is WebSocketState.CloseSent,
            // it means CloseStartingByLocalAsync() already has been called.
            if (this.ws?.State == WebSocketState.Open || this.ws?.State == WebSocketState.CloseReceived)
            {
                if (closingMsg is null)
                {
                    await this.ws!.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                }
                else
                {
                    await this.ws!.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, closingMsg.ReadAsUtf8Text(), cancellationToken);
                    if (!this.collectOnlyReceivedMessages)
                    {
                        this.exchangedMessagesCollectorWriter?.TryWrite(closingMsg);
                    }
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