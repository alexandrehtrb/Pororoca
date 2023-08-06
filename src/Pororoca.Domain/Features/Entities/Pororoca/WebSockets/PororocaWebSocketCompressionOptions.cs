using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

public sealed record PororocaWebSocketCompressionOptions
(
    [property: JsonInclude] int ClientMaxWindowBits = 15, // range from 9 to 15, default 15
    [property: JsonInclude] bool ClientContextTakeover = true, // default true
    [property: JsonInclude] int ServerMaxWindowBits = 15, // range from 9 to 15, default 15
    [property: JsonInclude] bool ServerContextTakeover = true // default true
)
{
    public const int DefaultClientMaxWindowBits = 15;
    public const int DefaultServerMaxWindowBits = 15;
    public const bool DefaultClientContextTakeover = true;
    public const bool DefaultServerContextTakeover = true;

    public PororocaWebSocketCompressionOptions Copy() => this with { };
}