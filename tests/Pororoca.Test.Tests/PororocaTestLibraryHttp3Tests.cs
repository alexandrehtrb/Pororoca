using Xunit;
using Xunit.Abstractions;
using System.Net;
using Pororoca.Test;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryHttp3Tests
{
    private readonly PororocaTest pororocaTest;
    private readonly ITestOutputHelper output;

    public PororocaTestLibraryHttp3Tests(ITestOutputHelper output)
    {
        this.output = output;
        string filePath = GetTestCollectionFilePath();
        pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                   .AndUseTheEnvironment("Local")
                                   .AndDontCheckTlsCertificate();
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_get_json_with_http_3_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Get JSON HTTP/3");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_post_json_with_http_3_successfully()
    {
        var res = await pororocaTest.SendRequestAsync("Post JSON HTTP/3");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);
        Assert.Contains("\"id\": 1", res.GetBodyAsText());
    }

    private static string GetTestCollectionFilePath()
    {
        DirectoryInfo testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }
}
