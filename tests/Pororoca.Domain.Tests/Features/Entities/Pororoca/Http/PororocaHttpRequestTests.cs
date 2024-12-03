using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;
using static Pororoca.Domain.Features.Entities.Pororoca.PororocaRequestAuth;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.Http;

public static class PororocaHttpRequestTests
{
    [Fact]
    public static void Should_copy_full_req_creating_new_instance()
    {
        // GIVEN
        PororocaHttpRequest req = new(
            Name: "name",
            HttpVersion: 3.0m,
            HttpMethod: "PUT",
            Url: "myurl",
            Headers: [new(true, "k1", "v1"), new(true, "k2", "v2")],
            Body: MakeRawContent("{\"id\":3162}", "application/json"),
            CustomAuth: MakeBasicAuth("usr", "pwd"),
            ResponseCaptures: [new(PororocaHttpResponseValueCaptureType.Header, "var1", "Host", null)]
        );

        // WHEN
        var copy = req.Copy();

        // THEN
        Assert.NotSame(req, copy);
        Assert.Equal(req.Name, copy.Name);
        Assert.Equal(req.HttpVersion, copy.HttpVersion);
        Assert.Equal(req.HttpMethod, copy.HttpMethod);
        Assert.Equal(req.Url, copy.Url);
        Assert.Equal(req.Headers, copy.Headers);
        Assert.Equal(req.Body, copy.Body);
        Assert.Equal(req.CustomAuth, copy.CustomAuth);
        Assert.Equal(req.ResponseCaptures, copy.ResponseCaptures);
    }

    [Fact]
    public static void Should_copy_empty_req_creating_new_instance()
    {
        // GIVEN
        PororocaHttpRequest req = new(
            Name: "name",
            HttpVersion: 3.0m,
            HttpMethod: "GET",
            Url: "myurl",
            Headers: null,
            Body: null,
            CustomAuth: null,
            ResponseCaptures: null
        );

        // WHEN
        var copy = req.Copy();

        // THEN
        Assert.NotSame(req, copy);
        Assert.Equal(req.Name, copy.Name);
        Assert.Equal(req.HttpVersion, copy.HttpVersion);
        Assert.Equal(req.HttpMethod, copy.HttpMethod);
        Assert.Equal(req.Url, copy.Url);
        Assert.Equal(req.Headers, copy.Headers);
        Assert.Equal(req.Body, copy.Body);
        Assert.Equal(req.CustomAuth, copy.CustomAuth);
        Assert.Equal(req.ResponseCaptures, copy.ResponseCaptures);
    }
}