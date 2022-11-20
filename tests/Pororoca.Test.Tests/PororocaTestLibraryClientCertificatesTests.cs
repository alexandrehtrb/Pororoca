using Xunit;
using System.Net;
using Pororoca.Test;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryClientCertificatesTests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryClientCertificatesTests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                   .AndUseTheEnvironment("Local");
        this.pororocaTest.SetEnvironmentVariable("Local", "BadSslClientCertDir", GetTestClientCertificatesDir());
    }

    [Fact]
    public async Task Should_receive_error_when_client_certificate_is_not_provided()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("No cert provided");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("text/html", res.ContentType);
        Assert.Contains("No required SSL certificate was sent", res.GetBodyAsText());
    }

    [Theory]
    [InlineData("PKCS#12 cert")]
    [InlineData("PEM cert with conjoined unencrypted private key")]
    [InlineData("PEM cert with conjoined encrypted private key")]
    [InlineData("PEM cert with separate unencrypted private key")]
    [InlineData("PEM cert with separate encrypted private key")]
    public async Task Should_be_successful_when_client_certificate_is_provided(string reqName)
    {
        var res = await this.pororocaTest.SendHttpRequestAsync(reqName);

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/html", res.ContentType);
        Assert.Contains("client-authenticated</a> TLS handshake", res.GetBodyAsText());
    }

    private static string GetTestCollectionFilePath()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "PororocaIntegrationTestCollection.pororoca_collection.json");
    }
    private static string GetTestClientCertificatesDir()
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "BadSslClientCertificates");
    }
}