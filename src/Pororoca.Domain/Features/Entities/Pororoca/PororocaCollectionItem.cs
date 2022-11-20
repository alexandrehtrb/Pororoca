using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public abstract class PororocaCollectionItem : ICloneable
{
    [JsonPropertyOrder(-2)]
    [JsonInclude]
    public Guid Id { get; init; }

    [JsonPropertyOrder(-1)]
    [JsonInclude]
    public string Name { get; protected set; }

    protected PororocaCollectionItem(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public void UpdateName(string name) =>
        Name = name;

    public abstract object Clone();
}