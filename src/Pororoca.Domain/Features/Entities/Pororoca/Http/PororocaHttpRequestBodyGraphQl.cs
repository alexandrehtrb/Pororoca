namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public sealed record PororocaHttpRequestBodyGraphQl(string? Query, string? Variables)
{
    // Parameterless constructor for JSON deserialization
    public PororocaHttpRequestBodyGraphQl() : this(null, null) { }

    public PororocaHttpRequestBodyGraphQl Copy() => this with { };
}