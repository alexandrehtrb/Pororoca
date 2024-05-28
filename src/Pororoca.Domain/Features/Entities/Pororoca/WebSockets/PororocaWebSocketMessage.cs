namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public enum PororocaWebSocketMessageDirection
{
    FromClient,
    FromServer
}

public enum PororocaWebSocketMessageType
{
    Binary,
    Text,
    Close
}

public abstract record PororocaWebSocketMessage
(
    PororocaWebSocketMessageDirection Direction,
    PororocaWebSocketMessageType MessageType
);