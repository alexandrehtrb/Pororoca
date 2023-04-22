using System.Net.WebSockets;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public sealed class PororocaWebSocketClientMessageToSend : PororocaWebSocketClientMessage, IDisposable
{
    #region AUXILIARY PROPERTIES FOR INFRASTRUCTURE

    [JsonIgnore]
    public Stream BytesStream { get; set; }

    [JsonIgnore]
    public long BytesLength { get; set; } // cannot use BytesStream.Length because this cannot be re-read after stream closes

    [JsonIgnore]
    public string? Text { get; set; }

    [JsonIgnore]
    public DateTimeOffset? SentAtUtc { get; set; }

    #endregion

    public PororocaWebSocketClientMessageToSend(PororocaWebSocketClientMessage templateWsCliMsg, Stream bytesStream, string? resolvedText) :
        base(msgType: templateWsCliMsg.MessageType,
             name: templateWsCliMsg.Name,
             contentMode: templateWsCliMsg.ContentMode,
             rawContent: templateWsCliMsg.RawContent,
             rawContentSyntax: templateWsCliMsg.RawContentSyntax,
             fileSrcPath: templateWsCliMsg.FileSrcPath,
             disableCompressionForThis: templateWsCliMsg.DisableCompressionForThis)
    {
        BytesStream = bytesStream;
        BytesLength = bytesStream.Length;
        Text = resolvedText;
    }

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