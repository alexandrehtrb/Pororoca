using System.Text;
using System.Text.Json;
using System.Net.WebSockets;

namespace Pororoca.Infrastructure.Features.WebSockets;

public enum WebSocketMessageDirection
{
    FromClient = 0,
    FromServer = 1
}

public sealed class WebSocketMessage
{
    public WebSocketMessageDirection Direction { get; }
    public WebSocketMessageType Type { get; }
    public Stream BytesStream { get; }
    // we can't read straight from Stream below,
    // because could throw Exception after Stream is closed
    public long BytesLength { get; }
    public bool DisableCompression { get; }

    internal WebSocketMessage(WebSocketMessageDirection direction, WebSocketMessageType type, Stream bytesStream, bool disableCompression)
    {
        Direction = direction;
        Type = type;
        BytesStream = bytesStream;
        BytesLength = BytesStream.Length;
        DisableCompression = disableCompression;
    }

    internal WebSocketMessage(WebSocketMessageDirection direction, WebSocketMessageType type, byte[] bytes, bool disableCompression)
    {
        Direction = direction;
        Type = type;
        BytesStream = new MemoryStream(bytes);
        BytesLength = BytesStream.Length;
        DisableCompression = disableCompression;
    }

    internal WebSocketMessage(WebSocketMessageDirection direction, WebSocketMessageType type, string text, bool disableCompression)
    {
        Direction = direction;
        Type = type;
        BytesStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        BytesLength = BytesStream.Length;
        DisableCompression = disableCompression;
    }

    internal WebSocketMessageFlags DetermineFlags()
    {
        var flags = WebSocketMessageFlags.None;

        if (ReachedEndOfStream())
            flags |= WebSocketMessageFlags.EndOfMessage;

        if (DisableCompression)
            flags |= WebSocketMessageFlags.DisableCompression;

        return flags;
    }

    internal bool ReachedEndOfStream() =>
        !BytesStream.CanRead || (BytesStream.Position == BytesStream.Length);
        // CanRead check above is required to avoid exceptions

    public string? ReadAsUtf8Text() =>
        BytesStream is MemoryStream ms ?
        Encoding.UTF8.GetString(ms.ToArray()) :
        throw new Exception("Parsing available only for MemoryStreams.");

    public T? ReadAsJson<T>(JsonSerializerOptions? options = default) =>
        BytesStream is MemoryStream ms ?
        JsonSerializer.Deserialize<T>(ms.ToArray(), options) :
        throw new Exception("Parsing available only for MemoryStreams.");
}