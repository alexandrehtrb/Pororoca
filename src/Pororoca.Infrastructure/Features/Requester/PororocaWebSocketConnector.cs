using System.Buffers;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Threading.Channels;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

namespace Pororoca.Infrastructure.Features.Requester;

public delegate void OnWebSocketConnectionChanged(PororocaWebSocketConnectorState state, Exception? connectionException);
public delegate void OnWebSocketMessageSending(bool isSendingAMessage);

public class PororocaWebSocketConnector
{
    private const int bufferSize = 1440; // because Ethernet's MTU is around 1500 bytes
    private const int messagesToSendChannelCapacity = 20;

    public PororocaWebSocketConnectorState State { get; private set; }

    public Exception? ConnectionException { get; private set; }

    private bool isSendingAMessageField;
    public bool IsSendingAMessage
    {
        get => this.isSendingAMessageField;
        private set
        {
            this.isSendingAMessageField = value;
            this.onMessageBeingSent?.Invoke(value);
        }
    }

    public ObservableCollection<PororocaWebSocketMessage> ExchangedMessages { get; } = new();

    private Channel<PororocaWebSocketClientMessageToSend>? MessagesToSendChannel { get; set; }

    private ClientWebSocket? client;

    private CancellationTokenSource? cancelSendingAndReceivingTokenSource;

    private readonly OnWebSocketConnectionChanged? onConnectionChanged;

    private readonly OnWebSocketMessageSending? onMessageBeingSent;

    public PororocaWebSocketConnector(OnWebSocketConnectionChanged? onConnectionChanged = null, OnWebSocketMessageSending? onMessageBeingSent = null)
    {
        this.onConnectionChanged = onConnectionChanged;
        this.onMessageBeingSent = onMessageBeingSent;
    }

    #region STATE SETTERS

    private void SetIsConnecting() =>
        SetChangedState(PororocaWebSocketConnectorState.Connecting, null);

    private void SetIsConnected() =>
        SetChangedState(PororocaWebSocketConnectorState.Connected, null);

    private void SetIsDisconnecting() =>
        SetChangedState(PororocaWebSocketConnectorState.Disconnecting, null);

    private void SetIsDisconnected(Exception? ex) =>
        SetChangedState(PororocaWebSocketConnectorState.Disconnected, ex);

    private void SetChangedState(PororocaWebSocketConnectorState state, Exception? connectionException)
    {
        State = state;
        ConnectionException = connectionException;
        this.onConnectionChanged?.Invoke(State, ConnectionException);
    }

    #endregion

    #region CONNECTION

    /// <summary>
    /// Do not use this method in your tests! Connect to your WebSockets using the PororocaTest class.
    /// </summary>
    public async Task ConnectAsync(ClientWebSocket client, HttpClient httpClient, Uri uri, CancellationToken cancellationToken = default)
    {
        if (State == PororocaWebSocketConnectorState.Connected || State == PororocaWebSocketConnectorState.Connecting)
            return;  // Not throwing exception if user tried to connect whilst WebSocket is connected

        try
        {
            SetIsConnecting();
            await client.ConnectAsync(uri!, httpClient, cancellationToken);
            SetupAfterConnected(client);
            SetIsConnected();
            // Starting at .NET 7, connection response status code and headers will be available for reading,
            // TODO: show them on screen.
        }
        catch (Exception ex)
        {
            SetIsDisconnected(ex);
        }
    }

    public virtual Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        State == PororocaWebSocketConnectorState.Connected ?
        CloseStartingByClientAsync(null, cancellationToken) :
        Task.CompletedTask;  // Not throwing exception if user tried to disconnect whilst WebSocket is not connected

    private void SetupAfterConnected(ClientWebSocket resolvedClient)
    {
        this.client = resolvedClient;
        ClearAfterConnect();
        MessagesToSendChannel = BuildMessagesToSendChannel();

        this.cancelSendingAndReceivingTokenSource = new(); // no maximum lifetime period
        StartMessagesExchangesProcessInBackground(this.cancelSendingAndReceivingTokenSource.Token);
    }

