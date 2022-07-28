namespace Pororoca.Domain.Features.Entities.Pororoca;

public class PororocaHttpRequestBodyGraphQl : ICloneable
{
    public string? Query { get; set; }

    public string? Variables { get; set; }

#nullable disable warnings
    public PororocaHttpRequestBodyGraphQl() : this(null, null)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaHttpRequestBodyGraphQl(string? query, string? variables)
    {
        Query = query;
        Variables = variables;
    }

    public object Clone() =>
        new PororocaHttpRequestBodyGraphQl()
        {
            Query = Query,
            Variables = Variables
        };
}