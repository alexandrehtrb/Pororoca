using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pororoca.Domain.Features.Common;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaResponse
{
    public TimeSpan ElapsedTime { get; }

    public DateTimeOffset ReceivedAt { get; }

    public Exception? Exception { get; }

    public bool Successful { get; }

    public HttpStatusCode? StatusCode { get; }

    public IEnumerable<KeyValuePair<string, string>>? Headers { get; }

    public IEnumerable<KeyValuePair<string, string>>? Trailers { get; }

    private readonly byte[]? binaryBody;

    public bool WasCancelled =>
        Exception is TaskCanceledException;

    public bool HasBody =>
        this.binaryBody?.Length > 0;

    public string? ContentType
    {
        get
        {
            var contentTypeHeaders = Headers?.FirstOrDefault(h => h.Key == "Content-Type");
            return contentTypeHeaders?.Value;
        }
    }

    public bool CanDisplayTextBody
    {
        get
        {
            string? contentType = ContentType;
            // Optimistic behavior, considering that if content type is not present, then probably is text
            // TODO: Check other charsets?
            return contentType == null || MimeTypesDetector.IsTextContent(contentType);
        }
    }

    public string? GetBodyAsText()
    {
        if (this.binaryBody == null || this.binaryBody.Length == 0)
        {
            return null;
        }
        else
        {
            string bodyStr = Encoding.UTF8.GetString(this.binaryBody);
            string? contentType = ContentType;
            if (contentType == null || !MimeTypesDetector.IsJsonContent(contentType))
            {
                return bodyStr;
            }
            else
            {
                try
                {
                    dynamic? jsonObj = JsonSerializer.Deserialize<dynamic>(bodyStr);
                    string prettyPrintJson = JsonSerializer.Serialize(jsonObj, options: ViewJsonResponseOptions);
                    return prettyPrintJson;
                }
                catch
                {
                    return bodyStr;
                }
            }
        }
    }

    public byte[]? GetBodyAsBinary() =>
        this.binaryBody;

    public T? GetJsonBodyAs<T>() =>
        GetJsonBodyAs<T>(MinifyingOptions);

    public T? GetJsonBodyAs<T>(JsonSerializerOptions jsonOptions) =>
        JsonSerializer.Deserialize<T>(this.binaryBody, jsonOptions);

    public string? GetContentDispositionFileName()
    {
        var contentDispositionHeader = Headers?.FirstOrDefault(h => h.Key == "Content-Disposition");
        string? contentDispositionValue = contentDispositionHeader?.Value;
        if (contentDispositionValue != null)
        {
            string[] contentDispositionParts = contentDispositionValue.Split("; ", StringSplitOptions.RemoveEmptyEntries);
            string? fileNamePart = contentDispositionParts.FirstOrDefault(p => p.StartsWith("filename"));
            if (fileNamePart != null)
            {
                string[] fileNamePartKv = fileNamePart.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (fileNamePartKv.Length == 2)
                {
                    return fileNamePartKv[1].Replace("\"", string.Empty);
                }
            }
        }
        return null;
    }

    public static async Task<PororocaResponse> SuccessfulAsync(TimeSpan elapsedTime, HttpResponseMessage responseMessage)
    {
        byte[] binaryBody = await responseMessage.Content.ReadAsByteArrayAsync();
        return new(elapsedTime, responseMessage, binaryBody);
    }

    public static PororocaResponse Failed(TimeSpan elapsedTime, Exception ex) =>
        new(elapsedTime, ex);

    private PororocaResponse(TimeSpan elapsedTime, HttpResponseMessage responseMessage, byte[] binaryBody)
    {
        static KeyValuePair<string, string> ConvertHeaderToKeyValuePair(KeyValuePair<string, IEnumerable<string>> header) =>
            new(header.Key, string.Join(';', header.Value));

        ElapsedTime = elapsedTime;
        ReceivedAt = DateTimeOffset.Now;
        Successful = true;
        StatusCode = responseMessage.StatusCode;

        HttpHeaders nonContentHeaders = responseMessage.Headers;
        HttpHeaders contentHeaders = responseMessage.Content.Headers;
        HttpHeaders trailingHeaders = responseMessage.TrailingHeaders;

        Headers = nonContentHeaders.Concat(contentHeaders).Select(ConvertHeaderToKeyValuePair);
        Trailers = trailingHeaders.Select(ConvertHeaderToKeyValuePair);

        this.binaryBody = binaryBody;
    }

    private PororocaResponse(TimeSpan elapsedTime, Exception exception)
    {
        ElapsedTime = elapsedTime;
        Successful = false;
        Exception = exception;
    }
}