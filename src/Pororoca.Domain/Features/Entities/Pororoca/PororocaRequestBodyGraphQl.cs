namespace Pororoca.Domain.Features.Entities.Pororoca;

public class PororocaRequestBodyGraphQl
{
    public string? Query { get; set; }

    public string? Variables { get; set; }

#nullable disable warnings
    public PororocaRequestBodyGraphQl() : this(null, null)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaRequestBodyGraphQl(string? query, string? variables)
    {
        Query = query;
        Variables = variables;
    }
}