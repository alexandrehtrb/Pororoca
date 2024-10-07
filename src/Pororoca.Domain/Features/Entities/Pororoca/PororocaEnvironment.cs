using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

#if DEBUG
[DebuggerDisplay("{Name,nq}")]
#endif
public sealed record PororocaEnvironment
(
    [property: JsonInclude] Guid Id,
    [property: JsonInclude] DateTimeOffset CreatedAt,
    [property: JsonInclude] string Name,
    [property: JsonInclude] bool IsCurrent,
    [property: JsonInclude] List<PororocaVariable> Variables
)
{
    [JsonPropertyOrder(-1)]
    public string Schema => PororocaCollection.SchemaVersion; // Needs to be object variable, not static

    public PororocaEnvironment(string name) : this(Guid.NewGuid(), DateTimeOffset.Now, name, false, new()) { }

    // Parameterless constructor for JSON deserialization
    public PororocaEnvironment() : this(string.Empty) { }

    public PororocaEnvironment Copy(bool preserveId) => this with
    {
        Id = preserveId ? Id : Guid.NewGuid(),
        Variables = Variables.Select(v => v.Copy()).ToList()
    };
}