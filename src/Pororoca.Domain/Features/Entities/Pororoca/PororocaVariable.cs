namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaVariable : ICloneable
{
    public bool Enabled { get; set; }
    
    public string Key { get; set; }

    public string? Value { get; set; }

    public bool IsSecret { get; set; }

    #nullable disable warnings
    public PororocaVariable() : this(true, string.Empty, null, false)
    {
        // Parameterless constructor for JSON deserialization
    }
    #nullable restore warnings

    public PororocaVariable(bool enabled, string key, string? value, bool isSecret)
    {
        Enabled = enabled;
        Key = key;
        Value = value;
        IsSecret = isSecret;
    }

    public object Clone() =>
        new PororocaVariable(Enabled, Key, Value, IsSecret);

    public override bool Equals(object? obj) =>
        obj is PororocaVariable other
        && Enabled == other.Enabled
        && Key == other.Key
        && Value == other.Value
        && IsSecret == other.IsSecret;

    public override int GetHashCode() =>
        HashCode.Combine(Enabled, Key, Value, IsSecret);
}