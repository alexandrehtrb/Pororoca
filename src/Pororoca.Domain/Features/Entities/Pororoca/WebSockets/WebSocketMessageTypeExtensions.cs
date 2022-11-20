using System.Net.WebSockets;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public static class WebSocketMessageTypeExtensions
{
    public static PororocaWebSocketMessageType ToPororocaWebSocketMessageType(this WebSocketMessageType msgType) =>
        msgType switch
        {
            WebSocketMessageType.Text => PororocaWebSocketMessageType.Text,
            WebSocketMessageType.Close => PororocaWebSocketMessageType.Close,
            _ => PororocaWebSocketMessageType.Binary
        };
    
    public static WebSocketMessageType ToWebSocketMessageType(this PororocaWebSocketMessageType msgType) =>
        msgType switch
        {
            PororocaWebSocketMessageType.Text => WebSocketMessageType.Text,
            PororocaWebSocketMessageType.Close => WebSocketMessageType.Close,
            _ => WebSocketMessageType.Binary
        };
}