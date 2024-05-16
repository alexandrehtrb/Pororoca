using System.Text;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public sealed record PororocaWebSocketServerMessage
(
    PororocaWebSocketMessageType MessageType,
    byte[] Bytes,
    string? Text,
    PororocaWebSocketMessageRawContentSyntax? TextSyntax,
    DateTimeOffset? ReceivedAtUtc
): PororocaWebSocketMessage(PororocaWebSocketMessageDirection.FromServer, MessageType)
{
    public static PororocaWebSocketServerMessage Make(PororocaWebSocketMessageType msgType, byte[] receivedBytes)
    {
        string? text = null;
        PororocaWebSocketMessageRawContentSyntax? textSyntax = null;

        if (msgType == PororocaWebSocketMessageType.Text || msgType == PororocaWebSocketMessageType.Close)
        {
            text = Encoding.UTF8.GetString(receivedBytes);
            textSyntax = JsonUtils.IsValidJson(text) ?
                PororocaWebSocketMessageRawContentSyntax.Json :
                PororocaWebSocketMessageRawContentSyntax.Other;
        }

        return new(msgType, receivedBytes, text, textSyntax, DateTimeOffset.Now);
    }

    public static PororocaWebSocketServerMessage Make(string? closeStatusDescription) =>
        Make(PororocaWebSocketMessageType.Close, closeStatusDescription is not null ?
                                                   Encoding.UTF8.GetBytes(closeStatusDescription) :
                                                   Array.Empty<byte>());
}