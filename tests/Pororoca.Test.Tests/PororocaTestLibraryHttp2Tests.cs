using Xunit;
using System.Net;
using Pororoca.Test;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryHttp2Tests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryHttp2Tests()
    {
        string filePath = GetTestCollectionFilePath();
        pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                   .AndUseTheEnvironment("Local")
                                   .AndDontCheckTlsCertificate();
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_get_json_with_http_2_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get JSON HTTP/2");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_post_json_with_http_2_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Post JSON HTTP/2");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_get_trailers_with_http_2_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get trailers HTTP/2");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
        Assert.NotNull(res.Headers);
        Assert.Contains(new("Trailer","MyTrailer"), res.Headers);
        Assert.NotNull(res.Trailers);
        Assert.Contains(new("mytrailer","MyTrailerValue"), res.Trailers);
    }

    private static string GetTestCollectionFilePath()
    {
        DirectoryInfo testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }
}