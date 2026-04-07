#if DEBUG
using System.Diagnostics;
#endif
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Pororoca.Infrastructure.Features.WebSockets;

// Source code taken from:
// https://github.com/alexandrehtrb/AlexandreHtrb.WebSocketExtensions
// 
// The MIT License(MIT)
// Copyright(c) 2026 Alexandre H.T.R. Bonfitto
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

public enum WebSocketMessageDirection
{
    FromClient = 0,
    FromServer = 1
}

#if DEBUG
[DebuggerDisplay("{DescriptionForDebugger,nq}")]
#endif
public sealed class WebSocketMessage
{
    internal bool IsStreamBased => BytesStream is not null;
    internal byte[]? Bytes { get; }
    internal Stream? BytesStream { get; }
    public WebSocketMessageDirection Direction { get; }
    public WebSocketMessageType Type { get; }
    internal bool DisableCompression { get; }
    public bool CanBeSavedToFile => this.Bytes != null || this.BytesStream is MemoryStream;
    public long Length { get; }

#if DEBUG
    private string DescriptionForDebugger => FormatForLogging();
#endif

    internal WebSocketMessage(WebSocketMessageDirection direction, WebSocketMessageType type, byte[] bytes, bool disableCompression)
    {
        Direction = direction;
        Type = type;
        DisableCompression = disableCompression;
        Bytes = bytes;
        Length = Bytes.Length;
    }

    internal WebSocketMessage(WebSocketMessageDirection direction, WebSocketMessageType type, string txt, bool disableCompression)
    {
        Direction = direction;
        Type = type;
        DisableCompression = disableCompression;
        Bytes = Encoding.UTF8.GetBytes(txt);
        Length = Bytes.Length;
    }

    internal WebSocketMessage(WebSocketMessageDirection direction, WebSocketMessageType type, Stream bytesStream, bool disableCompression)
    {
        Direction = direction;
        Type = type;
        DisableCompression = disableCompression;
        BytesStream = bytesStream;
        Length = BytesStream.Length;
    }

    internal WebSocketMessageFlags DetermineFlags()
    {
        var flags = WebSocketMessageFlags.None;

        if (!IsStreamBased || ReachedEndOfStream())
            flags |= WebSocketMessageFlags.EndOfMessage;

        if (DisableCompression)
            flags |= WebSocketMessageFlags.DisableCompression;

        return flags;
    }

    internal bool ReachedEndOfStream() =>
        // CanRead check below is required to avoid exceptions
        BytesStream is not null && (!BytesStream.CanRead || (BytesStream.Position == BytesStream.Length));

    public byte[] ReadBytes() =>
        Bytes is not null ?
        Bytes :
        BytesStream is MemoryStream ms ?
        ms.ToArray() :
        throw new NotSupportedException("Parsing available only for MemoryStreams.");

    public Stream ReadAsStream() =>
        BytesStream is not null ?
        BytesStream :
        new MemoryStream(Bytes!);

    public string? ReadAsUtf8Text() =>
        Bytes is not null ?
        Encoding.UTF8.GetString(Bytes) :
        BytesStream is MemoryStream ms ?
        Encoding.UTF8.GetString(ms.ToArray()) :
        throw new NotSupportedException("Parsing available only for MemoryStreams.");

    public T ReadAsUtf8Json<T>(JsonTypeInfo<T> jsonTypeInfo) =>
        Bytes is not null ?
        JsonSerializer.Deserialize(Bytes.AsSpan(), jsonTypeInfo)! :
        BytesStream is MemoryStream ms ?
        JsonSerializer.Deserialize(ms.ToArray(), jsonTypeInfo)! :
        throw new NotSupportedException("Parsing available only for MemoryStreams.");

    public T ReadAsUtf8Json<T>(JsonSerializerOptions opts) =>
        Bytes is not null ?
        JsonSerializer.Deserialize<T>(Bytes.AsSpan(), opts)! :
        BytesStream is MemoryStream ms ?
        JsonSerializer.Deserialize<T>(ms.ToArray(), opts)! :
        throw new NotSupportedException("Parsing available only for MemoryStreams.");

    public string FormatForLogging() => Type switch
    {
        WebSocketMessageType.Text or WebSocketMessageType.Close when Bytes is not null || BytesStream is MemoryStream => ReadAsUtf8Text()!,
        WebSocketMessageType.Text when Bytes is null && (BytesStream is null || BytesStream is not MemoryStream) => "(text, ? bytes)",
        WebSocketMessageType.Close when Bytes is null && (BytesStream is null || BytesStream is not MemoryStream) => "(close, ? bytes)",
        WebSocketMessageType.Binary when Bytes is not null => $"(binary, {Bytes.Length} bytes)",
        WebSocketMessageType.Binary when BytesStream is MemoryStream ms => $"(binary, {ms.Length} bytes)",
        WebSocketMessageType.Binary when BytesStream is not MemoryStream => "(binary, ? bytes)",
        _ => "(unknown)"
    };
}