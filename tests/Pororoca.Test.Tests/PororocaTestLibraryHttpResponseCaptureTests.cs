using System.Net;
using Pororoca.Test;
using Xunit;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryHttpResponseCaptureTests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryHttpResponseCaptureTests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local");
    }

    [Fact]
    public async Task Should_capture_JSON_value_successfully()
    {
        Assert.Null(this.pororocaTest.GetEnvironmentVariable("Local", "CapturedJSONValue"));

        var res = await this.pororocaTest.SendHttpRequestAsync("Capture JSON value");

        Assert.NotNull(res);
        string? body = res.GetBodyAsText();
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("1", this.pororocaTest.GetEnvironmentVariable("Local", "CapturedJSONValue"));
    }

    [Fact]
    public async Task Should_capture_XML_value_successfully()
    {
        Assert.Null(this.pororocaTest.GetEnvironmentVariable("Local", "CapturedXMLValue"));

        var res = await this.pororocaTest.SendHttpRequestAsync("Capture XML value");

        Assert.NotNull(res);
        string? body = res.GetBodyAsText();
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("123987456", this.pororocaTest.GetEnvironmentVariable("Local", "CapturedXMLValue"));
    }

    private static string GetTestCollectionFilePath()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }
}