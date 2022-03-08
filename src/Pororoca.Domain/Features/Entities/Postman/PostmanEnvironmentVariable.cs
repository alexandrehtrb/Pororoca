#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman
{
    public class PostmanEnvironmentVariable
    {
        public string Key { get; set; }

        public string? Value { get; set; }

        public bool Enabled { get; set; }
    }
}

#nullable enable warnings