using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public abstract class PororocaCollectionItem : ICloneable
{
    [JsonPropertyOrder(-1)]
    [JsonInclude]
    public string Name { get; protected set; }

    protected PororocaCollectionItem(string name)
    {
        Name = name;
    }

    public void UpdateName(string name) =>
        Name = name;

    public abstract object Clone();
}