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
        pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                   .AndUseTheEnvironment("Local");
    }

    [Fact]
    public async Task Should_get_json_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }

    [Fact]
    public async Task Should_get_image_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get image");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("image/gif", res.ContentType);
        Assert.Equal(888, res.GetBodyAsBinary()?.Length);
    }

    [Fact]
    public async Task Should_get_text_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get text");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain", res.ContentType);
        Assert.Equal(462, res.GetBodyAsBinary()?.Length);
        Assert.Contains("Cross-Stitch Pattern", res.GetBodyAsText());
    }

    [Fact]
    public async Task Should_get_headers_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get headers");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        var bodyText = res.GetBodyAsText();
        Assert.Contains("\"Header1\": [" + Environment.NewLine + "    \"oi\"", bodyText);
        Assert.Contains("\"Header2\": [" + Environment.NewLine + "    \"ciao\"", bodyText);
    }

    [Fact]
    public async Task Should_post_none_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Post none");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Empty(res.GetBodyAsBinary());
    }

    [Fact]
    public async Task Should_post_json_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Post JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }

    [Fact]
    public async Task Should_post_form_url_encoded_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Post form URL encoded");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        var bodyText = res.GetBodyAsText();
        Assert.Contains("application/x-www-form-urlencoded", bodyText);
        Assert.Contains("a=xyz&b=123&c=true&myIdSecret=789", bodyText);

        // lets change {{MyIdSecret}} to another value using PororocaTest
        pororocaTest.SetCollectionVariable("MyIdSecret", "999");

        // sending request with new {{MyIdSecret}} value
        res = await pororocaTest.SendRequestAsync("Post form URL encoded");

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
        var res = await pororocaTest.SendRequestAsync("Post multipart form data");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        var bodyText = res.GetBodyAsText();
        Assert.Contains("Content-Disposition:form-data; name=a", bodyText);
        Assert.Contains("Body: xyz", bodyText);
        Assert.Contains("Content-Disposition:form-data; name=b", bodyText);
        Assert.Contains("Body: {\"id\":2}", bodyText);
        Assert.Contains("Content-Disposition:form-data; name=myIdSecret", bodyText);
        Assert.Contains("Body: 789", bodyText);
    }

    [Fact]
    public async Task Should_send_basic_auth_header_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("BASIC");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        var bodyText = res.GetBodyAsText();
        Assert.Contains("Basic dXNyOnB3ZA==", bodyText);
    }

    [Fact]
    public async Task Should_send_bearer_auth_header_with_http_1_1_successfully()
    {
        var res = await pororocaTest.SendRequestAsync(Guid.Parse("af41ca31-6731-4b68-a596-f59d837bc985"));

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        var bodyText = res.GetBodyAsText();
        Assert.Contains("Bearer token_local", bodyText);

        // lets change {{BearerAuthToken}} to another value using PororocaTest
        pororocaTest.SetEnvironmentVariable("Local", "BearerAuthToken", "token_development");

        // xUnit tests run in sequence when they are in the same class
        // hence, no risk of causing troubles in other tests
        // https://xunit.net/docs/running-tests-in-parallel

        // sending request with new {{BearerAuthToken}} value
        res = await pororocaTest.SendRequestAsync(Guid.Parse("af41ca31-6731-4b68-a596-f59d837bc985"));

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        bodyText = res.GetBodyAsText();
        Assert.Contains("Bearer token_development", bodyText);
    }

    private static string GetTestCollectionFilePath()
    {
        DirectoryInfo testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }
}