    private void ClearAfterConnect()
    {
        ConnectionException = null;
        foreach (var msg in ExchangedMessages)
        {
            if (msg is IDisposable disposableMsg)
            {
                disposableMsg.Dispose();
            }
        }
        ExchangedMessages.Clear();
    }

    private void ClearAfterClosure()
    {
        // ExchangedMessages and ConnectionException will not be cleared here;
        // will be cleared only when a new connection is successful
        this.client?.Dispose();
        this.client = null;
        MessagesToSendChannel = null;
        // To cancel the emission and reception threads:
        this.cancelSendingAndReceivingTokenSource?.Cancel();
        // Then, to clear this cancellation token source:
        this.cancelSendingAndReceivingTokenSource = null;
    }

    private Channel<PororocaWebSocketClientMessageToSend> BuildMessagesToSendChannel()
    {
        BoundedChannelOptions sendChannelOpts = new(messagesToSendChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        };
        return Channel.CreateBounded<PororocaWebSocketClientMessageToSend>(sendChannelOpts);
    }

    #endregion

    #region SEND MESSAGES

    private void StartMessagesExchangesProcessInBackground(CancellationToken disconnectToken) =>
        Task.Factory.StartNew(() => ProcessMessagesExchangesAsync(disconnectToken),
                              TaskCreationOptions.LongRunning);

    private async Task ProcessMessagesExchangesAsync(CancellationToken clientDisconnectToken)
    {
        // We are using a channel as a queue here because two different messages
        // should not be sent at the same time through a WebSocket,
        // as the other party cannot distinguish which message part corresponds to which message.
        Task HasMessageToSendAsync()
        {
            try
            {
                return MessagesToSendChannel!.Reader.WaitToReadAsync(clientDisconnectToken).AsTask();
            }
            catch (OperationCanceledException)
            {
                return Task.CompletedTask;
            }
        }

        Task<ValueWebSocketReceiveResult> HasMessageToReceiveAsync()
        {
            try
            {
                return this.client!.ReceiveAsync(Memory<byte>.Empty, clientDisconnectToken).AsTask();
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(default(ValueWebSocketReceiveResult));
            }
        }

        bool CanSendMessages() =>
            !clientDisconnectToken.IsCancellationRequested
         && (this.client?.State == WebSocketState.Open || this.client?.State == WebSocketState.Connecting || this.client?.State == WebSocketState.CloseReceived)
         && MessagesToSendChannel is not null;

        bool CanReceiveMessages() =>
            !clientDisconnectToken.IsCancellationRequested
         && (this.client?.State == WebSocketState.Open || this.client?.State == WebSocketState.Connecting || this.client?.State == WebSocketState.CloseSent);

        bool ServerAbortedConnection() =>
            this.client?.State == WebSocketState.Aborted;

        ValueTask<PororocaWebSocketClientMessageToSend> DequeueMessageToSendAsync() =>
            MessagesToSendChannel!.Reader.ReadAsync(CancellationToken.None);

        while (CanSendMessages() && CanReceiveMessages())
        {
            var beganSending = HasMessageToSendAsync();
            var beganReceiving = HasMessageToReceiveAsync();

            var firstOperation = await Task.WhenAny(beganSending, beganReceiving);

            if (clientDisconnectToken.IsCancellationRequested)
            {
                break;
            }
            else if (ServerAbortedConnection())
            {
                await CloseStartingByClientAsync(null, clientDisconnectToken);
                break;
            }
            else if (firstOperation == beganSending && CanSendMessages())
            {
                var msgToSend = await DequeueMessageToSendAsync();
                await SendMessageAsync(msgToSend!, clientDisconnectToken);
            }
            else if (firstOperation == beganReceiving)
            {
                bool isClosing = beganReceiving.Result.MessageType == WebSocketMessageType.Close;
                if (isClosing)
                {
                    // closure message already received
                    PororocaWebSocketServerMessage closingMsg = new(this.client?.CloseStatusDescription);
                    ExchangedMessages.Add(closingMsg);
                    await FinishClosureStartedByServerAsync();
                    break; // exits the reception thread
                }
                else if (CanReceiveMessages())
                {
                    var receivedMsg = await ReceiveMessageAsync(clientDisconnectToken);
                    if (receivedMsg?.MessageType == PororocaWebSocketMessageType.Close)
                    {
                        await FinishClosureStartedByServerAsync();
                        break; // exits the reception thread
                    }
                }
            }
        }
    }

