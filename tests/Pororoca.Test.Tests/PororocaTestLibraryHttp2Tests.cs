using System.Net;
using Xunit;

namespace Pororoca.Test.Tests;

public sealed class PororocaTestLibraryHttp2Tests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryHttp2Tests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local")
                                        .AndDontCheckTlsCertificate();
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_get_json_with_http_2_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get JSON HTTP/2");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_post_json_with_http_2_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post JSON HTTP/2");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_get_trailers_with_http_2_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get trailers HTTP/2");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        Assert.NotNull(res.Headers);
        Assert.Equal("MyTrailer", res.Headers["Trailer"]);
        Assert.NotNull(res.Trailers);
        Assert.Equal("MyTrailerValue", res.Trailers["mytrailer"]);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }
}