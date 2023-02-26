using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaEnvironment : ICloneable
{
    public string Schema => PororocaCollection.SchemaVersion; // Needs to be object variable, not static

    [JsonInclude]
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    [JsonInclude]
    public string Name { get; private set; }

    public bool IsCurrent { get; set; }

    [JsonInclude]
    public IReadOnlyList<PororocaVariable> Variables { get; private set; }

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
        Variables = new List<PororocaVariable>().AsReadOnly();
        IsCurrent = false;
    }

    public void UpdateName(string name) =>
        Name = name;

    public void UpdateVariables(IEnumerable<PororocaVariable> vars) =>
        Variables = vars.ToList().AsReadOnly();

    public void AddVariable(PororocaVariable variable)
    {
        List<PororocaVariable> newList = new(Variables);
        newList.Add(variable);
        Variables = newList.AsReadOnly();
    }

    public void RemoveVariable(string variableKey)
    {
        List<PororocaVariable> newList = new(Variables);
        newList.RemoveAll(i => i.Key == variableKey);
        Variables = newList.AsReadOnly();
    }

    public PororocaEnvironment ClonePreservingId()
    {
        PororocaEnvironment it = (PororocaEnvironment)Clone();
        it.Id = this.Id;
        return it;
    }

    public object Clone() =>
        new PororocaEnvironment(Guid.NewGuid(), Name, CreatedAt)
        {
            IsCurrent = IsCurrent,
            Variables = Variables.Select(v => (PororocaVariable)v.Clone()).ToList().AsReadOnly()
        };
}