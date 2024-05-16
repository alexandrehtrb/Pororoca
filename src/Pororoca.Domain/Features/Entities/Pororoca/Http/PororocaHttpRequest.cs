namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public sealed record PororocaHttpRequest
(
    string Name,
    decimal HttpVersion = 1.1m,
    string HttpMethod = "GET",
    string Url = "",
    List<PororocaKeyValueParam>? Headers = null,
    PororocaHttpRequestBody? Body = null,
    PororocaRequestAuth? CustomAuth = null,
    List<PororocaHttpResponseValueCapture>? ResponseCaptures = null
) : PororocaRequest(PororocaRequestType.Http, Name)
{
    // we can't specify InheritFromCollection here because when there is no auth,
    // CustomAuth should be null, and the JSON deserialization (which uses this constructor)
    // does not replace the InheritFromCollection value with null
    // also, it's safer to leave "no auth" as the default auth.

    // Parameterless constructor for JSON deserialization
    public PororocaHttpRequest() : this(string.Empty){}

    public override PororocaRequest CopyAbstract() => Copy();

    public PororocaHttpRequest Copy() => this with
    {
        HttpVersion = HttpVersion,
        HttpMethod = HttpMethod,
        Url = Url,
        Headers = Headers?.Select(h => h.Copy())?.ToList(),
        Body = (PororocaHttpRequestBody?) Body?.Clone(),
        CustomAuth = CustomAuth?.Copy(),
        ResponseCaptures = ResponseCaptures?.Select(c => c.Copy())?.ToList()
    };
}