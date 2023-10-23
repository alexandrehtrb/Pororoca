using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.VariableCapture;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.Entities.Pororoca.Http;

public sealed class PororocaHttpResponse
{
    public TimeSpan ElapsedTime { get; }

    public DateTimeOffset ReceivedAt { get; }

    public Exception? Exception { get; }

    public bool Successful { get; }

    public HttpStatusCode? StatusCode { get; }

    public IEnumerable<KeyValuePair<string, string>>? Headers { get; }

    public IEnumerable<KeyValuePair<string, string>>? Trailers { get; }

    private readonly byte[]? binaryBody;

    private (XmlDocument xmlDoc, XmlNamespaceManager xmlNsm)? cachedXmlDocAndNsm;

    public bool WasCancelled =>
        Exception is TaskCanceledException;

    public bool FailedDueToTlsVerification =>
        Exception?.InnerException is AuthenticationException aex
        && aex.Message.Contains("remote certificate is invalid", StringComparison.InvariantCultureIgnoreCase);

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

    public bool CanDisplayTextBody =>
        MimeTypesDetector.IsTextContent(ContentType);

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
                    dynamic? jsonObj = JsonSerializer.Deserialize<dynamic>(bodyStr);
                    string prettyPrintJson = JsonSerializer.Serialize(jsonObj, options: ViewJsonResponseOptions);
                    return prettyPrintJson;
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
                    return PrettifyXml(bodyStr);
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
                this.cachedXmlDocAndNsm ??= PororocaResponseValueCapturer.LoadXmlDocumentAndNamespaceManager(body);
                if (this.cachedXmlDocAndNsm is not null)
                {
                    return PororocaResponseValueCapturer.CaptureXmlValue(capture.Path!, this.cachedXmlDocAndNsm.Value.xmlDoc, this.cachedXmlDocAndNsm.Value.xmlNsm);
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

    private static string PrettifyXml(string xml)
    {
        string result = xml;

        using MemoryStream mStream = new();
        using XmlTextWriter writer = new(mStream, Encoding.UTF8);
        XmlDocument document = new();

        try
        {
            // Load the XmlDocument with the XML.
            document.LoadXml(xml);

            writer.Formatting = Formatting.Indented;

            // Write the XML into a formatting XmlTextWriter
            document.WriteContentTo(writer);
            writer.Flush();
            mStream.Flush();

            // Have to rewind the MemoryStream in order to read
            // its contents.
            mStream.Position = 0;

            // Read MemoryStream contents into a StreamReader.
            using StreamReader sReader = new(mStream);

            // Extract the text from the StreamReader.
            string formattedXml = sReader.ReadToEnd();

            result = formattedXml;
        }
        catch (XmlException)
        {
            // Handle the exception
        }

        mStream.Close();
        writer.Close();

        return result;
    }

    public static async Task<PororocaHttpResponse> SuccessfulAsync(TimeSpan elapsedTime, HttpResponseMessage responseMessage)
    {
        byte[] binaryBody = await responseMessage.Content.ReadAsByteArrayAsync();
        return new(elapsedTime, responseMessage, binaryBody);
    }

    public static PororocaHttpResponse Failed(TimeSpan elapsedTime, Exception ex) =>
        new(elapsedTime, ex);

    private PororocaHttpResponse(TimeSpan elapsedTime, HttpResponseMessage responseMessage, byte[] binaryBody)
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

    private PororocaHttpResponse(TimeSpan elapsedTime, Exception exception)
    {
        ElapsedTime = elapsedTime;
        Successful = false;
        Exception = exception;
    }
}