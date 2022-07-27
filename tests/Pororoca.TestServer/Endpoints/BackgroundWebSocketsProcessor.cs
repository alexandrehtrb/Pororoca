using System.Net.WebSockets;
using System.Text;

namespace Pororoca.TestServer.Endpoints;

public static class BackgroundWebSocketsProcessor
{
    private static readonly TimeSpan maximumLifetimePeriod = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan waitForClientPeriod = TimeSpan.FromMinutes(1);

    public static async Task RegisterAndProcessAsync(WebSocket ws, TaskCompletionSource<object> socketFinishedTcs)
    {
        CancellationTokenSource cts = new();
        var maximumLifetimeTask = Task.Delay(maximumLifetimePeriod);
        var msgsExchangesTask = ProcessMessagesExchangesAsync(ws, cts.Token);

        var completedTask = await Task.WhenAny(msgsExchangesTask, maximumLifetimeTask);
        if (completedTask == maximumLifetimeTask)
        {
            // maximum lifetime reached and client did not close websocket yet;
            // server will close it
            cts.Cancel();
            await CloseStartingByServerAsync(ws);
        }
        socketFinishedTcs.SetResult(true);
    }

    private static async Task ProcessMessagesExchangesAsync(WebSocket ws, CancellationToken cancellationToken)
    {
        using MemoryStream accumulator = new();
        while (!cancellationToken.IsCancellationRequested)
        {
            (bool receivedWithinPeriod, var messageType) = await ReceiveMessageWithinPeriodAsync(ws, accumulator, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            else if (!receivedWithinPeriod)
            {
                await PingClientAsync(ws);
            }
            else
            {
                if (IsOpenAndClientStartedClosing(ws, messageType))
                {
                    await FinishClosingStartedByClientAsync(ws);
                    break;
                }
                else
                {
                    await ReplyToClientAsync(ws, (WebSocketMessageType)messageType!, accumulator.ToArray());
                    accumulator.Clear();
                }
            }
        }
    }

    private static async Task<(bool ReceivedWithinPeriod, WebSocketMessageType? MessageType)> ReceiveMessageWithinPeriodAsync(WebSocket ws, MemoryStream accumulator, CancellationToken cancellationToken)
    {
        Memory<byte> buffer = new(new byte[256]);
        Task<ValueWebSocketReceiveResult> receiveTask;
        Task receivalWaitTask;
        bool receivedWithinPeriod;

        do
        {
            receivalWaitTask = Task.Delay(waitForClientPeriod, cancellationToken);
            // do not use below the same cancellationToken as above
            // otherwise, the WebSocket status will become 'Aborted'
            receiveTask = ws.ReceiveAsync(buffer, default).AsTask();
            var completedTask = await Task.WhenAny(receiveTask, receivalWaitTask);
            if (completedTask == receivalWaitTask)
            {
                receivedWithinPeriod = false;
                break;
            }
            else
            {
                receivedWithinPeriod = true;
                var trimmedBuffer = buffer.TrimEnd((byte)0);
                await accumulator.WriteAsync(trimmedBuffer, default);
            }
        } while (receiveTask.Result.IsReceivingMessage() && !cancellationToken.IsCancellationRequested);

        return (receivedWithinPeriod, receiveTask.IsCompletedSuccessfully ? receiveTask.Result.MessageType : null);
    }

    private static bool IsOpenAndClientStartedClosing(WebSocket ws, WebSocketMessageType? messageType) =>
        messageType == WebSocketMessageType.Close
        && (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived);

    private static bool IsReceivingMessage(this ValueWebSocketReceiveResult vwsrr) =>
        vwsrr.MessageType != WebSocketMessageType.Close
        && vwsrr.EndOfMessage == false;

    private static async Task ReplyToClientAsync(WebSocket ws, WebSocketMessageType messageType, ArraySegment<byte> receivedMsg)
    {
        if (messageType == WebSocketMessageType.Text)
        {
            byte[] replyTxtMsg = Encoding.UTF8.GetBytes($"received text {receivedMsg.Count} bytes");
            await ws.SendAsync(replyTxtMsg, WebSocketMessageType.Text, true, default);
        }
        else if (messageType == WebSocketMessageType.Binary)
        {
            byte[] replyBinaryMsg = Encoding.UTF8.GetBytes($"received binary {receivedMsg.Count} bytes");
            await ws.SendAsync(replyBinaryMsg, WebSocketMessageType.Binary, true, default);
        }
    }

    private static Task FinishClosingStartedByClientAsync(WebSocket ws) =>
        ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "bye", default);

    private static Task CloseStartingByServerAsync(WebSocket ws) =>
        ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "maximum lifetime, bye", default);

    private static async Task PingClientAsync(WebSocket ws)
    {
        byte[] pingMsg = Encoding.UTF8.GetBytes($"ping to client");
        await ws.SendAsync(pingMsg, WebSocketMessageType.Text, true, default);
    }

    private static void Clear(this MemoryStream ms)
    {
        byte[] buffer = ms.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);
        ms.Position = 0;
        ms.SetLength(0);
    }
}
