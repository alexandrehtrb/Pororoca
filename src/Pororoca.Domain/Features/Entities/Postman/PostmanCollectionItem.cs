#nullable disable warnings

using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Postman
{
    public class PostmanCollectionItem
    {
        public string Name { get; set; }

        [JsonPropertyName("item")]
        public PostmanCollectionItem[]? Items { get; set; }

        public PostmanRequest? Request { get; set; }
    }
}

#nullable enable warnings