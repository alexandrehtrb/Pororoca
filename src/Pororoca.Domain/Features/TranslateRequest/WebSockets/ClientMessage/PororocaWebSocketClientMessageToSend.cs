using System.Net.WebSockets;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public sealed record PororocaWebSocketClientMessageToSend
(
    // parent props
    PororocaWebSocketMessageType MessageType,
    string Name,
    // this props
    Stream BytesStream,
    long BytesLength, // cannot use BytesStream.Length because this cannot be re-read after stream closes
    string? Text,
    DateTimeOffset? SentAtUtc,
    // more parent props
    PororocaWebSocketClientMessageContentMode ContentMode = PororocaWebSocketClientMessageContentMode.Raw,
    string? RawContent = null,
    PororocaWebSocketMessageRawContentSyntax? RawContentSyntax = null,
    string? FileSrcPath = null,
    bool DisableCompressionForThis = false
): PororocaWebSocketClientMessage(MessageType, Name, ContentMode, RawContent, RawContentSyntax, FileSrcPath, DisableCompressionForThis), IDisposable
{
    public PororocaWebSocketClientMessageToSend(PororocaWebSocketClientMessage templateWsCliMsg, Stream bytesStream, string? resolvedText) :
        this(MessageType: templateWsCliMsg.MessageType,
             Name: templateWsCliMsg.Name,
             BytesStream: bytesStream,
             BytesLength: bytesStream.Length,
             Text: resolvedText,
             SentAtUtc: DateTimeOffset.Now,
             DisableCompressionForThis: templateWsCliMsg.DisableCompressionForThis,
             ContentMode: templateWsCliMsg.ContentMode,
             RawContent: templateWsCliMsg.RawContent,
             RawContentSyntax: templateWsCliMsg.RawContentSyntax,
             FileSrcPath: templateWsCliMsg.FileSrcPath){}

    public WebSocketMessageFlags DetermineFlags()
    {
        var flags = WebSocketMessageFlags.None;

        if (ReachedEndOfStream())
            flags |= WebSocketMessageFlags.EndOfMessage;

        if (DisableCompressionForThis)
            flags |= WebSocketMessageFlags.DisableCompression;

        return flags;
    }

    public bool ReachedEndOfStream() =>
        BytesStream.Position == BytesStream.Length;

    public void Dispose() =>
        BytesStream.Dispose();
}