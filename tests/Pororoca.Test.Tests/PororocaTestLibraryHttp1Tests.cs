using System.Net;
using Xunit;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryHttp1Tests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryHttp1Tests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local");
    }

    [Fact]
    public async Task Should_get_json_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [Fact]
    public async Task Should_get_image_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get image");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("image/gif", res.ContentType);
        Assert.Equal(888, res.GetBodyAsBinary()?.Length);
    }

    [Fact]
    public async Task Should_get_text_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get text");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain", res.ContentType);
        int expectedLength = OperatingSystem.IsWindows() ? 462 : 448; // because of \r\n instead of \n
        Assert.Equal(expectedLength, res.GetBodyAsBinary()?.Length);
        Assert.Contains("Cross-Stitch Pattern", res.GetBodyAsPrettyText());
    }

    [Fact]
    public async Task Should_get_multipart_text_and_binary_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get multipart text and binary");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("multipart/form-data", res.ContentType);
        Assert.NotNull(res.MultipartParts);
        Assert.Equal(2, res.MultipartParts.Length);

        Assert.Contains(new KeyValuePair<string, string>("Content-Type", "text/plain"), res.MultipartParts[0].Headers);
        Assert.Contains(new KeyValuePair<string, string>("Content-Disposition", "form-data; name=a"), res.MultipartParts[0].Headers);
        Assert.True(res.MultipartParts[0].IsTextContent);
        Assert.Equal("oi"u8, res.MultipartParts[0].BinaryBody);

        Assert.Contains(new KeyValuePair<string, string>("Content-Type", "image/gif"), res.MultipartParts[1].Headers);
        Assert.Contains(new KeyValuePair<string, string>("Content-Disposition", "form-data; name=arq; filename=pirate.gif"), res.MultipartParts[1].Headers);
        Assert.False(res.MultipartParts[1].IsTextContent);
        Assert.Equal(
            Convert.FromBase64String("R0lGODlhQABAAMQRAAAAAAgIAAgQGBAQEFoAAIRjOZR7Sq0ICLUAAL29vcalY86le+fe3vf39//OnP//AP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQFDwARACwAAAAAQABAAAAF/iAkjmRpnmiqrmzrvnAsz3Rt33iu7zPg/z9eDkgEwnxCUXFpZPkcyB1zGk0BoFiAbjriop7X8JAJ0ZrJpt9iAWBDcd4uutRmr9fvG7VcjpPaDnd4WjVUVYaEXQ6BeHk9e31BkXNli5ZshYh0mkqAlomPXkSdoglAi6AyiKtFZQkMsLCjegZXlre4uAC1f6+xvwypoVO5n7xdvsCycLbFq73Jv1U0np8/1qeXoAAECMrCqmu2AAUiBeNEqIHb3d7SQ26LfRDY11CYf+0ICADBUrfz6j1BRUffvmlDLAVElc5RF4MEwOkpdiubwzL7MvJLUinXgkUKnP3RmFEiHGfN/iqOJIkgIseBFHNFAcCypUkbNFOmVPeJH02WBFwK+QkTSkOGNfcFvUmtXURcgnQZ1Lg0CTeq1YopuFqz6tCp/Xx8xLU1bFehQ1mGDbYkWDCgaHlw1djPrTK7PzN6tVqzrl1psfK2jJsWqF+/f+fufUky6FJZgIMFVUqYb0nKEX1I8+G4M1M4evN6ltYZ82c9oak+DuxY9WmcqdVKO/u6UOx9Dx7wm53bNceVNjX2Rkyzt97KhYPLDkz7d5fbCIbz1n28NjXo0pkbp2y9B/SSs+F2V4VdN/Ho1Lk7V1J+t/b0g8fHEC0YPPOu8o+0P79daf4XfpmSV3ayoNfYfy0gS8aAKcsViN96CjKIm3nTHQghMBIayB91nkG4RGsHhTdYhxCu09mJxJ1IonNXsMEZisRxA+N6XdgBhGMKSrZUEzSW4SIrPwjAI0chAAAh+QQFDwARACwTAAcAGwAeAAAFhGAkjmQpAoAIQWbbok66rm4NxM44s3U53DfdrFdCLRaRRXBHHAGUyGSM2YzcjsnIdFi9OY4Lx5bXaD7FuRM30A2LreuqdS7kyQEJholqzhf5RHh6JACAPYKDalx9eYmFNHJWBAgRBwiPkHeTTouRf3aef6GjpKUjBKapqqusra49B6EhAAA7"),
            res.MultipartParts[1].BinaryBody);
    }

    [Fact]
    public async Task Should_get_multipart_text_only_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get multipart text only");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("multipart/form-data", res.ContentType);
        Assert.NotNull(res.MultipartParts);
        Assert.Equal(2, res.MultipartParts.Length);

        Assert.Contains(new KeyValuePair<string, string>("Content-Type", "text/plain"), res.MultipartParts[0].Headers);
        Assert.Contains(new KeyValuePair<string, string>("Content-Disposition", "form-data; name=a"), res.MultipartParts[0].Headers);
        Assert.True(res.MultipartParts[0].IsTextContent);
        Assert.Equal("oi"u8, res.MultipartParts[0].BinaryBody);

        Assert.Contains(new KeyValuePair<string, string>("Content-Type", "application/json"), res.MultipartParts[1].Headers);
        Assert.Contains(new KeyValuePair<string, string>("Content-Disposition", "form-data; name=b"), res.MultipartParts[1].Headers);
        Assert.True(res.MultipartParts[1].IsTextContent);
        Assert.Equal("{\"msg\":\"ciao\"}"u8, res.MultipartParts[1].BinaryBody);
    }

    [Fact]
    public async Task Should_get_headers_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Get headers");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);

        Assert.NotNull(res.Headers);
        Assert.Equal("oi", res.Headers["MIRRORED-Header1"]);
        Assert.Equal("ciao", res.Headers["MIRRORED-Header2"]);
    }

    [Fact]
    public async Task Should_post_none_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post none");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.NotNull(res.GetBodyAsBinary());
        Assert.Empty(res.GetBodyAsBinary()!);
    }

    [Fact]
    public async Task Should_post_json_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post JSON");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/json; charset=utf-8", res.ContentType);

        var jsonObj = res.GetJsonBodyAs<Dictionary<string, int>>();
        Assert.NotNull(jsonObj);
        Assert.Contains(new KeyValuePair<string, int>("id", 1), jsonObj);
    }

    [Fact]
    public async Task Should_post_form_url_encoded_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post form URL encoded");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsPrettyText();
        Assert.Contains("application/x-www-form-urlencoded", bodyText);
        Assert.Contains("a=xyz&b=123&c=true&myIdSecret=789", bodyText);

        // let's change {{MyIdSecret}} to another value using PororocaTest
        this.pororocaTest.SetCollectionVariable("MyIdSecret", "999");

        // sending request with new {{MyIdSecret}} value
        res = await this.pororocaTest.SendHttpRequestAsync("Post form URL encoded");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        bodyText = res.GetBodyAsPrettyText();
        Assert.Contains("application/x-www-form-urlencoded", bodyText);
        Assert.Contains("a=xyz&b=123&c=true&myIdSecret=999", bodyText);
    }

    [Fact]
    public async Task Should_post_multipart_form_data_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("Post multipart form data");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsPrettyText();
        Assert.Contains("Content-Disposition: form-data; name=a", bodyText);
        Assert.Contains("Body: xyz", bodyText);
        Assert.Contains("Content-Disposition: form-data; name=b", bodyText);
        Assert.Contains("Body: {\"id\":2}", bodyText);
        Assert.Contains("Content-Disposition: form-data; name=myIdSecret", bodyText);
        Assert.Contains("Body: 789", bodyText);
    }

    [Fact]
    public async Task Should_send_basic_auth_header_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("BASIC");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsPrettyText();
        Assert.Contains("Basic dXNyOnB3ZA==", bodyText);
    }

    [Fact]
    public async Task Should_send_bearer_auth_header_with_http_1_1_successfully()
    {
        var res = await this.pororocaTest.SendHttpRequestAsync("BEARER");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        string? bodyText = res.GetBodyAsPrettyText();
        Assert.Contains("Bearer token_local", bodyText);

        // lets change {{BearerAuthToken}} to another value using PororocaTest
        this.pororocaTest.SetEnvironmentVariable("Local", "BearerAuthToken", "token_development");

        // xUnit tests run in sequence when they are in the same class
        // hence, no risk of causing troubles in other tests
        // https://xunit.net/docs/running-tests-in-parallel

        // sending request with new {{BearerAuthToken}} value
        res = await this.pororocaTest.SendHttpRequestAsync("BEARER");

        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", res.ContentType);

        bodyText = res.GetBodyAsPrettyText();
        Assert.Contains("Bearer token_development", bodyText);
    }
}