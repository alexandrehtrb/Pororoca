using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public sealed class PororocaHttpRequest : PororocaRequest
{
    [JsonInclude]
    public decimal HttpVersion { get; internal set; }

    [JsonInclude]
    public string HttpMethod { get; internal set; }

    [JsonInclude]
    public string Url { get; internal set; }

    [JsonInclude]
    public List<PororocaKeyValueParam>? Headers { get; internal set; }

    [JsonInclude]
    public PororocaHttpRequestBody? Body { get; internal set; }

    [JsonInclude]
    public PororocaRequestAuth? CustomAuth { get; internal set; }

    [JsonInclude]
    public List<PororocaHttpResponseValueCapture>? ResponseCaptures { get; internal set; }

#nullable disable warnings
    public PororocaHttpRequest() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaHttpRequest(string name) : base(PororocaRequestType.Http, name)
    {
        HttpVersion = 1.1m;
        HttpMethod = "GET";
        Url = string.Empty;
        // we can't specify InheritFromCollection here because when there is no auth,
        // CustomAuth should be null, and the JSON deserialization (which uses this constructor)
        // does not replace the InheritFromCollection value with null
        // also, it's safer to leave "no auth" as the default auth.
        CustomAuth = null;
        Headers = null;
        Body = null;
    }

    public void UpdateMethod(string httpMethod) =>
        HttpMethod = httpMethod;

    public void UpdateUrl(string url) =>
        Url = url;

    public void UpdateHttpVersion(decimal httpVersion) =>
        HttpVersion = httpVersion;

    public void UpdateHeaders(IEnumerable<PororocaKeyValueParam>? headers) =>
        Headers = headers?.ToList();

    public void UpdateCustomAuth(PororocaRequestAuth? auth) =>
        CustomAuth = auth;

    public void UpdateBody(PororocaHttpRequestBody? body) =>
        Body = body;
    
    public void UpdateResponseCaptures(List<PororocaHttpResponseValueCapture>? captures) =>
        ResponseCaptures = captures;

    public void Update(string name, decimal httpVersion, string httpMethod, string url, PororocaRequestAuth? customAuth, IEnumerable<PororocaKeyValueParam>? headers, PororocaHttpRequestBody? body, IEnumerable<PororocaHttpResponseValueCapture>? captures)
    {
        Name = name;
        HttpVersion = httpVersion;
        HttpMethod = httpMethod;
        Url = url;
        CustomAuth = customAuth;
        Headers = headers?.ToList();
        Body = body;
        ResponseCaptures = captures?.ToList();
    }

    public override object Clone() =>
        new PororocaHttpRequest(Name)
        {
            HttpVersion = HttpVersion,
            HttpMethod = HttpMethod,
            Url = Url,
            CustomAuth = CustomAuth?.Copy(),
            Headers = Headers?.Select(h => h.Copy())?.ToList(),
            Body = (PororocaHttpRequestBody?)Body?.Clone(),
            ResponseCaptures = ResponseCaptures?.Select(c => c.Copy())?.ToList()
        };
}