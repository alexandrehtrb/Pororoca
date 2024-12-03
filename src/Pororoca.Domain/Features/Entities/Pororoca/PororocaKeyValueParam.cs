namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed record PororocaKeyValueParam(bool Enabled, string Key, string? Value)
{
    // Parameterless constructor for JSON deserialization
    public PororocaKeyValueParam() : this(true, string.Empty, string.Empty) { }

    public PororocaKeyValueParam Copy() => this with { };

#if DEBUG
    public override string ToString() => $"{(Enabled ? "✔️" : "⛔")} {Key}: \"{Value}\"";
#endif
}