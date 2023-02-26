using Xunit;
using System.Net;
using Pororoca.Test;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryHttp1Tests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryHttp1Tests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local");
    }

    [Fact]
    public async Task Should_get_json_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [Fact]
    public async Task Should_get_image_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get image");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("image/gif", res.ContentType);
        Assert.Equal(888, res.GetBodyAsBinary()?.Length);
    }

    [Fact]
    public async Task Should_get_text_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get text");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain", res.ContentType);
        Assert.Equal(448, res.GetBodyAsBinary()?.Length);
        Assert.Contains("Cross-Stitch Pattern", res.GetBodyAsText());
    }

    [Fact]
    public async Task Should_get_headers_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get headers");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var bodyListedHeaders = res.GetJsonBodyAs<Dictionary<string, string[]>>();
        Assert.True(bodyListedHeaders!.TryGetValue("Header1", out string[]? hdr1Values));
        Assert.Contains("oi", hdr1Values);
        Assert.True(bodyListedHeaders!.TryGetValue("Header2", out string[]? hdr2Values));
        Assert.Contains("ciao", hdr2Values);
    }

    [Fact]
    public async Task Should_post_none_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post none");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.NotNull(res.GetBodyAsBinary());
        Assert.Empty(res.GetBodyAsBinary()!);
    }

    [Fact]
    public async Task Should_post_json_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        
        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [Fact]
    public async Task Should_post_form_url_encoded_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post form URL encoded");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsText();
        Assert.Contains("application/x-www-form-urlencoded", bodyText);
        Assert.Contains("a=xyz&b=123&c=true&myIdSecret=789", bodyText);

        // let's change {{MyIdSecret}} to another value using PororocaTest
        this.pororocaTest.SetCollectionVariable("MyIdSecret", "999");

        // sending request with new {{MyIdSecret}} value
        res = await this.pororocaTest.SendHttpRequestAsync("Post form URL encoded");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        bodyText = res.GetBodyAsText();
        Assert.Contains("application/x-www-form-urlencoded", bodyText);
        Assert.Contains("a=xyz&b=123&c=true&myIdSecret=999", bodyText);
    }

    [Fact]
    public async Task Should_post_multipart_form_data_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post multipart form data");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsText();
        Assert.Contains("Content-Disposition: form-data; name=a", bodyText);
        Assert.Contains("Body: xyz", bodyText);
        Assert.Contains("Content-Disposition: form-data; name=b", bodyText);
        Assert.Contains("Body: {\"id\":2}", bodyText);
        Assert.Contains("Content-Disposition: form-data; name=myIdSecret", bodyText);
        Assert.Contains("Body: 789", bodyText);
    }

    [Fact]
    public async Task Should_send_basic_auth_header_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("BASIC");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsText();
        Assert.Contains("Basic dXNyOnB3ZA==", bodyText);
    }

    [Fact]
    public async Task Should_send_bearer_auth_header_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("BEARER");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsText();
        Assert.Contains("Bearer token_local", bodyText);

        // lets change {{BearerAuthToken}} to another value using PororocaTest
        this.pororocaTest.SetEnvironmentVariable("Local", "BearerAuthToken", "token_development");

        // xUnit tests run in sequence when they are in the same class
        // hence, no risk of causing troubles in other tests
        // https://xunit.net/docs/running-tests-in-parallel

        // sending request with new {{BearerAuthToken}} value
        res = await this.pororocaTest.SendHttpRequestAsync("BEARER");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        bodyText = res.GetBodyAsText();
        Assert.Contains("Bearer token_development", bodyText);
    }

    private static string GetTestCollectionFilePath()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }
}