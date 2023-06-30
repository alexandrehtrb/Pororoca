namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed record PororocaVariable(bool Enabled, string Key, string? Value, bool IsSecret)
{
    // Parameterless constructor for JSON deserialization
    public PororocaVariable() : this(true, string.Empty, null, false) { }

    public PororocaVariable Copy() => this with { };
}