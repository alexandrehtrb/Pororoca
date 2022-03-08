using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaResponse
{
    public TimeSpan ElapsedTime { get; }

    public DateTime ReceivedAt { get; }

    public Exception? Exception { get; }

    public bool Success { get; }

    public HttpStatusCode? StatusCode { get; }

    public IEnumerable<PororocaKeyValueParam>? Headers { get; }

    private readonly byte[]? _binaryBody;
    
    public bool WasCancelled =>
        Exception is TaskCanceledException;

    public bool HasBody =>
        _binaryBody?.Length > 0;

    public string? ContentType
    {
        get
        {
            PororocaKeyValueParam? contentTypeHeaders = Headers?.FirstOrDefault(h => h.Key == "Content-Type");
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
        if (_binaryBody == null || _binaryBody.Length == 0)
        {
            return null;
        }
        else
        {
            string bodyStr = Encoding.UTF8.GetString(_binaryBody);
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
        _binaryBody;

    public string? GetContentDispositionFileName()
    {
        string? contentDispositionValue = Headers?.FirstOrDefault(h => h.Key == "Content-Disposition")?.Value;
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
        ElapsedTime = elapsedTime;
        ReceivedAt = DateTime.Now;
        Success = true;
        StatusCode = responseMessage.StatusCode;
        
        HttpHeaders nonContentHeaders = responseMessage.Headers;
        HttpHeaders contentHeaders = responseMessage.Content.Headers;

        Headers = nonContentHeaders.Concat(contentHeaders).Select(h => new PororocaKeyValueParam(true, h.Key, string.Join(';', h.Value)));

        this._binaryBody = binaryBody;
    }

    private PororocaResponse(TimeSpan elapsedTime, Exception exception)
    {
        ElapsedTime = elapsedTime;
        Success = false;
        Exception = exception;
    }
}