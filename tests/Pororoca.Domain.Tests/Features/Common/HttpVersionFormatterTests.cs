using Xunit;
using static Pororoca.Domain.Features.Common.HttpVersionFormatter;

namespace Pororoca.Domain.Tests.Features.Common;

public static class HttpVersionFormatterTests
{
    [Theory]
    [InlineData("HTTP/1.0", 1.0)]
    [InlineData("HTTP/1.1", 1.1)]
    [InlineData("HTTP/2", 2.0)]
    [InlineData("HTTP/3", 3.0)]
    [InlineData("HTTP/4.0", 4.0)]
    public static void Should_properly_format_HTTP_version_as_string(string expectedStr, float httpVersion) =>
        Assert.Equal(expectedStr, FormatHttpVersion((decimal)httpVersion));
}