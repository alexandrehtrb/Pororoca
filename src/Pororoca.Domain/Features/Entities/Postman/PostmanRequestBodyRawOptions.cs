#nullable disable warnings

using System.Collections.Frozen;

namespace Pororoca.Domain.Features.Entities.Postman;

internal class PostmanRequestBodyRawOptions
{
    public static readonly FrozenSet<string> PostmanRequestBodyRawLanguages =
        new[] {"json", "javascript", "html", "xml", "text"}.ToFrozenSet();

    public string Language { get; set; }
}


#nullable enable warnings