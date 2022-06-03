using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Pororoca.TestServer.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger) => this._logger = logger;

    #region GET

    [Route("get/json")]
    [HttpGet]
    public IActionResult TestGetJson() =>
        Ok(new { id = 1 });

    [Route("get/img")]
    [HttpGet]
    public IActionResult TestGetImg()
    {
        const string fileName = "pirate.gif";
        string testFilePath = GetTestFilePath(fileName);
        return File(System.IO.File.OpenRead(testFilePath), "image/gif");
    }

    [Route("get/txt")]
    [HttpGet]
    public IActionResult TestGetTxt()
    {
        const string fileName = "ascii.txt";
        string testFilePath = GetTestFilePath(fileName);
        return File(System.IO.File.OpenRead(testFilePath), "text/plain", fileName);
    }

    [Route("get/headers")]
    [HttpGet]
    public IActionResult TestGetHeaders() =>
        Ok(Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value));

    #endregion

    #region POST

    [Route("post/none")]
    [HttpPost]
    public IActionResult TestPostNone() =>
        NoContent();

    [Route("post/json")]
    [HttpPost]
    public IActionResult TestPostJson(dynamic obj) =>
        Ok(obj);

    [Route("post/file")]
    [HttpPost]
    public async Task<IActionResult> TestPostFile()
    {
        string? contentType = Request.ContentType;
        string? contentDisposition = Request.Headers.ContentDisposition.ToString();
        using MemoryStream ms = new();
        await Request.Body.CopyToAsync(ms);
        long bodyLength = ms.Length;
        return Ok($"Received file:\nContent-Type:{contentType}\nContent-Disposition:{contentDisposition}\nBody length: {bodyLength} bytes");
    }

    [Route("post/urlencoded")]
    [HttpPost]
    public async Task<IActionResult> TestPostUrlEncoded()
    {
        string? contentType = Request.ContentType;
        using StreamReader sr = new(Request.Body, Encoding.UTF8);
        string body = await sr.ReadToEndAsync();
        return Ok($"Received URL encoded params:\nContent-Type:{contentType}\nBody: {body}");
    }

    [Route("post/multipartformdata")]
    [HttpPost]
    public async Task<IActionResult> TestPostMultipartFormData()
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            return BadRequest("Error: not a multipart request!");
        }

        StringBuilder sb = new("Received multipart request:\n");

        string boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), 10000);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            string? sectionContentType = section.ContentType;
            string? sectionContentDisposition = section.ContentDisposition?.ToString();
            using StreamReader sr = new(section.Body, Encoding.UTF8);
            string sectionBody = await sr.ReadToEndAsync();
            sb.AppendLine($"Section:\n\tContent-Type:{sectionContentType}\n\tContent-Disposition:{sectionContentDisposition}\n\tBody: {sectionBody}");

            section = await reader.ReadNextSectionAsync();
        }

        return Ok(sb.ToString());
    }

    #endregion

    #region AUTH

    [Route("auth")]
    [HttpGet]
    public IActionResult TestAuthHeader()
    {
        string authHeader = Request.Headers.Authorization.ToString();
        return Ok(authHeader);
    }

    #endregion

    private static string GetTestFilePath(string testFileName)
    {
        string rootPath = new DirectoryInfo(Directory.GetCurrentDirectory()).FullName;
        return Path.Combine(rootPath, "TestFiles", testFileName);
    }
}