    // It has the same name as the method below, but this one enqueues in the channel;
    // it is not responsible for the real sending
    /// <summary>
    /// Do not use this method in your tests! Use the <c>SendMessageAsync(PororocaWebSocketClientMessage msg)</c> instead.
    /// </summary>
    public ValueTask SendMessageAsync(PororocaWebSocketClientMessageToSend msg) =>
        State == PororocaWebSocketConnectorState.Connected && MessagesToSendChannel is not null ?
        MessagesToSendChannel!.Writer.WriteAsync(msg) :
        ValueTask.CompletedTask; // Not throwing exception if user tried to send message whilst WebSocket is not connected

    private async Task SendMessageAsync(PororocaWebSocketClientMessageToSend msg, CancellationToken cancellationToken = default)
    {
        var msgType = msg.MessageType.ToWebSocketMessageType();
        byte[]? buffer = null;
        try
        {
            IsSendingAMessage = true;

            if (msgType == WebSocketMessageType.Close)
            {
                await CloseStartingByClientAsync(msg, cancellationToken);
                IsSendingAMessage = false;
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
                    await this.client!.SendAsync(trimmedBuffer, msgType, flags, cancellationToken);
                    Array.Clear(buffer);
                }
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = null;
            }

            if (!cancellationToken.IsCancellationRequested && msg.ReachedEndOfStream())
            {
                msg.SentAtUtc = DateTimeOffset.Now;
                ExchangedMessages.Add(msg);
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
        finally
        {
            IsSendingAMessage = false;
        }
    }

    #endregion

    #region RECEIVE MESSAGES

    private async Task<PororocaWebSocketServerMessage?> ReceiveMessageAsync(CancellationToken disconnectToken)
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
                receivalResult = await this.client!.ReceiveAsync(mem, disconnectToken);
                var trimmedBuffer = mem.TrimEnd((byte)0);
                await accumulator.WriteAsync(trimmedBuffer, disconnectToken);
                Array.Clear(buffer);
            }
            while (IsReceivingMessage(receivalResult) && !disconnectToken.IsCancellationRequested);

            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;

            var msgType = receivalResult.MessageType.ToPororocaWebSocketMessageType();
            PororocaWebSocketServerMessage msg =
                msgType == PororocaWebSocketMessageType.Close ?
                new(this.client?.CloseStatusDescription) :
                new(msgType, accumulator.ToArray());

            ExchangedMessages.Add(msg);
            return msg;
        }
        catch
        {
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

    private async Task FinishClosureStartedByServerAsync()
    {
        Exception? closingException = null;
        try
        {
            SetIsDisconnecting();
            // These are the valid states for calling client.CloseOutputAsync()
            if (this.client?.State == WebSocketState.Open || this.client?.State == WebSocketState.CloseReceived)
            {
                await this.client!.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, default);
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

    private async Task CloseStartingByClientAsync(PororocaWebSocketClientMessageToSend? closingMsg, CancellationToken cancellationToken = default)
    {
        Exception? closingException = null;
        try
        {
            SetIsDisconnecting();
            // These are the valid states for calling client.CloseAsync()
            if (this.client?.State == WebSocketState.Open || this.client?.State == WebSocketState.CloseReceived || this.client?.State == WebSocketState.CloseSent)
            {
                if (closingMsg is null)
                {
                    await this.client!.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                }
                else
                {
                    await this.client!.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, closingMsg.Text, cancellationToken);
                    closingMsg!.SentAtUtc = DateTimeOffset.Now;
                    ExchangedMessages.Add(closingMsg);
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