using System.Net;
using Xunit;
using static Pororoca.Domain.Features.Common.HttpStatusCodeFormatter;

namespace Pororoca.Domain.Tests.Features.Common;

public static class HttpStatusCodeFormatterTests
{
    [Theory]
    [InlineData("200 OK", HttpStatusCode.OK)]
    [InlineData("404 NotFound", HttpStatusCode.NotFound)]
    [InlineData("500 InternalServerError", HttpStatusCode.InternalServerError)]
    public static void Should_properly_format_HTTP_status_code_as_text(string expected, HttpStatusCode statusCode) =>
        Assert.Equal(expected, FormatHttpStatusCodeText(statusCode));
}