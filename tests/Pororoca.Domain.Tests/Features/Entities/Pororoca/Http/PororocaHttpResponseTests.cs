using System.Net;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.Http;

public static class PororocaHttpResponseTests
{
    private static readonly TimeSpan testElapsedTime = TimeSpan.FromSeconds(4);

    [Fact]
    public static async Task Should_return_success_true_and_the_status_code_if_successful()
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", "inline");

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.True(res.Successful);
        Assert.Equal(HttpStatusCode.Accepted, res.StatusCode);
    }

    [Fact]
    public static void Should_return_success_false_and_the_exception_if_failed()
    {
        // GIVEN
        Exception testException = new();

        // WHEN
        var res = PororocaHttpResponse.Failed(testElapsedTime, testException);

        // THEN
        Assert.False(res.Successful);
        Assert.Equal(testException, res.Exception);
    }

    [Fact]
    public static async Task Should_return_all_headers_found()
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", "inline");

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.NotNull(res.Headers);
        Assert.Equal(4, res.Headers!.Count());
        Assert.Contains(new("Header1", "Value1"), res.Headers);
        Assert.Contains(new("Content-Type", "text/plain"), res.Headers);
        Assert.Contains(new("Content-Disposition", "inline"), res.Headers);
        Assert.Contains(new("Content-Length", "2"), res.Headers);
    }

    [Fact]
    public static void If_there_was_an_exception_but_not_cancelled_then_return_false_in_property()
    {
        // GIVEN

        // WHEN
        var res = PororocaHttpResponse.Failed(testElapsedTime, new Exception());

        // THEN
        Assert.False(res.WasCancelled);
    }

    [Fact]
    public static void If_was_cancelled_then_return_true_in_property()
    {
        // GIVEN

        // WHEN
        var res = PororocaHttpResponse.Failed(testElapsedTime, new TaskCanceledException());

        // THEN
        Assert.True(res.WasCancelled);
    }

    [Fact]
    public static async Task If_no_body_then_has_body_should_be_false()
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage(null, null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.False(res.HasBody);
    }

    [Fact]
    public static async Task If_there_is_body_then_has_body_should_be_true()
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("oi", null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.True(res.HasBody);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("application/json")]
    [InlineData("application/octet-stream")]
    public static async Task Should_get_the_correct_content_type(string? contentType)
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", contentType, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.Equal(contentType, res.ContentType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("application/json")]
    [InlineData("text/html")]
    public static async Task Can_display_text_body_should_be_true_when_text_content_type_or_null(string? contentType)
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", contentType, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.True(res.CanDisplayTextBody);
    }

    [Theory]
    [InlineData("application/octet-stream")]
    [InlineData("video/avi")]
    public static async Task Can_display_text_body_should_be_false_when_not_text_content_type(string? contentType)
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", contentType, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.False(res.CanDisplayTextBody);
    }

    [Fact]
    public static async Task If_no_body_then_body_as_text_should_be_null()
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage(null, null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.Null(res.GetBodyAsText(null));
    }

    [Fact]
    public static async Task If_non_json_text_body_then_body_as_text_should_be_as_is()
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.Equal("oi", res.GetBodyAsText(null));
    }

    [Fact]
    public static async Task If_json_text_body_then_body_as_text_should_be_pretty_printed()
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", "application/json", null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.Equal("{" + Environment.NewLine + "  \"id\": 1" + Environment.NewLine + "}", res.GetBodyAsText(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("oi")]
    public static async Task Get_body_as_binary_should_return_content_as_bytes_or_null(string? content)
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage(content, null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        if (content == null)
        {
            Assert.NotNull(res.GetBodyAsBinary());
            Assert.Empty(res.GetBodyAsBinary()!);
        }
        else
        {
            Assert.Equal(Encoding.UTF8.GetBytes(content), res.GetBodyAsBinary());
        }
    }

    [Theory]
    [InlineData("cool.html", "attachment; filename=\"cool.html\"")]
    [InlineData("filename.jpg", "form-data; name=\"fieldName\"; filename=\"filename.jpg\"")]
    public static async Task Should_parse_content_disposition_filename_correctly_when_available(string expectedFileName, string? contentDisposition)
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", contentDisposition);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.Equal(expectedFileName, res.GetContentDispositionFileName());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("inline")]
    [InlineData("attachment")]
    [InlineData("form-data; name=\"fieldName\"")]
    public static async Task If_content_disposition_filename_not_available_then_return_null(string? contentDisposition)
    {
        // GIVEN
        var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", contentDisposition);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(testElapsedTime, resMsg);

        // THEN
        Assert.Null(res.GetContentDispositionFileName());
    }

    private static HttpResponseMessage CreateTestHttpResponseMessage(string? body, string? contentType, string? contentDisposition)
    {
        ByteArrayContent? content = null;
        if (body != null)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
            content = new(bodyBytes);
            if (contentType != null)
            {
                content.Headers.ContentType = new(contentType);
            }
            if (contentDisposition != null)
            {
                content.Headers.TryAddWithoutValidation("Content-Disposition", contentDisposition);
            }
        }
        HttpResponseMessage resMsg = new(HttpStatusCode.Accepted)
        {
            Content = content
        };
        resMsg.Headers.Add("Header1", "Value1");

        return resMsg;
    }

}