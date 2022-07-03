using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaRequest : PororocaCollectionItem, ICloneable
{
    public PororocaRequestType RequestType => PororocaRequestType.Http; // Leaving this here for possible future support for other protocols

    [JsonInclude]
    public Guid Id { get; init; }

    [JsonInclude]
    public string Name { get; private set; }

    [JsonInclude]
    public decimal HttpVersion { get; private set; }

    [JsonInclude]
    public string HttpMethod { get; private set; }

    [JsonInclude]
    public string Url { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaKeyValueParam>? Headers { get; private set; }

    [JsonInclude]
    public PororocaRequestBody? Body { get; private set; }

    [JsonInclude]
    public PororocaRequestAuth? CustomAuth { get; private set; }

#nullable disable warnings
    public PororocaRequest() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaRequest(string name) : this(Guid.NewGuid(), name)
    {
    }

    public PororocaRequest(Guid id, string name)
    {
        Id = id;
        Name = name;
        HttpVersion = 1.1m;
        HttpMethod = "GET";
        Url = string.Empty;
        CustomAuth = null;
        Headers = null;
        Body = null;
    }

    public void UpdateName(string name) =>
        Name = name;

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

    public void UpdateBody(PororocaRequestBody? body) =>
        Body = body;

    public void Update(string name, decimal httpVersion, string httpMethod, string url, PororocaRequestAuth? customAuth, IEnumerable<PororocaKeyValueParam>? headers, PororocaRequestBody? body)
    {
        Name = name;
        HttpVersion = httpVersion;
        HttpMethod = httpMethod;
        Url = url;
        CustomAuth = customAuth;
        Headers = headers?.ToList()?.AsReadOnly();
        Body = body;
    }

    public object Clone() =>
        new PororocaRequest(Name)
        {
            HttpVersion = HttpVersion,
            HttpMethod = HttpMethod,
            Url = Url,
            CustomAuth = (PororocaRequestAuth?)CustomAuth?.Clone(),
            Headers = Headers?.Select(h => (PororocaKeyValueParam)h.Clone())?.ToList()?.AsReadOnly(),
            Body = (PororocaRequestBody?)Body?.Clone()
        };

}