#nullable disable warnings

using System.Collections.Immutable;

namespace Pororoca.Domain.Features.Entities.Postman;

internal class PostmanRequestBodyRawOptions
{
    public static readonly ImmutableList<string> PostmanRequestBodyRawLanguages =
        ["json", "javascript", "html", "xml", "text"];

    public string Language { get; set; }
}


#nullable enable warnings