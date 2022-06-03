#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman;

internal class PostmanRequestUrl
{
    public string Raw { get; set; }

    public string? Protocol { get; set; }

    public string[] Host { get; set; }

    public string[]? Path { get; set; }

    public string? Port { get; set; }

    public PostmanVariable[]? Query { get; set; }
}


#nullable enable warnings