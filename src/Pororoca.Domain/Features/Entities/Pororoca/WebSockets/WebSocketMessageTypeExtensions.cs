using System.Net.WebSockets;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public static class WebSocketMessageTypeExtensions
{
    public static WebSocketMessageType ToWebSocketMessageType(this PororocaWebSocketMessageType msgType) =>
        msgType switch
        {
            PororocaWebSocketMessageType.Text => WebSocketMessageType.Text,
            PororocaWebSocketMessageType.Close => WebSocketMessageType.Close,
            _ => WebSocketMessageType.Binary
        };
}