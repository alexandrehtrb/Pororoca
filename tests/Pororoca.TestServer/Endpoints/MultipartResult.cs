using System.Collections.ObjectModel;
using Microsoft.Net.Http.Headers;

namespace Pororoca.TestServer.Endpoints;

#nullable disable warnings
public sealed class MultipartContent
{
    public string Name { get; set; }

    public string ContentType { get; set; }

    public string? FileName { get; set; }

    public Stream Stream { get; set; }
}
#nullable restore warnings

public sealed class MultipartFormDataResult : Collection<MultipartContent>, IResult
{
    private readonly MultipartFormDataContent content;

    public MultipartFormDataResult(string? boundary = null) =>
        this.content = new(boundary ?? Guid.NewGuid().ToString());

    public async Task ExecuteAsync(HttpContext context)
    {
        foreach (var item in this)
        {
            if (item.Stream != null)
            {
                var content = new StreamContent(item.Stream);

                if (item.ContentType != null)
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(item.ContentType);
                }

                content.Headers.ContentDisposition = new("form-data");
                content.Headers.ContentDisposition.Name = item.Name;

                if (item.FileName != null)
                {
                    content.Headers.ContentDisposition.FileName = item.FileName;
                }

                this.content.Add(content);
            }
        }

        context.Response.ContentLength = this.content.Headers.ContentLength;
        context.Response.ContentType = this.content.Headers.ContentType?.ToString();

        await this.content.CopyToAsync(context.Response.Body);
        this.content.Dispose();
    }
}