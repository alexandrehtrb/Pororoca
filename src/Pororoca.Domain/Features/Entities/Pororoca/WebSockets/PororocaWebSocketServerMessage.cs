using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public sealed class PororocaWebSocketServerMessage : PororocaWebSocketMessage
{
    #region AUXILIARY PROPERTIES FOR INFRASTRUCTURE

    [JsonIgnore]
    public byte[] Bytes { get; }

    [JsonIgnore]
    public string? Text { get; }

    [JsonIgnore]
    public PororocaWebSocketMessageRawContentSyntax? TextSyntax { get; }

    [JsonIgnore]
    public DateTimeOffset? ReceivedAtUtc { get; }

    #endregion

    public PororocaWebSocketServerMessage(string? closeStatusDescription)
        : this(PororocaWebSocketMessageType.Close, closeStatusDescription is not null ?
                                                   Encoding.UTF8.GetBytes(closeStatusDescription) :
                                                   Array.Empty<byte>())
    {
    }

    public PororocaWebSocketServerMessage(PororocaWebSocketMessageType msgType, byte[] receivedBytes) : base(PororocaWebSocketMessageDirection.FromServer, msgType)
    {
        Bytes = receivedBytes;
        ReceivedAtUtc = DateTimeOffset.Now;

        if (msgType == PororocaWebSocketMessageType.Text || msgType == PororocaWebSocketMessageType.Close)
        {
            Text = Encoding.UTF8.GetString(Bytes);
            TextSyntax = JsonUtils.IsValidJson(Text) ?
                PororocaWebSocketMessageRawContentSyntax.Json :
                PororocaWebSocketMessageRawContentSyntax.Other;
        }
    }
}