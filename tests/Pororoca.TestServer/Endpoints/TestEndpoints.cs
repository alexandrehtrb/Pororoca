using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Pororoca.TestServer.Endpoints;

public static class TestEndpoints
{
    public static WebApplication MapTestEndpoints(this WebApplication app)
    {
        app.MapGet("test/get/json", TestGetJson);
        app.MapGet("test/get/xml", TestGetXml);
        app.MapGet("test/get/img", TestGetImg);
        app.MapGet("test/get/txt", TestGetTxt);
        app.MapGet("test/get/headers", TestGetHeaders);
        app.MapGet("test/get/trailers", TestGetTrailers);
        app.MapGet("test/get/multipartformdata", TestGetMultipartFormData);
        app.MapGet("test/auth", TestAuthHeader);
        app.MapGet("test/http1websocket", (Delegate)TestHttp1WebSocket);
        app.MapConnect("test/http2websocket", (Delegate)TestHttp2WebSocket);

        // HttpContext as a parameter makes some endpoints hidden in Swagger (?)
        app.MapPost("test/post/none", TestPostNone);
        app.MapPost("test/post/json", TestPostJson);
        app.MapPost("test/post/txt", TestPostTxt);
        app.MapPost("test/post/problemdetails", TestPostProblemDetails);
        app.MapPost("test/post/file", TestPostFile);
        app.MapPost("test/post/urlencoded", TestPostUrlEncoded);
        app.MapPost("test/post/multipartformdata", TestPostMultipartFormData);
        return app;
    }

    #region GET

    private static IResult TestGetJson() =>
        Results.Ok(new { id = 1 });

    private static IResult TestGetXml()
    {
        const string xml =
        @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
              <env:Body>
                  <xsi:response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                      <wsa:MyVal1>Alexandre</wsa:MyVal1>
                      <xsi:Value>
                          <wsa:MyVal2>123987456</wsa:MyVal2>
                      </xsi:Value>
                  </xsi:response>
              </env:Body>
          </env:Envelope>";
        return Results.Text(xml, "text/xml", Encoding.UTF8);
    }

    private static IResult TestGetImg()
    {
        const string fileName = "pirate.gif";
        string testFilePath = GetTestFilePath(fileName);
        return Results.File(File.OpenRead(testFilePath), "image/gif");
    }

    private static IResult TestGetTxt()
    {
        const string fileName = "ascii.txt";
        string testFilePath = GetTestFilePath(fileName);
        return Results.File(File.OpenRead(testFilePath), "text/plain", fileName);
    }

    private static IResult TestGetHeaders(HttpContext httpCtx)
    {
        foreach (var reqHeader in httpCtx.Request.Headers)
        {
            httpCtx.Response.Headers.Append($"MIRRORED-{reqHeader.Key}", reqHeader.Value);
        }
        return Results.NoContent();
    }

    private static async Task TestGetTrailers(HttpResponse httpRes)
    {
        httpRes.SupportsTrailers();
        httpRes.DeclareTrailer("MyTrailer");
        httpRes.StatusCode = (int)HttpStatusCode.OK;
        httpRes.Headers.ContentType = new("application/json; charset=utf-8");
        await httpRes.StartAsync();
        byte[] bytes = Encoding.UTF8.GetBytes("{\"id\":1}");
        await httpRes.BodyWriter.WriteAsync(bytes);
        httpRes.AppendTrailer("MyTrailer", new("MyTrailerValue"));
        await httpRes.CompleteAsync();
    }

    private static MultipartFormDataResult TestGetMultipartFormData() =>
        new()
        {
            new MultipartContent()
            {
                Name = "a",
                ContentType = "text/plain",
                FileName = null,
                Stream = new MemoryStream("oi"u8.ToArray())
            },
            new MultipartContent()
            {
                Name = "arq",
                ContentType = "image/gif",
                FileName = "pirate.gif",
                Stream = new FileStream(GetTestFilePath("pirate.gif"), FileMode.Open)
            }
        };

    #endregion

    #region POST

    private static IResult TestPostNone() =>
        Results.NoContent();

    private static IResult TestPostJson(dynamic obj) =>
        Results.Ok(obj);

