#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman;

internal class PostmanVariable
{
    public string Key { get; set; }

    public string? Value { get; set; }

    public string? Type { get; set; }

    public string? Description { get; set; }

    public bool? Disabled { get; set; }
}

#nullable enable warnings