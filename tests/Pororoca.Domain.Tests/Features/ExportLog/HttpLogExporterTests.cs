using System.Collections.Frozen;
using System.Net;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;
using static Pororoca.Domain.Features.ExportLog.HttpLogExporter;

namespace Pororoca.Domain.Tests.Features.ExportLog;

public static class HttpLogExporterTests
{
    [Fact]
    public static void Should_produce_http_log_top_part_correctly()
    {
        // GIVEN
        var res = GenerateResponse();

        // WHEN, THEN
        Assert.Equal(
@"--------------- POROROCA HTTP LOG ----------------

Started at: Wednesday, 06 December 2023 17:17:46.503 GMT-03:00
Elapsed time: 00:00:01.3944318
(binary contents depicted as base-64 strings)

", ProduceHttpLogPartTop(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_nothing_correctly()
    {
        // GIVEN
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://localhost:5000/test/post/none"));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /test/post/none HTTP/1.1
Host: localhost
Accept-Encoding: gzip, deflate, br

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_custom_auth_correctly()
    {
        // GIVEN
        var res = GenerateResponse(resolvedReq: new("",
            Url: "https://httpbin.org/headers",
            CustomAuth: PororocaRequestAuth.MakeBasicAuth("usr", "pwd")));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

GET /headers HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Authorization: Basic dXNyOnB3ZA==

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_headers_correctly()
    {
        // GIVEN
        var res = GenerateResponse(resolvedReq: new("",
            Url: "https://httpbin.org/headers",
            Headers: [new(true, "MyHeader", "MyHeaderValue")]));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

GET /headers HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
MyHeader: MyHeaderValue

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_raw_text_body_correctly()
    {
        // GIVEN
        var body = MakeRawContent("oi", "text/plain");
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Body: body));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Content-Type: text/plain
Content-Length: 2

oi

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_file_text_body_correctly()
    {
        // GIVEN
        var body = MakeFileContent(GetTestFilePath("testfilecontent1.json"), "application/json");
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Body: body));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Content-Type: application/json
Content-Length: 8

{""id"":1}

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_file_binary_body_correctly()
    {
        // GIVEN
        var body = MakeFileContent(GetTestFilePath("pirate.gif"), "image/gif");
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Body: body));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Content-Type: image/gif
Content-Length: 888

R0lGODlhQABAAMQRAAAAAAgIAAgQGBAQEFoAAIRjOZR7Sq0ICLUAAL29vcalY86le+fe3vf39//OnP//AP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQFDwARACwAAAAAQABAAAAF/iAkjmRpnmiqrmzrvnAsz3Rt33iu7zPg/z9eDkgEwnxCUXFpZPkcyB1zGk0BoFiAbjriop7X8JAJ0ZrJpt9iAWBDcd4uutRmr9fvG7VcjpPaDnd4WjVUVYaEXQ6BeHk9e31BkXNli5ZshYh0mkqAlomPXkSdoglAi6AyiKtFZQkMsLCjegZXlre4uAC1f6+xvwypoVO5n7xdvsCycLbFq73Jv1U0np8/1qeXoAAECMrCqmu2AAUiBeNEqIHb3d7SQ26LfRDY11CYf+0ICADBUrfz6j1BRUffvmlDLAVElc5RF4MEwOkpdiubwzL7MvJLUinXgkUKnP3RmFEiHGfN/iqOJIkgIseBFHNFAcCypUkbNFOmVPeJH02WBFwK+QkTSkOGNfcFvUmtXURcgnQZ1Lg0CTeq1YopuFqz6tCp/Xx8xLU1bFehQ1mGDbYkWDCgaHlw1djPrTK7PzN6tVqzrl1psfK2jJsWqF+/f+fufUky6FJZgIMFVUqYb0nKEX1I8+G4M1M4evN6ltYZ82c9oak+DuxY9WmcqdVKO/u6UOx9Dx7wm53bNceVNjX2Rkyzt97KhYPLDkz7d5fbCIbz1n28NjXo0pkbp2y9B/SSs+F2V4VdN/Ho1Lk7V1J+t/b0g8fHEC0YPPOu8o+0P79daf4XfpmSV3ayoNfYfy0gS8aAKcsViN96CjKIm3nTHQghMBIayB91nkG4RGsHhTdYhxCu09mJxJ1IonNXsMEZisRxA+N6XdgBhGMKSrZUEzSW4SIrPwjAI0chAAAh+QQFDwARACwTAAcAGwAeAAAFhGAkjmQpAoAIQWbbok66rm4NxM44s3U53DfdrFdCLRaRRXBHHAGUyGSM2YzcjsnIdFi9OY4Lx5bXaD7FuRM30A2LreuqdS7kyQEJholqzhf5RHh6JACAPYKDalx9eYmFNHJWBAgRBwiPkHeTTouRf3aef6GjpKUjBKapqqusra49B6EhAAA7

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_file_binary_body_file_disappeared_correctly()
    {
        // GIVEN
        var body = MakeFileContent(GetTestFilePath("pirate1.gif"), "image/gif");
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Body: body));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Content-Type: image/gif
Content-Length: 0

(file disappeared)

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_url_encoded_text_body_correctly()
    {
        // GIVEN
        var body = MakeUrlEncodedContent([
            new(true, "a", "xyz"),
            new(true, "b", "123"),
            new(true, "c", "true à é"),
            new(true, "myIdSecret", "456")]);
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Body: body));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Content-Type: application/x-www-form-urlencoded
Content-Length: 47

a=xyz&b=123&c=true+%C3%A0+%C3%A9&myIdSecret=456

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_formdata_with_text_and_file_body_correctly()
    {
        // GIVEN
        var body = MakeFormDataContent([
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "a", "xyz", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "b", "{\"id\":2}", "application/json"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "myIdSecret", "456", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeFileParam(true, "arq", GetTestFilePath("pirate.gif"), "image/gif")
        ]);
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Body: body));

        var boundary = Guid.Parse("9e0248d8-f117-404e-a46b-51f0382d00bd");

        // WHEN, THEN
        int contentLength = OperatingSystem.IsWindows() ? 1489 : 1470;
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Content-Type: multipart/form-data
Content-Length: " + contentLength + @"

