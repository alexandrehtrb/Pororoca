using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaEnvironment : ICloneable
{
    public string Schema => PororocaCollection.SchemaVersion; // Needs to be object variable, not static

    [JsonInclude]
    public Guid Id { get; set; }

    [JsonInclude]
    public DateTimeOffset CreatedAt { get; internal set; }

    [JsonInclude]
    public string Name { get; internal set; }

    public bool IsCurrent { get; set; }

    [JsonInclude]
    public List<PororocaVariable> Variables { get; internal set; }

#nullable disable warnings
    public PororocaEnvironment()
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaEnvironment(string name) : this(Guid.NewGuid(), name, DateTimeOffset.Now)
    {
    }

    public PororocaEnvironment(Guid id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        Variables = new List<PororocaVariable>();
        IsCurrent = false;
    }

    public void UpdateName(string name) =>
        Name = name;

    public void UpdateVariables(IEnumerable<PororocaVariable> vars) =>
        Variables = vars.ToList();

    public void AddVariable(PororocaVariable variable)
    {
        List<PororocaVariable> newList = new(Variables);
        newList.Add(variable);
        Variables = newList;
    }

    public void RemoveVariable(string variableKey)
    {
        List<PororocaVariable> newList = new(Variables);
        newList.RemoveAll(i => i.Key == variableKey);
        Variables = newList;
    }

    public PororocaEnvironment ClonePreservingId()
    {
        var it = (PororocaEnvironment)Clone();
        it.Id = Id;
        return it;
    }

    public object Clone() =>
        new PororocaEnvironment(Guid.NewGuid(), Name, CreatedAt)
        {
            IsCurrent = IsCurrent,
            Variables = Variables.Select(v => v.Copy()).ToList()
        };
}