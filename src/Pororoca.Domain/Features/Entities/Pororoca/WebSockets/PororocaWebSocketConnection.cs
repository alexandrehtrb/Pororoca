using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public sealed record PororocaWebSocketConnection
(
    string Name,
    decimal HttpVersion = 1.1m,
    string Url = "",
    List<PororocaKeyValueParam>? Headers = null,
    PororocaRequestAuth? CustomAuth = null,
    PororocaWebSocketCompressionOptions? CompressionOptions = null,
    List<PororocaKeyValueParam>? Subprotocols = null,
    List<PororocaWebSocketClientMessage>? ClientMessages = null
) : PororocaRequest(PororocaRequestType.Websocket, Name)
{
    [JsonIgnore]
    public bool EnableCompression => CompressionOptions is not null;

#nullable disable warnings
    public PororocaWebSocketConnection() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public override PororocaRequest CopyAbstract() => Copy();

    public PororocaWebSocketConnection Copy() => this with
    {
        Headers = Headers?.Select(h => h.Copy())?.ToList(),
        CustomAuth = CustomAuth?.Copy(),
        CompressionOptions = CompressionOptions?.Copy(),
        Subprotocols = Subprotocols?.Select(s => s.Copy())?.ToList(),
        ClientMessages = ClientMessages?.Select(m => m.Copy())?.ToList()
    };
}