--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=a

xyz
--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: application/json; charset=utf-8
Content-Disposition: form-data; name=b

{""id"":2}
--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=myIdSecret

456
--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: image/gif
Content-Disposition: form-data; name=arq; filename=pirate.gif; filename*=utf-8''pirate.gif

R0lGODlhQABAAMQRAAAAAAgIAAgQGBAQEFoAAIRjOZR7Sq0ICLUAAL29vcalY86le+fe3vf39//OnP//AP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQFDwARACwAAAAAQABAAAAF/iAkjmRpnmiqrmzrvnAsz3Rt33iu7zPg/z9eDkgEwnxCUXFpZPkcyB1zGk0BoFiAbjriop7X8JAJ0ZrJpt9iAWBDcd4uutRmr9fvG7VcjpPaDnd4WjVUVYaEXQ6BeHk9e31BkXNli5ZshYh0mkqAlomPXkSdoglAi6AyiKtFZQkMsLCjegZXlre4uAC1f6+xvwypoVO5n7xdvsCycLbFq73Jv1U0np8/1qeXoAAECMrCqmu2AAUiBeNEqIHb3d7SQ26LfRDY11CYf+0ICADBUrfz6j1BRUffvmlDLAVElc5RF4MEwOkpdiubwzL7MvJLUinXgkUKnP3RmFEiHGfN/iqOJIkgIseBFHNFAcCypUkbNFOmVPeJH02WBFwK+QkTSkOGNfcFvUmtXURcgnQZ1Lg0CTeq1YopuFqz6tCp/Xx8xLU1bFehQ1mGDbYkWDCgaHlw1djPrTK7PzN6tVqzrl1psfK2jJsWqF+/f+fufUky6FJZgIMFVUqYb0nKEX1I8+G4M1M4evN6ltYZ82c9oak+DuxY9WmcqdVKO/u6UOx9Dx7wm53bNceVNjX2Rkyzt97KhYPLDkz7d5fbCIbz1n28NjXo0pkbp2y9B/SSs+F2V4VdN/Ho1Lk7V1J+t/b0g8fHEC0YPPOu8o+0P79daf4XfpmSV3ayoNfYfy0gS8aAKcsViN96CjKIm3nTHQghMBIayB91nkG4RGsHhTdYhxCu09mJxJ1IonNXsMEZisRxA+N6XdgBhGMKSrZUEzSW4SIrPwjAI0chAAAh+QQFDwARACwTAAcAGwAeAAAFhGAkjmQpAoAIQWbbok66rm4NxM44s3U53DfdrFdCLRaRRXBHHAGUyGSM2YzcjsnIdFi9OY4Lx5bXaD7FuRM30A2LreuqdS7kyQEJholqzhf5RHh6JACAPYKDalx9eYmFNHJWBAgRBwiPkHeTTouRf3aef6GjpKUjBKapqqusra49B6EhAAA7
--9e0248d8-f117-404e-a46b-51f0382d00bd

