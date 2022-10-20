using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public sealed class PororocaWebSocketCompressionOptions : ICloneable
{
    public const int DefaultClientMaxWindowBits = 15;
    public const int DefaultServerMaxWindowBits = 15;
    public const bool DefaultClientContextTakeover = true;
    public const bool DefaultServerContextTakeover = true;

    [JsonInclude]
    public int ClientMaxWindowBits { get; set; } // range from 9 to 15, default 15

    [JsonInclude]
    public bool ClientContextTakeover { get; set; } // default true

    [JsonInclude]
    public int ServerMaxWindowBits { get; set; } // range from 9 to 15, default 15

    [JsonInclude]
    public bool ServerContextTakeover { get; set; } // default true

    // No need for parameterless constructor for JSON deserialization,
    // since the constructor below already supplies a parameterless construction
    public PororocaWebSocketCompressionOptions(int clientMaxWindowBits = DefaultClientMaxWindowBits,
                                               bool clientContextTakeover = DefaultClientContextTakeover,
                                               int serverMaxWindowBits = DefaultServerMaxWindowBits,
                                               bool serverContextTakeover = DefaultServerContextTakeover)
    {
        ClientMaxWindowBits = clientMaxWindowBits;
        ClientContextTakeover = clientContextTakeover;
        ServerMaxWindowBits = serverMaxWindowBits;
        ServerContextTakeover = serverContextTakeover;
    }

    public object Clone() =>
        new PororocaWebSocketCompressionOptions(ClientMaxWindowBits, ClientContextTakeover, ServerMaxWindowBits, ServerContextTakeover);
}