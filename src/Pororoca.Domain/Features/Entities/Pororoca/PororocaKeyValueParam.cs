namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaKeyValueParam : ICloneable
{
    public bool Enabled { get; set; }
    
    public string Key { get; set; }

    public string? Value { get; set; }

    #nullable disable warnings
    public PororocaKeyValueParam() : this(true, string.Empty, string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
    #nullable restore warnings

    public PororocaKeyValueParam(bool enabled, string key, string? value)
    {
        Enabled = enabled;
        Key = key;
        Value = value;
    }

    public override bool Equals(object? obj) =>
        obj is PororocaKeyValueParam param
        && Enabled == param.Enabled
        && Key == param.Key
        && Value == param.Value;

    public override int GetHashCode() =>
        HashCode.Combine(Enabled, Key, Value);

    public object Clone() =>
        new PororocaKeyValueParam(Enabled, Key, Value);
}