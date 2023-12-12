using System.Net;
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
    public async Task Should_capture_header_value_successfully()
    {
        Assert.Null(this.pororocaTest.GetEnvironmentVariable("Local", "CapturedHeaderValue"));

        var res = await this.pororocaTest.SendHttpRequestAsync("Capture header value");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Equal("oi", this.pororocaTest.GetEnvironmentVariable("Local", "CapturedHeaderValue"));
    }

    [Fact]
    public async Task Should_capture_JSON_value_successfully()
    {
        Assert.Null(this.pororocaTest.GetEnvironmentVariable("Local", "CapturedJSONValue"));

        var res = await this.pororocaTest.SendHttpRequestAsync("Capture JSON value");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("1", this.pororocaTest.GetEnvironmentVariable("Local", "CapturedJSONValue"));
    }

    [Fact]
    public async Task Should_capture_XML_value_successfully()
    {
        Assert.Null(this.pororocaTest.GetEnvironmentVariable("Local", "CapturedXMLValue"));

        var res = await this.pororocaTest.SendHttpRequestAsync("Capture XML value");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("123987456", this.pororocaTest.GetEnvironmentVariable("Local", "CapturedXMLValue"));
    }
}