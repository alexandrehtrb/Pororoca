#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman;

internal class PostmanRequest
{
    public PostmanAuth? Auth { get; set; }

    public string Method { get; set; }

    public PostmanVariable[] Header { get; set; }

    public PostmanRequestBody? Body { get; set; }

    public object? Url { get; set; }
}

#nullable enable warnings