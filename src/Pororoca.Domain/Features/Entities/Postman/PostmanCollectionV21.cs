#nullable disable warnings

using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Postman;

internal class PostmanCollectionV21
{
    public const string SchemaUrl = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json";

    public PostmanCollectionInfo Info { get; set; }

    [JsonPropertyName("item")]
    public PostmanCollectionItem[] Items { get; set; }

    public PostmanVariable[]? Variable { get; set; }

    public PostmanAuth? Auth { get; set; }
}

#nullable enable warnings