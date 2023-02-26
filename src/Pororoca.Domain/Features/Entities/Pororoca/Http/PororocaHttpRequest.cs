using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public sealed class PororocaHttpRequest : PororocaRequest
{
    [JsonInclude]
    public decimal HttpVersion { get; private set; }

    [JsonInclude]
    public string HttpMethod { get; private set; }

    [JsonInclude]
    public string Url { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaKeyValueParam>? Headers { get; private set; }

    [JsonInclude]
    public PororocaHttpRequestBody? Body { get; private set; }

    [JsonInclude]
    public PororocaRequestAuth? CustomAuth { get; private set; }

#nullable disable warnings
    public PororocaHttpRequest() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaHttpRequest(string name) : this(Guid.NewGuid(), name)
    {
    }

    public PororocaHttpRequest(Guid id, string name) : base(PororocaRequestType.Http, id, name)
    {
        HttpVersion = 1.1m;
        HttpMethod = "GET";
        Url = string.Empty;
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
        Headers = headers?.ToList()?.AsReadOnly();

    public void UpdateCustomAuth(PororocaRequestAuth? auth) =>
        CustomAuth = auth;

    public void UpdateBody(PororocaHttpRequestBody? body) =>
        Body = body;

    public void Update(string name, decimal httpVersion, string httpMethod, string url, PororocaRequestAuth? customAuth, IEnumerable<PororocaKeyValueParam>? headers, PororocaHttpRequestBody? body)
    {
        Name = name;
        HttpVersion = httpVersion;
        HttpMethod = httpMethod;
        Url = url;
        CustomAuth = customAuth;
        Headers = headers?.ToList()?.AsReadOnly();
        Body = body;
    }

    public override PororocaHttpRequest ClonePreservingId() => new(Id, Name)
    {
        HttpVersion = HttpVersion,
        HttpMethod = HttpMethod,
        Url = Url,
        CustomAuth = (PororocaRequestAuth?)CustomAuth?.Clone(),
        Headers = Headers?.Select(h => (PororocaKeyValueParam)h.Clone())?.ToList()?.AsReadOnly(),
        Body = (PororocaHttpRequestBody?)Body?.Clone()
    };

    public override object Clone() =>
        new PororocaHttpRequest(Name)
        {
            HttpVersion = HttpVersion,
            HttpMethod = HttpMethod,
            Url = Url,
            CustomAuth = (PororocaRequestAuth?)CustomAuth?.Clone(),
            Headers = Headers?.Select(h => (PororocaKeyValueParam)h.Clone())?.ToList()?.AsReadOnly(),
            Body = (PororocaHttpRequestBody?)Body?.Clone()
        };
}