    private static async Task<IResult> TestPostTxt(HttpRequest req)
    {
        string? contentType = req.ContentType;
        using MemoryStream ms = new();
        await req.Body.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        using StreamReader sr = new(ms, Encoding.UTF8);
        string body = await sr.ReadToEndAsync();

        return Results.Text(body, contentType, Encoding.UTF8);
    }

    private static IResult TestPostProblemDetails() =>
        Results.ValidationProblem(title: "Validation problem title",
                                  detail: "Validation problem detail",
                                  errors: new Dictionary<string, string[]>());

    private static async Task<string> TestPostFile(HttpRequest httpReq)
    {
        string? contentType = httpReq.ContentType;
        string? contentDisposition = httpReq.Headers.ContentDisposition.ToString();
        using MemoryStream ms = new();
        await httpReq.Body.CopyToAsync(ms);
        long bodyLength = ms.Length;
        return $"--- Received file ---\nContent-Type: {contentType}\nContent-Disposition: {contentDisposition}\nBody length: {bodyLength} bytes";
    }

    private static async Task<string> TestPostUrlEncoded(HttpRequest httpReq)
    {
        string? contentType = httpReq.ContentType;
        using StreamReader sr = new(httpReq.Body, Encoding.UTF8);
        string body = await sr.ReadToEndAsync();
        return $"--- Received URL encoded params ---\nContent-Type: {contentType}\nBody: {body}";
    }

    private static async Task<string> TestPostMultipartFormData(HttpRequest httpReq, HttpResponse httpRes)
    {
        if (!MultipartRequestHelper.IsMultipartContentType(httpReq.ContentType))
        {
            httpRes.StatusCode = (int)HttpStatusCode.BadRequest;
            return "Error: not a multipart request!";
        }

        StringBuilder sb = new("### Received multipart request ###\n");

        string boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(httpReq.ContentType), 10000);
        var reader = new MultipartReader(boundary, httpReq.Body);
        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            string? sectionContentType = section.ContentType;
            string? sectionContentDisposition = section.ContentDisposition?.ToString();
            using StreamReader sr = new(section.Body, Encoding.UTF8);
            string sectionBody = await sr.ReadToEndAsync();
            sb.AppendLine($"--- Section ---\n\tContent-Type: {sectionContentType}\n\tContent-Disposition: {sectionContentDisposition}\n\tBody: {sectionBody}");

            section = await reader.ReadNextSectionAsync();
        }

        return sb.ToString();
    }

    #endregion

    #region AUTH

    private static string TestAuthHeader(HttpContext httpCtx)
    {
        string authHeader = httpCtx.Request.Headers.Authorization.ToString();
        return authHeader;
    }

    #endregion

    #region WEBSOCKETS

    private static async Task<IResult> TestHttp1WebSocket(HttpContext httpCtx)
    {
        if (!httpCtx.WebSockets.IsWebSocketRequest)
        {
            return Results.BadRequest("Only WebSockets requests are accepted here!");
        }
        else
        {
            using var webSocket = await httpCtx.WebSockets.AcceptWebSocketAsync();
            TaskCompletionSource<object> socketFinishedTcs = new();

            await BackgroundWebSocketsProcessor.RegisterAndProcessAsync(webSocket, socketFinishedTcs);
            await socketFinishedTcs.Task;

            return Results.NoContent();
        }
    }

    private static async Task<IResult> TestHttp2WebSocket(HttpContext httpCtx)
    {
        if (httpCtx.Request.Protocol != "HTTP/2" || !httpCtx.WebSockets.IsWebSocketRequest)
        {
            return Results.BadRequest("Only HTTP/2 websocket requests are accepted here!");
        }
        else
        {
            using var webSocket = await httpCtx.WebSockets.AcceptWebSocketAsync();
            TaskCompletionSource<object> socketFinishedTcs = new();

            await BackgroundWebSocketsProcessor.RegisterAndProcessAsync(webSocket, socketFinishedTcs);
            await socketFinishedTcs.Task;

            return Results.NoContent();
        }
    }

    #endregion

    private static string GetTestFilePath(string testFileName)
    {
        string rootPath = new DirectoryInfo(Directory.GetCurrentDirectory()).FullName;
        return Path.Combine(rootPath, "TestFiles", testFileName);
    }
}