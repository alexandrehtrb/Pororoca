#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman;

internal enum PostmanAuthType
{
    // TODO: Rename enum values according to C# style convention,
    // but preserving JSON serialization and deserialization
    noauth,
    basic,
    oauth1,
    oauth2,
    bearer,
    digest,
    apikey,
    awsv4,
    hawk,
    ntlm
}

internal class PostmanAuth
{
    public PostmanAuthType Type { get; set; }

    public PostmanVariable[]? Basic { get; set; }

    public PostmanVariable[]? Bearer { get; set; }
}

#nullable enable warnings