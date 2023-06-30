using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public sealed class PororocaWebSocketConnection : PororocaRequest
{
    [JsonInclude]
    public decimal HttpVersion { get; set; }

    [JsonInclude]
    public string Url { get; set; }

    [JsonInclude]
    public List<PororocaKeyValueParam>? Headers { get; set; }

    [JsonInclude]
    public PororocaRequestAuth? CustomAuth { get; set; }

    [JsonInclude]
    public PororocaWebSocketCompressionOptions? CompressionOptions { get; set; }

    [JsonInclude]
    public List<PororocaKeyValueParam>? Subprotocols { get; set; }

    [JsonInclude]
    public List<PororocaWebSocketClientMessage>? ClientMessages { get; set; }

    [JsonIgnore]
    public bool EnableCompression => CompressionOptions is not null;

#nullable disable warnings
    public PororocaWebSocketConnection() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaWebSocketConnection(string name) : base(PororocaRequestType.Websocket, name)
    {
        HttpVersion = 1.1m;
        Url = string.Empty;
        Headers = null;
        CustomAuth = null;
    }

    public override object Clone() =>
        new PororocaWebSocketConnection(Name)
        {
            HttpVersion = HttpVersion,
            Url = Url,
            Headers = Headers?.Select(h => h.Copy())?.ToList(),
            CustomAuth = CustomAuth?.Copy(),
            ClientMessages = ClientMessages?.Select(m => (PororocaWebSocketClientMessage)m.Clone())?.ToList(),
            Subprotocols = Subprotocols,
            CompressionOptions = CompressionOptions?.Copy()
        };
}