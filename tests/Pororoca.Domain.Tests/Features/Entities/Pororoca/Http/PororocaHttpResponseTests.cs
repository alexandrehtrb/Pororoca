using System.Net;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.TranslateRequest.Http;
using Xunit;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.Http;

public static class PororocaHttpResponseTests
{
    private static readonly TimeSpan testElapsedTime = TimeSpan.FromSeconds(4);

    [Fact]
    public static async Task Should_return_success_true_and_the_status_code_if_successful()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", "inline");

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

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
        var res = PororocaHttpResponse.Failed(null, DateTimeOffset.Now, testElapsedTime, testException);

        // THEN
        Assert.False(res.Successful);
        Assert.Equal(testException, res.Exception);
    }

    [Fact]
    public static async Task Should_return_all_headers_found()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", "inline");

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.NotNull(res.Headers);
        Assert.Equal(4, res.Headers!.Count);
        Assert.Equal("Value1", res.Headers["Header1"]);
        Assert.Equal("text/plain", res.Headers["Content-Type"]);
        Assert.Equal("inline", res.Headers["Content-Disposition"]);
        Assert.Equal("2", res.Headers["Content-Length"]);
    }

    [Fact]
    public static void If_there_was_an_exception_but_not_cancelled_then_return_false_in_property()
    {
        // GIVEN

        // WHEN
        var res = PororocaHttpResponse.Failed(null, DateTimeOffset.Now, testElapsedTime, new Exception());

        // THEN
        Assert.False(res.WasCancelled);
    }

    [Fact]
    public static void If_was_cancelled_then_return_true_in_property()
    {
        // GIVEN

        // WHEN
        var res = PororocaHttpResponse.Failed(null, DateTimeOffset.Now, testElapsedTime, new TaskCanceledException());

        // THEN
        Assert.True(res.WasCancelled);
    }

    [Fact]
    public static async Task If_no_body_then_has_body_should_be_false()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage(null, null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.False(res.HasBody);
    }

    [Fact]
    public static async Task If_there_is_body_then_has_body_should_be_true()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("oi", null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

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
        using var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", contentType, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

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
        using var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", contentType, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.True(res.CanDisplayTextBody);
    }

    [Theory]
    [InlineData("application/octet-stream")]
    [InlineData("video/avi")]
    public static async Task Can_display_text_body_should_be_false_when_not_text_content_type(string? contentType)
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", contentType, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.False(res.CanDisplayTextBody);
    }

    [Fact]
    public static async Task If_no_body_then_body_as_text_should_be_null()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage(null, null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Null(res.GetBodyAsPrettyText(null));
    }

    [Fact]
    public static async Task If_non_json_text_body_then_body_as_text_should_be_as_is()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Equal("oi", res.GetBodyAsPrettyText(null));
    }

    [Fact]
    public static async Task If_json_text_body_then_body_as_text_should_be_pretty_printed()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", "application/json", null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Equal("{" + Environment.NewLine + "  \"id\": 1" + Environment.NewLine + "}", res.GetBodyAsPrettyText(null));
    }

    [Fact]
    public static async Task If_xml_text_body_then_body_as_text_should_be_pretty_printed()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("<A><B>qwerty</B></A>", "text/xml", null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Equal("<A>" + Environment.NewLine + "  <B>qwerty</B>" + Environment.NewLine + "</A>", res.GetBodyAsPrettyText(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("oi")]
    public static async Task Get_body_as_binary_should_return_content_as_bytes_or_null(string? content)
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage(content, null, null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

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
        using var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", contentDisposition);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

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
        using var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", contentDisposition);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Null(res.GetContentDispositionFileName());
    }

    [Fact]
    public static async Task Should_read_multipart_parts_if_multipart_response()
    {
        // GIVEN
        var body = MakeFormDataContent([
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "a", "oi", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeFileParam(true, "arq", GetTestFilePath("pirate.gif"), "image/gif")
        ]);
        using HttpResponseMessage resMsg = new(HttpStatusCode.Accepted)
        {
            Content = PororocaHttpRequestTranslator.MakeRequestContent(body, new())
        };

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.NotNull(res.MultipartParts);
        Assert.Equal(2, res.MultipartParts.Length);

        Assert.Equal([
            new("Content-Type", "text/plain; charset=utf-8"),
            new("Content-Disposition", "form-data; name=a")],
            res.MultipartParts[0].Headers.ToArray());
        Assert.Equal("oi", Encoding.UTF8.GetString(res.MultipartParts[0].BinaryBody));

        Assert.Equal([
            new("Content-Type", "image/gif"),
            new("Content-Disposition", "form-data; name=arq; filename=pirate.gif; filename*=utf-8''pirate.gif")],
            res.MultipartParts[1].Headers.ToArray());
        Assert.Equal("R0lGODlhQABAAMQRAAAAAAgIAAgQGBAQEFoAAIRjOZR7Sq0ICLUAAL29vcalY86le+fe3vf39//OnP//AP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQFDwARACwAAAAAQABAAAAF/iAkjmRpnmiqrmzrvnAsz3Rt33iu7zPg/z9eDkgEwnxCUXFpZPkcyB1zGk0BoFiAbjriop7X8JAJ0ZrJpt9iAWBDcd4uutRmr9fvG7VcjpPaDnd4WjVUVYaEXQ6BeHk9e31BkXNli5ZshYh0mkqAlomPXkSdoglAi6AyiKtFZQkMsLCjegZXlre4uAC1f6+xvwypoVO5n7xdvsCycLbFq73Jv1U0np8/1qeXoAAECMrCqmu2AAUiBeNEqIHb3d7SQ26LfRDY11CYf+0ICADBUrfz6j1BRUffvmlDLAVElc5RF4MEwOkpdiubwzL7MvJLUinXgkUKnP3RmFEiHGfN/iqOJIkgIseBFHNFAcCypUkbNFOmVPeJH02WBFwK+QkTSkOGNfcFvUmtXURcgnQZ1Lg0CTeq1YopuFqz6tCp/Xx8xLU1bFehQ1mGDbYkWDCgaHlw1djPrTK7PzN6tVqzrl1psfK2jJsWqF+/f+fufUky6FJZgIMFVUqYb0nKEX1I8+G4M1M4evN6ltYZ82c9oak+DuxY9WmcqdVKO/u6UOx9Dx7wm53bNceVNjX2Rkyzt97KhYPLDkz7d5fbCIbz1n28NjXo0pkbp2y9B/SSs+F2V4VdN/Ho1Lk7V1J+t/b0g8fHEC0YPPOu8o+0P79daf4XfpmSV3ayoNfYfy0gS8aAKcsViN96CjKIm3nTHQghMBIayB91nkG4RGsHhTdYhxCu09mJxJ1IonNXsMEZisRxA+N6XdgBhGMKSrZUEzSW4SIrPwjAI0chAAAh+QQFDwARACwTAAcAGwAeAAAFhGAkjmQpAoAIQWbbok66rm4NxM44s3U53DfdrFdCLRaRRXBHHAGUyGSM2YzcjsnIdFi9OY4Lx5bXaD7FuRM30A2LreuqdS7kyQEJholqzhf5RHh6JACAPYKDalx9eYmFNHJWBAgRBwiPkHeTTouRf3aef6GjpKUjBKapqqusra49B6EhAAA7",
                     Convert.ToBase64String(res.MultipartParts[1].BinaryBody));
    }

    [Fact]
    public static async Task Should_show_multipart_response_as_text_if_all_parts_are_text()
    {
        // GIVEN
        var body = MakeFormDataContent([
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "a", "oi", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "b", "[\"ciao\",\"bye\"]", "application/json")
        ]);
        using HttpResponseMessage resMsg = new(HttpStatusCode.Accepted)
        {
            Content = PororocaHttpRequestTranslator.MakeRequestContent(body, new())
        };

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.NotNull(res.MultipartParts);
        Assert.Equal(2, res.MultipartParts.Length);

        Assert.Equal([
            new("Content-Type", "text/plain; charset=utf-8"),
            new("Content-Disposition", "form-data; name=a")],
            res.MultipartParts[0].Headers.ToArray());
        Assert.Equal("oi", Encoding.UTF8.GetString(res.MultipartParts[0].BinaryBody));

        Assert.Equal([
            new("Content-Type", "application/json; charset=utf-8"),
            new("Content-Disposition", "form-data; name=b")],
            res.MultipartParts[1].Headers.ToArray());
        Assert.Equal("[\"ciao\",\"bye\"]", Encoding.UTF8.GetString(res.MultipartParts[1].BinaryBody));

        Assert.True(res.CanDisplayTextBody);
    }

    [Fact]
    public static async Task Should_capture_response_header_correctly()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("oi", "text/plain", null);
        PororocaHttpResponseValueCapture capture = new(PororocaHttpResponseValueCaptureType.Header, "MyVar", "Header1", null);

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Equal("Value1", res.CaptureValue(capture));
    }

    [Fact]
    public static async Task Should_capture_response_body_json_correctly()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("{\"id\":1}", "text/json", null);
        PororocaHttpResponseValueCapture capture = new(PororocaHttpResponseValueCaptureType.Body, "MyVar", null, "$.id");

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Equal("1", res.CaptureValue(capture));
    }

    [Fact]
    public static async Task Should_capture_response_body_xml_correctly()
    {
        // GIVEN
        using var resMsg = CreateTestHttpResponseMessage("<A><B>100</B></A>", "application/xml", null);
        PororocaHttpResponseValueCapture capture = new(PororocaHttpResponseValueCaptureType.Body, "MyVar", null, "/A/B");

        // WHEN
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, testElapsedTime, resMsg);

        // THEN
        Assert.Equal("100", res.CaptureValue(capture));
    }

    internal static HttpResponseMessage CreateTestHttpResponseMessage(string? body, string? contentType, string? contentDisposition, HttpStatusCode statusCode = HttpStatusCode.Accepted)
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
        HttpResponseMessage resMsg = new(statusCode)
        {
            Content = content
        };
        resMsg.Headers.Add("Header1", "Value1");

        return resMsg;
    }
}