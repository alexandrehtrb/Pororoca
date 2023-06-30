using System.Text.Json.Serialization;

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

public abstract class PororocaWebSocketMessage
{
    [JsonInclude]
    public PororocaWebSocketMessageDirection Direction { get; }

    [JsonInclude]
    public PororocaWebSocketMessageType MessageType { get; private set; }

#nullable disable warnings
    protected PororocaWebSocketMessage() : this(PororocaWebSocketMessageDirection.FromClient, PororocaWebSocketMessageType.Text)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    protected PororocaWebSocketMessage(PororocaWebSocketMessageDirection direction, PororocaWebSocketMessageType msgType)
    {
        Direction = direction;
        MessageType = msgType;
    }
}