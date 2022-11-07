using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

namespace Pororoca.TestServer.Endpoints;

public static class BackgroundWebSocketsProcessor
{
    private const int bufferSize = 1440; // because Ethernet's MTU is around 1500 bytes
    private static readonly TimeSpan maximumLifetimePeriod = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan waitForClientPeriod = TimeSpan.FromSeconds(12);
    private static readonly Channel<string> messagesToSendChannel = BuildMessagesToSendChannel();

    public static async Task RegisterAndProcessAsync(WebSocket ws, TaskCompletionSource<object> socketFinishedTcs)
    {
        CancellationTokenSource cts = new(maximumLifetimePeriod);
        await ProcessMessagesExchangesAsync(ws, cts.Token);
        socketFinishedTcs.SetResult(true);
    }

    private static async Task ProcessMessagesExchangesAsync(WebSocket ws, CancellationToken disconnectToken = default)
    {
        async Task<bool> HasPingToSendAsync()
        {
            try
            {
                await Task.Delay(waitForClientPeriod, disconnectToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        Task<bool> HasMessageToSendAsync()
        {
            // We are using a channel as a queue here because two different messages
            // should not be sent at the same time through a WebSocket,
            // as the other party cannot distinguish which message part corresponds to which message.
            try
            {
                // we are using only ping here,
                // this is for a hypothetical case
                return messagesToSendChannel!.Reader.WaitToReadAsync(disconnectToken).AsTask();
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(false);
            }
        }

        Task<ValueWebSocketReceiveResult> HasMessageToReceiveAsync()
        {
            try
            {
                return ws.ReceiveAsync(Memory<byte>.Empty, disconnectToken).AsTask();
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(default(ValueWebSocketReceiveResult));
            }
        }

        ValueTask<string> DequeueMessageToSendAsync() =>
            messagesToSendChannel.Reader.ReadAsync(CancellationToken.None);

        bool CanSendMessages() =>
            !disconnectToken.IsCancellationRequested
         && (ws.State == WebSocketState.Open || ws.State == WebSocketState.Connecting || ws.State == WebSocketState.CloseReceived)
         && messagesToSendChannel is not null;

        bool CanReceiveMessages() =>
            !disconnectToken.IsCancellationRequested
         && (ws.State == WebSocketState.Open || ws.State == WebSocketState.Connecting || ws.State == WebSocketState.CloseSent);

        while (CanSendMessages() && CanReceiveMessages())
        {
            var beganSending = HasMessageToSendAsync();
            var beganReceiving = HasMessageToReceiveAsync();
            var beganPinging = HasPingToSendAsync();

            var firstOperation = await Task.WhenAny(beganSending, beganReceiving, beganPinging);

            if (disconnectToken.IsCancellationRequested)
            {
                await CloseStartingByServerAsync(ws, "maximum lifetime, bye", CancellationToken.None);
                break;
            }
            else if (firstOperation == beganSending && CanSendMessages())
            {
                string msgToSend = await DequeueMessageToSendAsync();
                await SendTextMessageAsync(ws, msgToSend, disconnectToken);
            }
            else if (firstOperation == beganPinging && CanSendMessages())
            {
                await PingClientAsync(ws, "{\"ping\": \"hey client\"}", disconnectToken);
            }
            else if (firstOperation == beganReceiving)
            {
                bool isClosing = beganReceiving.Result.MessageType == WebSocketMessageType.Close;
                if (isClosing)
                {
                    // closure message already received
                    await FinishClosingStartedByClientAsync(ws, "ok, bye");
                    break; // exits the reception thread
                }
                else if (CanReceiveMessages())
                {
                    var (receivedMsgType, receivedBytes) = await ReceiveMessageAsync(ws, disconnectToken);
                    if (receivedMsgType == WebSocketMessageType.Close)
                    {
                        await FinishClosingStartedByClientAsync(ws, "ok, bye");
                        break; // exits the reception thread
                    }
                    else
                    {
                        await ReplyToClientAsync(ws, receivedMsgType, receivedBytes, disconnectToken);
                    }
                }
            }
        }
    }

    private static async Task<(WebSocketMessageType, byte[])> ReceiveMessageAsync(WebSocket ws, CancellationToken disconnectToken)
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
                receivalResult = await ws.ReceiveAsync(mem, disconnectToken);
                var trimmedBuffer = mem.TrimEnd((byte)0);
                await accumulator.WriteAsync(trimmedBuffer, disconnectToken);
                Array.Clear(buffer);
            }
            while (IsReceivingMessage(receivalResult) && !disconnectToken.IsCancellationRequested);

            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;

            return (receivalResult.MessageType, accumulator.ToArray());
        }
        catch
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            return (WebSocketMessageType.Close, Array.Empty<byte>());
        }
    }

    private static bool IsReceivingMessage(this ValueWebSocketReceiveResult vwsrr) =>
        vwsrr.MessageType != WebSocketMessageType.Close
        && vwsrr.EndOfMessage == false;

    private static Task ReplyToClientAsync(WebSocket ws, WebSocketMessageType receivedMsgType, ArraySegment<byte> receivedMsg, CancellationToken cancellationToken = default)
    {
        if (receivedMsgType == WebSocketMessageType.Text)
        {
            string receivedStr = Encoding.UTF8.GetString(receivedMsg);
            return SendTextMessageAsync(ws, $"received text ({receivedMsg.Count} bytes): {receivedStr}", cancellationToken);
        }
        else if (receivedMsgType == WebSocketMessageType.Binary)
        {
            return SendTextMessageAsync(ws, $"received binary {receivedMsg.Count} bytes", cancellationToken);
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    private static Task FinishClosingStartedByClientAsync(WebSocket ws, string msg)
    {
        // These are the valid states for calling client.CloseOutputAsync()
        if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
        {
            return ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, msg, default);
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    private static async Task CloseStartingByServerAsync(WebSocket ws, string? closingMsg = null, CancellationToken cancellationToken = default)
    {
        // These are the valid states for calling client.CloseAsync()
        if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived || ws.State == WebSocketState.CloseSent)
        {
            if (closingMsg is null)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, closingMsg, cancellationToken);
            }
            else
            {
                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, closingMsg, cancellationToken);
            }
        }
    }

    private static Task PingClientAsync(WebSocket ws, string msg, CancellationToken cancellationToken = default) =>
        SendTextMessageAsync(ws, msg, cancellationToken);

    private static Task SendTextMessageAsync(WebSocket ws, string msg, CancellationToken cancellationToken = default)
    {
        MemoryStream ms = new(Encoding.UTF8.GetBytes(msg));
        return SendMessageAsync(ws, WebSocketMessageType.Text, ms, cancellationToken);
    }

    private static async Task SendMessageAsync(WebSocket ws, WebSocketMessageType msgType, Stream msgStream, CancellationToken cancellationToken = default)
    {
        byte[]? buffer = null;
        try
        {
            if (msgType == WebSocketMessageType.Close)
            {
                await CloseStartingByServerAsync(ws, "closing starting by server", cancellationToken);
                return;
            }
            else
            {
                buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                Array.Clear(buffer);
                msgStream.Seek(0, SeekOrigin.Begin);
#pragma warning disable CA1835
                while (!cancellationToken.IsCancellationRequested
                    && await msgStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken) > 0)
                {
#pragma warning restore CA1835
                    var trimmedBuffer = buffer.AsMemory().TrimEnd((byte)0);
                    var flags = msgStream.DetermineFlags();
                    await ws.SendAsync(trimmedBuffer, msgType, flags, cancellationToken);
                    Array.Clear(buffer);
                }
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = null;
            }
        }
        catch
        {
            // We will not attempt to close connection if an exception happens
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            throw;
        }
    }

    private static Channel<string> BuildMessagesToSendChannel()
    {
        BoundedChannelOptions sendChannelOpts = new(20)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        };
        return Channel.CreateBounded<string>(sendChannelOpts);
    }

    private static WebSocketMessageFlags DetermineFlags(this Stream stream)
    {
        var flags = WebSocketMessageFlags.None;

        if (stream.ReachedEnd())
            flags |= WebSocketMessageFlags.EndOfMessage;
        
        return flags;
    }

    private static bool ReachedEnd(this Stream stream) =>
        stream.Position == stream.Length;
}