", ProduceHttpLogPartRequest(res, boundary).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_formdata_with_text_and_file_body_file_disappeared_correctly()
    {
        // GIVEN
        var body = MakeFormDataContent([
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "a", "xyz", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "b", "{\"id\":2}", "application/json"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "myIdSecret", "456", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeFileParam(true, "arq", GetTestFilePath("pirate1.gif"), "image/gif")
        ]);
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Body: body));
        var boundary = Guid.Parse("9e0248d8-f117-404e-a46b-51f0382d00bd");

        // WHEN, THEN
        int contentLength = OperatingSystem.IsWindows() ? 603 : 584;
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Content-Type: multipart/form-data
Content-Length: " + contentLength + @"

--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=a

xyz
--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: application/json; charset=utf-8
Content-Disposition: form-data; name=b

{""id"":2}
--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=myIdSecret

456
--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: image/gif
Content-Disposition: form-data; name=arq; filename=pirate1.gif; filename*=utf-8''pirate1.gif

(file disappeared)
--9e0248d8-f117-404e-a46b-51f0382d00bd

", ProduceHttpLogPartRequest(res, boundary).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_graphql_text_body_correctly()
    {
        // GIVEN
        var body = MakeGraphQlContent("myquery", "{\"id\":17}");
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://fruits-api.netlify.app/graphql",
            Body: body));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /graphql HTTP/1.1
Host: fruits-api.netlify.app
Accept-Encoding: gzip, deflate, br
Content-Type: application/json
Content-Length: 41

