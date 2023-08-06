using System.Net;
using Xunit;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryHttp3Tests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryHttp3Tests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local")
                                        .AndDontCheckTlsCertificate();
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_get_json_with_http_3_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get JSON HTTP/3");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_post_json_with_http_3_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post JSON HTTP/3");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_get_trailers_with_http_3_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get trailers HTTP/3");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        Assert.NotNull(res.Headers);
        Assert.Contains(new("Trailer", "MyTrailer"), res.Headers);
        Assert.NotNull(res.Trailers);
        Assert.Contains(new("mytrailer", "MyTrailerValue"), res.Trailers);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    private static string GetTestCollectionFilePath()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }
}