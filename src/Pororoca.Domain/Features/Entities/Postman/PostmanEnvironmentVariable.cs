#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman;

internal class PostmanEnvironmentVariable
{
    public string Key { get; set; }

    public string? Value { get; set; }

    public bool Enabled { get; set; }
}

#nullable enable warnings