{""query"":""myquery"",""variables"":{""id"":17}}

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_request_part_with_custom_auth_custom_headers_and_body_correctly()
    {
        // GIVEN
        var body = MakeRawContent("oi", "text/plain");
        var res = GenerateResponse(resolvedReq: new("",
            HttpMethod: "POST",
            Url: "https://httpbin.org/anything",
            Headers: [new(true, "MyHeader", "MyHeaderValue")],
            Body: body,
            CustomAuth: PororocaRequestAuth.MakeBearerAuth("my_token")));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- REQUEST ---------------------

POST /anything HTTP/1.1
Host: httpbin.org
Accept-Encoding: gzip, deflate, br
Authorization: Bearer my_token
MyHeader: MyHeaderValue
Content-Type: text/plain
Content-Length: 2

oi

", ProduceHttpLogPartRequest(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_response_part_with_nothing_correctly()
    {
        // GIVEN
        var res = GenerateResponse(statusCode: HttpStatusCode.NoContent);

        // WHEN, THEN
        Assert.Equal(
@"-------------------- RESPONSE --------------------

HTTP/1.1 204 NoContent
", ProduceHttpLogPartResponse(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_response_part_with_headers_correctly()
    {
        // GIVEN
        var res = GenerateResponse(statusCode: HttpStatusCode.NoContent,
                            responseHeaders: new() {
                                {"Date", "Wed, 06 Dec 2023 18:51:53 GMT"},
                                {"Server", "Kestrel"}
                            });

        // WHEN, THEN
        Assert.Equal(
@"-------------------- RESPONSE --------------------

HTTP/1.1 204 NoContent
Date: Wed, 06 Dec 2023 18:51:53 GMT
Server: Kestrel
", ProduceHttpLogPartResponse(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_response_part_with_text_body_correctly()
    {
        // GIVEN
        var res = GenerateResponse(statusCode: HttpStatusCode.OK,
                            responseHeaders: new() {
                                {"Date", "Wed, 06 Dec 2023 18:51:53 GMT"},
                                {"Server", "Kestrel"},
                                {"Content-Type", "text/plain"},
                                {"Content-Length", "2"}
                            },
                            binaryBody: "oi"u8.ToArray());

        // WHEN, THEN
        Assert.Equal(
@"-------------------- RESPONSE --------------------

HTTP/1.1 200 OK
Date: Wed, 06 Dec 2023 18:51:53 GMT
Server: Kestrel
Content-Type: text/plain
Content-Length: 2

oi
", ProduceHttpLogPartResponse(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_response_part_with_binary_body_correctly()
    {
        // GIVEN
        var res = GenerateResponse(statusCode: HttpStatusCode.OK,
                            responseHeaders: new() {
                                {"Date", "Wed, 06 Dec 2023 18:51:53 GMT"},
                                {"Server", "Kestrel"},
                                {"Content-Type", "image/gif"},
                                {"Content-Length", "888"},
                            },
                            binaryBody: File.ReadAllBytes(GetTestFilePath("pirate.gif")));

        // WHEN, THEN
        Assert.Equal(
@"-------------------- RESPONSE --------------------

HTTP/1.1 200 OK
Date: Wed, 06 Dec 2023 18:51:53 GMT
Server: Kestrel
Content-Type: image/gif
Content-Length: 888

R0lGODlhQABAAMQRAAAAAAgIAAgQGBAQEFoAAIRjOZR7Sq0ICLUAAL29vcalY86le+fe3vf39//OnP//AP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQFDwARACwAAAAAQABAAAAF/iAkjmRpnmiqrmzrvnAsz3Rt33iu7zPg/z9eDkgEwnxCUXFpZPkcyB1zGk0BoFiAbjriop7X8JAJ0ZrJpt9iAWBDcd4uutRmr9fvG7VcjpPaDnd4WjVUVYaEXQ6BeHk9e31BkXNli5ZshYh0mkqAlomPXkSdoglAi6AyiKtFZQkMsLCjegZXlre4uAC1f6+xvwypoVO5n7xdvsCycLbFq73Jv1U0np8/1qeXoAAECMrCqmu2AAUiBeNEqIHb3d7SQ26LfRDY11CYf+0ICADBUrfz6j1BRUffvmlDLAVElc5RF4MEwOkpdiubwzL7MvJLUinXgkUKnP3RmFEiHGfN/iqOJIkgIseBFHNFAcCypUkbNFOmVPeJH02WBFwK+QkTSkOGNfcFvUmtXURcgnQZ1Lg0CTeq1YopuFqz6tCp/Xx8xLU1bFehQ1mGDbYkWDCgaHlw1djPrTK7PzN6tVqzrl1psfK2jJsWqF+/f+fufUky6FJZgIMFVUqYb0nKEX1I8+G4M1M4evN6ltYZ82c9oak+DuxY9WmcqdVKO/u6UOx9Dx7wm53bNceVNjX2Rkyzt97KhYPLDkz7d5fbCIbz1n28NjXo0pkbp2y9B/SSs+F2V4VdN/Ho1Lk7V1J+t/b0g8fHEC0YPPOu8o+0P79daf4XfpmSV3ayoNfYfy0gS8aAKcsViN96CjKIm3nTHQghMBIayB91nkG4RGsHhTdYhxCu09mJxJ1IonNXsMEZisRxA+N6XdgBhGMKSrZUEzSW4SIrPwjAI0chAAAh+QQFDwARACwTAAcAGwAeAAAFhGAkjmQpAoAIQWbbok66rm4NxM44s3U53DfdrFdCLRaRRXBHHAGUyGSM2YzcjsnIdFi9OY4Lx5bXaD7FuRM30A2LreuqdS7kyQEJholqzhf5RHh6JACAPYKDalx9eYmFNHJWBAgRBwiPkHeTTouRf3aef6GjpKUjBKapqqusra49B6EhAAA7
", ProduceHttpLogPartResponse(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_response_part_with_form_data_text_and_file_body_correctly()
    {
        // GIVEN
        var res = GenerateResponse(statusCode: HttpStatusCode.OK,
                            responseHeaders: new() {
                                {"Date", "Wed, 06 Dec 2023 18:51:53 GMT"},
                                {"Server", "Kestrel"},
                                {"Content-Type", "multipart/form-data; boundary=\"9e0248d8-f117-404e-a46b-51f0382d00bd\""}
                            }, binaryBody: "bytes"u8.ToArray());
        Dictionary<string, string> headersTxt = new()
        {
            { "Content-Type", "text/plain; charset=utf-8" },
            { "Content-Disposition", "form-data; name=a" }
        };
        Dictionary<string, string> headersFile = new()
        {
            { "Content-Type", "image/gif" },
            { "Content-Disposition", "form-data; name=arq; filename=pirate.gif; filename*=utf-8''pirate.gif" }
        };
        res.MultipartParts = [
            new(headersTxt.ToFrozenDictionary(), "oi"u8.ToArray()),
            new(headersFile.ToFrozenDictionary(), File.ReadAllBytes(GetTestFilePath("pirate.gif")))
        ];

        // WHEN, THEN
        Assert.Equal(
@"-------------------- RESPONSE --------------------

HTTP/1.1 200 OK
Date: Wed, 06 Dec 2023 18:51:53 GMT
Server: Kestrel
Content-Type: multipart/form-data; boundary=""9e0248d8-f117-404e-a46b-51f0382d00bd""

--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: text/plain; charset=utf-8
Content-Disposition: form-data; name=a

oi
--9e0248d8-f117-404e-a46b-51f0382d00bd
Content-Type: image/gif
Content-Disposition: form-data; name=arq; filename=pirate.gif; filename*=utf-8''pirate.gif

R0lGODlhQABAAMQRAAAAAAgIAAgQGBAQEFoAAIRjOZR7Sq0ICLUAAL29vcalY86le+fe3vf39//OnP//AP///wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH/C05FVFNDQVBFMi4wAwEAAAAh+QQFDwARACwAAAAAQABAAAAF/iAkjmRpnmiqrmzrvnAsz3Rt33iu7zPg/z9eDkgEwnxCUXFpZPkcyB1zGk0BoFiAbjriop7X8JAJ0ZrJpt9iAWBDcd4uutRmr9fvG7VcjpPaDnd4WjVUVYaEXQ6BeHk9e31BkXNli5ZshYh0mkqAlomPXkSdoglAi6AyiKtFZQkMsLCjegZXlre4uAC1f6+xvwypoVO5n7xdvsCycLbFq73Jv1U0np8/1qeXoAAECMrCqmu2AAUiBeNEqIHb3d7SQ26LfRDY11CYf+0ICADBUrfz6j1BRUffvmlDLAVElc5RF4MEwOkpdiubwzL7MvJLUinXgkUKnP3RmFEiHGfN/iqOJIkgIseBFHNFAcCypUkbNFOmVPeJH02WBFwK+QkTSkOGNfcFvUmtXURcgnQZ1Lg0CTeq1YopuFqz6tCp/Xx8xLU1bFehQ1mGDbYkWDCgaHlw1djPrTK7PzN6tVqzrl1psfK2jJsWqF+/f+fufUky6FJZgIMFVUqYb0nKEX1I8+G4M1M4evN6ltYZ82c9oak+DuxY9WmcqdVKO/u6UOx9Dx7wm53bNceVNjX2Rkyzt97KhYPLDkz7d5fbCIbz1n28NjXo0pkbp2y9B/SSs+F2V4VdN/Ho1Lk7V1J+t/b0g8fHEC0YPPOu8o+0P79daf4XfpmSV3ayoNfYfy0gS8aAKcsViN96CjKIm3nTHQghMBIayB91nkG4RGsHhTdYhxCu09mJxJ1IonNXsMEZisRxA+N6XdgBhGMKSrZUEzSW4SIrPwjAI0chAAAh+QQFDwARACwTAAcAGwAeAAAFhGAkjmQpAoAIQWbbok66rm4NxM44s3U53DfdrFdCLRaRRXBHHAGUyGSM2YzcjsnIdFi9OY4Lx5bXaD7FuRM30A2LreuqdS7kyQEJholqzhf5RHh6JACAPYKDalx9eYmFNHJWBAgRBwiPkHeTTouRf3aef6GjpKUjBKapqqusra49B6EhAAA7
--9e0248d8-f117-404e-a46b-51f0382d00bd
", ProduceHttpLogPartResponse(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_response_part_with_trailers_correctly()
    {
        // GIVEN
        var res = GenerateResponse(statusCode: HttpStatusCode.NoContent,
                            responseHeaders: new() {
                                {"Date", "Wed, 06 Dec 2023 18:51:53 GMT"},
                                {"Server", "Kestrel"}
                            },
                            responseTrailers: new() {
                                {"MyTrailer", "MyTrailerValue"}
                            });

        // WHEN, THEN
        Assert.Equal(
@"-------------------- RESPONSE --------------------

HTTP/1.1 204 NoContent
Date: Wed, 06 Dec 2023 18:51:53 GMT
Server: Kestrel

MyTrailer: MyTrailerValue
", ProduceHttpLogPartResponse(res).ToString());
    }

    [Fact]
    public static void Should_produce_http_log_response_part_with_headers_body_and_trailers_correctly()
    {
        // GIVEN
        var res = GenerateResponse(statusCode: HttpStatusCode.OK,
                            responseHeaders: new() {
                                {"Date", "Wed, 06 Dec 2023 18:51:53 GMT"},
                                {"Server", "Kestrel"},
                                {"Content-Type", "text/plain"},
                                {"Content-Length", "2"},
                            },
                            responseTrailers: new() {
                                {"MyTrailer", "MyTrailerValue"}
                            },
                            binaryBody: "oi"u8.ToArray());

        // WHEN, THEN
        Assert.Equal(
@"-------------------- RESPONSE --------------------

HTTP/1.1 200 OK
Date: Wed, 06 Dec 2023 18:51:53 GMT
Server: Kestrel
Content-Type: text/plain
Content-Length: 2

oi

MyTrailer: MyTrailerValue
", ProduceHttpLogPartResponse(res).ToString());
    }

    private static PororocaHttpResponse GenerateResponse(PororocaHttpRequest? resolvedReq = null, HttpStatusCode statusCode = HttpStatusCode.OK, Dictionary<string, string>? responseHeaders = null, Dictionary<string, string>? responseTrailers = null, byte[]? binaryBody = null) => new(
        resolvedReq: resolvedReq ?? new(string.Empty),
        startedAt: DateTimeOffset.Parse("2023-12-06T17:17:46.503-03:00"),
        elapsedTime: TimeSpan.Parse("00:00:01.3944318"),
        httpStatusCode: statusCode,
        headers: responseHeaders?.ToFrozenDictionary() ?? new Dictionary<string, string>().ToFrozenDictionary(),
        trailers: responseTrailers?.ToFrozenDictionary() ?? new Dictionary<string, string>().ToFrozenDictionary(),
        binaryBody: binaryBody ?? Array.Empty<byte>());
}