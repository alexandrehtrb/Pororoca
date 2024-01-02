using Pororoca.Domain.Features.Common;
using Xunit;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Domain.Tests.Features.Common;

public static class CommonHeadersValuesTests
{
    [Theory]
    [InlineData("Accept-Datetime")]
    [InlineData("Date")]
    [InlineData("If-Modified-Since")]
    [InlineData("If-Unmodified-Since")]
    public static void Should_provide_datetime_now_as_sample_value_for_date_header(string headerName)
    {
        // Date: Wed, 21 Oct 2015 07:28:00 GMT
        string expectedDateStr = DateTime.Now.ToUniversalTime().ToString("r");
        string actualDateStr = ProvideSampleValueForHeader(headerName);
        Assert.Equal(expectedDateStr[..23], actualDateStr[..23]);
        Assert.EndsWith(" GMT", actualDateStr);
    }

    [Theory]
    [InlineData("Accept-Language")]
    [InlineData("Content-Language")]
    public static void Should_provide_lcid_as_sample_value_for_language_header(string headerName) =>
        Assert.Contains(ProvideSampleValueForHeader(headerName), ExampleLcids);

    [Theory]
    [InlineData("Accept-Encoding")]
    [InlineData("Content-Encoding")]
    public static void Should_provide_compression_algorithm_sample_value_for_encoding_header(string headerName) =>
        Assert.Equal("gzip, br, zstd", ProvideSampleValueForHeader(headerName));

    [Theory]
    [InlineData("X-Request-ID")]
    [InlineData("X-Correlation-ID")]
    public static void Should_provide_guid_sample_value_for_identification_header(string headerName) =>
        Assert.True(Guid.TryParse(ProvideSampleValueForHeader(headerName), out _));

    [Theory]
    [InlineData("Accept", "*/*")]
    [InlineData("Access-Control-Request-Method", "GET")]
    [InlineData("Access-Control-Request-Headers", "origin, x-requested-with")]
    [InlineData("Cache-Control", "no-cache")]
    [InlineData("Connection", "keep-alive")]
    [InlineData("Cookie", "$Version=1; Skin=new;")]
    [InlineData("From", "user@example.com")]
    [InlineData("Host", "en.wikipedia.org")]
    [InlineData("If-Match", "")]
    [InlineData("If-None-Match", "")]
    [InlineData("If-Range", "")]
    [InlineData("Max-Forwards", "10")]
    [InlineData("Origin", "http://www.pudim.com.br")]
    [InlineData("Pragma", "no-cache")]
    [InlineData("Proxy-Authorization", "Basic {{proxy_credentials_here}}")]
    [InlineData("Range", "bytes=500-999")]
    [InlineData("Referer", "http://en.wikipedia.org/wiki/Main_Page")]
    [InlineData("Prefer", "return=representation")]
    [InlineData("Save-Data", "on")]
    [InlineData("Sec-GPC", "1")]
    [InlineData("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/119.0")]
    [InlineData("Via", "1.0 fred, 1.1 example.com (Apache/1.1)")]
    [InlineData("DNT", "1")]
    [InlineData("MyCustomHeader", "")]
    public static void Should_provide_determined_sample_value_for_other_headers(string headerName, string expectedHeaderValue) =>
        Assert.Equal(expectedHeaderValue, ProvideSampleValueForHeader(headerName));
}