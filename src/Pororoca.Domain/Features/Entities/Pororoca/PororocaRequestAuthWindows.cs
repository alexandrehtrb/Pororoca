namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed record PororocaRequestAuthWindows(bool UseCurrentUser, string? Login, string? Password, string? Domain)
{
    // Parameterless constructor for JSON deserialization
    public PororocaRequestAuthWindows() : this(true, null, null, null) { }

    public PororocaRequestAuthWindows Copy() => this with { };
}