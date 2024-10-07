using System.Collections.Frozen;
using System.Net;
using System.Text;
using System.Xml;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.VariableCapture;
using static Pororoca.Domain.Features.ResponseParsing.PororocaHttpMultipartResponseBodyReader;

namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public record PororocaHttpResponseMultipartPart(
    FrozenDictionary<string, string> Headers,
    byte[] BinaryBody)
{
    public bool IsTextContent =>
        MimeTypesDetector.IsTextContent(Headers?.FirstOrDefault(h => h.Key == "Content-Type").Value);
}

public sealed class PororocaHttpResponse
{
    public PororocaHttpRequest? ResolvedRequest { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public TimeSpan ElapsedTime { get; }

    public DateTimeOffset ReceivedAt => StartedAtUtc + ElapsedTime;

    public Exception? Exception { get; }

    public bool Successful { get; }

    public HttpStatusCode? StatusCode { get; }

    public FrozenDictionary<string, string>? Headers { get; }

    public FrozenDictionary<string, string>? Trailers { get; }

    public PororocaHttpResponseMultipartPart[]? MultipartParts { get; internal set; }

    private readonly byte[]? binaryBody;

    private XmlDocument? cachedXmlDoc;
    private XmlNamespaceManager? cachedXmlNsm;

    public bool WasCancelled =>
        Exception is TaskCanceledException;

    public bool FailedDueToTlsVerification =>
        // for HTTP/1.1 and HTTP/2, inner exception is AuthenticationException
        // for HTTP/3, inner exception is HttpRequestException
        Exception?.InnerException is Exception iex
        && iex.Message.Contains("remote certificate is invalid", StringComparison.InvariantCultureIgnoreCase);

    public bool HasBody =>
        this.binaryBody?.Length > 0;

    public string? ContentType
    {
        get
        {
            var contentTypeHeaders = Headers?.FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase));
            return contentTypeHeaders?.Value;
        }
    }

    public bool CanDisplayTextBody =>
        MimeTypesDetector.IsTextContent(ContentType)
     || (MultipartParts is not null && MultipartParts.Length > 0 && MultipartParts.All(p => p.IsTextContent));

    private static FrozenDictionary<string, string> MakeKvTable(IEnumerable<KeyValuePair<string, IEnumerable<string>>> input) =>
        input.ToFrozenDictionary(x => x.Key, x => string.Join(';', x.Value));

    public string? GetBodyAsString(string? nonUtf8BodyMessageToShow = null)
    {
        if (this.binaryBody == null || this.binaryBody.Length == 0)
        {
            return null;
        }
        else
        {
            try
            {
                return Encoding.UTF8.GetString(this.binaryBody);
            }
            catch
            {
                if (nonUtf8BodyMessageToShow is not null)
                {
                    try
                    {
                        return string.Format(nonUtf8BodyMessageToShow, GetBodyAsBinary()!.Length);
                    }
                    catch
                    {
                        return nonUtf8BodyMessageToShow;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public string? GetBodyAsPrettyText(string? nonUtf8BodyMessageToShow = null)
    {
        string? bodyStr = GetBodyAsString(nonUtf8BodyMessageToShow);
        if (bodyStr is null)
        {
            return null;
        }

        try
        {
            string? contentType = ContentType;
            if (contentType == null)
            {
                return bodyStr;
            }
            else if (MimeTypesDetector.IsJsonContent(contentType))
            {
                try
                {
                    return JsonUtils.PrettifyJson(bodyStr);
                }
                catch
                {
                    return bodyStr;
                }
            }
            else if (MimeTypesDetector.IsXmlContent(contentType))
            {
                try
                {
                    return XmlUtils.PrettifyXml(bodyStr);
                }
                catch
                {
                    return bodyStr;
                }
            }
            else
            {
                return bodyStr;
            }
        }
        catch
        {
            return bodyStr;
        }
    }

    public byte[]? GetBodyAsBinary() =>
        this.binaryBody;

    public string? GetContentDispositionFileName()
    {
        var contentDispositionHeader = Headers?.FirstOrDefault(h => h.Key.Equals("Content-Disposition", StringComparison.InvariantCultureIgnoreCase));
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

    public string? CaptureValue(PororocaHttpResponseValueCapture capture)
    {
        if (capture.Type == PororocaHttpResponseValueCaptureType.Header)
        {
            // HTTP/2 lower-cases all header names, hence, we need to compare header names ignoring case
            return Headers?.FirstOrDefault(x => x.Key.Equals(capture.HeaderName, StringComparison.InvariantCultureIgnoreCase)).Value;
        }
        else if (capture.Type == PororocaHttpResponseValueCaptureType.Body)
        {
            bool isJsonBody = MimeTypesDetector.IsJsonContent(ContentType ?? string.Empty);
            bool isXmlBody = MimeTypesDetector.IsXmlContent(ContentType ?? string.Empty);
            if (isJsonBody)
            {
                string body = GetBodyAsString() ?? string.Empty;
                return PororocaResponseValueCapturer.CaptureJsonValue(capture.Path!, body);
            }
            else if (isXmlBody)
            {
                string body = GetBodyAsString() ?? string.Empty;
                // holding the doc and nsm here to spare processing
                // of reading and parsing XML document and namespaces
                // if cachedXmlDocAndNsm is not null (already loaded),
                // then it won't be loaded again
                if (this.cachedXmlDoc is null || this.cachedXmlNsm is null)
                {
                    XmlUtils.LoadXmlDocumentAndNamespaceManager(body, out this.cachedXmlDoc, out this.cachedXmlNsm);
                }

                if (this.cachedXmlDoc is not null && this.cachedXmlNsm is not null)
                {
                    return PororocaResponseValueCapturer.CaptureXmlValue(capture.Path!, this.cachedXmlDoc, this.cachedXmlNsm);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public static async Task<PororocaHttpResponse> SuccessfulAsync(PororocaHttpRequest resolvedReq, DateTimeOffset startedAt, TimeSpan elapsedTime, HttpResponseMessage responseMessage)
    {
        // the binaryBody needs to be read before making the table of headers,
        // otherwise, the Content-Length header disappears for some reason (???)
        byte[] binaryBody = await responseMessage.Content.ReadAsByteArrayAsync();

        var nonContentHeaders = responseMessage.Headers;
        var contentHeaders = responseMessage.Content.Headers;

        var headers = nonContentHeaders.Concat(contentHeaders);
        var trailers = responseMessage.TrailingHeaders;

        PororocaHttpResponse res = new(resolvedReq, startedAt, elapsedTime, responseMessage.StatusCode, MakeKvTable(headers), MakeKvTable(trailers), binaryBody);

        if (res.ContentType?.StartsWith("multipart") == true)
        {
            string boundary = ReadMultipartBoundary(res.ContentType);
            res.MultipartParts = ReadMultipartResponseParts(binaryBody, boundary);
        }

        return res;
    }

    public static PororocaHttpResponse Failed(PororocaHttpRequest? resolvedReq, DateTimeOffset startedAt, TimeSpan elapsedTime, Exception ex) =>
        new(resolvedReq, startedAt, elapsedTime, ex);

    internal PororocaHttpResponse(PororocaHttpRequest? resolvedReq, DateTimeOffset startedAt, TimeSpan elapsedTime, HttpStatusCode httpStatusCode, FrozenDictionary<string, string> headers, FrozenDictionary<string, string> trailers, byte[] binaryBody)
    {
        ResolvedRequest = resolvedReq;
        StartedAtUtc = startedAt;
        ElapsedTime = elapsedTime;
        Successful = true;
        StatusCode = httpStatusCode;
        Headers = headers;
        Trailers = trailers;
        this.binaryBody = binaryBody;
    }

    private PororocaHttpResponse(PororocaHttpRequest? resolvedReq, DateTimeOffset startedAt, TimeSpan elapsedTime, Exception exception)
    {
        StartedAtUtc = startedAt;
        ResolvedRequest = resolvedReq;
        ElapsedTime = elapsedTime;
        Successful = false;
        Exception = exception;
    }
}