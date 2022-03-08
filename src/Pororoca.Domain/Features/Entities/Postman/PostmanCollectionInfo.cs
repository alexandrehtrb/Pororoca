#nullable disable warnings

using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Postman
{
    public class PostmanCollectionInfo
    {
        [JsonPropertyName("_postman_id")]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public string Schema { get; set; }
    }
}

#nullable enable warnings