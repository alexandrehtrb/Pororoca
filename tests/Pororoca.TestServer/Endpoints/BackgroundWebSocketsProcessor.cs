using System.Net.WebSockets;
using Pororoca.Infrastructure.Features.WebSockets;

namespace Pororoca.TestServer.Endpoints;

public static class BackgroundWebSocketsProcessor
{
    private static readonly TimeSpan maximumLifetimePeriod = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan waitForClientPeriod = TimeSpan.FromSeconds(12);

    public static async Task RegisterAndProcessAsync(ILogger<WebSocketServerSideConnector> logger, WebSocket ws, string? subprotocol, TaskCompletionSource<object> socketFinishedTcs)
    {
        WebSocketServerSideConnector wsc = new(ws);

        // periodic ping whenever client goes quiet
        PeriodicTimer pingTimer = new(waitForClientPeriod);
        _ = Task.Run(async () =>
        {
            while (await pingTimer.WaitForNextTickAsync())
            {
                if (wsc.ConnectionState == WebSocketConnectorState.Connected)
                {
                    await wsc.SendMessageAsync(WebSocketMessageType.Text, GetPingText(subprotocol), false);
                }
            }
        });

        // maximum lifetime closure message
        _ = Task.Run(async () =>
        {
            await Task.Delay(maximumLifetimePeriod);
            await wsc.SendMessageAsync(WebSocketMessageType.Close, GetMaximumLifetimeText(subprotocol), false);
        });

        int msgCount = 0;
        await foreach (var msg in wsc.ExchangedMessagesCollector!.ReadAllAsync())
        {
            msgCount++;
            string msgText = msg.Type switch
            {
                WebSocketMessageType.Binary => $"(binary, {msg.BytesLength} bytes)",
                _ => msg.ReadAsUtf8Text()!
            };
            logger.LogInformation("Message {msgCount}, {direction}: {msgText}", msgCount, msg.Direction, msgText);

            if (msg.Direction == WebSocketMessageDirection.FromServer)
            {
                continue;
            }

            // reset ping timer whenever a client msg is received
            pingTimer.Reset();
            await wsc.SendMessageAsync(WebSocketMessageType.Text, GetReplyText(subprotocol, msg), false);
        }

        pingTimer.Dispose();
        socketFinishedTcs.SetResult(true);
    }

    private static void Reset(this PeriodicTimer timer) =>
        timer.Period = timer.Period;

    private static string GetPingText(string? subprotocol) =>
        subprotocol == "json" ?
        $"{{\"messageType\":\"text\",\"text\":\"¡ping!\"}}" :
        "¡ping!";

    private static string GetReplyText(string? subprotocol, WebSocketMessage receivedMsg)
    {
        if (receivedMsg.Type == WebSocketMessageType.Text)
        {
            string receivedStr = receivedMsg.ReadAsUtf8Text()!;
            return subprotocol == "json" ?
                $"{{\"bytesReceived\":{receivedMsg.BytesLength},\"messageType\":\"text\",\"text\":\"{receivedStr}\"}}" :
                $"received text ({receivedMsg.BytesLength} bytes): {receivedStr}";
        }
        else if (receivedMsg.Type == WebSocketMessageType.Binary)
        {
            return subprotocol == "json" ?
                $"{{\"bytesReceived\":{receivedMsg.BytesLength},\"messageType\":\"binary\"}}" :
                $"received binary {receivedMsg.BytesLength} bytes";
        }
        else
        {
            return string.Empty;
        }
    }

    private static string GetMaximumLifetimeText(string? subprotocol) =>
        subprotocol == "json" ?
        $"{{\"messageType\":\"close\",\"text\":\"maximum lifetime, bye\"}}" :
        "maximum lifetime, bye";
}