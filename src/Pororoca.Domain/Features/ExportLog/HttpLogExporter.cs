using System.Globalization;
using System.Net;
using System.Text;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using static Pororoca.Domain.Features.Common.HttpVersionFormatter;
using static Pororoca.Domain.Features.ResponseParsing.PororocaHttpMultipartResponseBodyReader;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using static Pororoca.Domain.Features.TranslateRequest.Http.PororocaHttpRequestTranslator;

namespace Pororoca.Domain.Features.ExportLog;

public static class HttpLogExporter
{
    public static string ProduceHttpLog(PororocaHttpResponse res)
    {
        StringBuilder sb = new();
        sb.Append(ProduceHttpLogPartTop(res));
        sb.Append(ProduceHttpLogPartRequest(res));
        sb.Append(ProduceHttpLogPartResponse(res));
        return sb.ToString();
    }

    internal static StringBuilder ProduceHttpLogPartTop(PororocaHttpResponse res)
    {
        StringBuilder sb = new("--------------- POROROCA HTTP LOG ----------------");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Started at: " + res.StartedAtUtc.ToString("dddd, dd MMMM yyyy HH':'mm':'ss'.'fff 'GMT'zzz", CultureInfo.InvariantCulture));
        sb.AppendLine("Elapsed time: " + res.ElapsedTime);
        sb.AppendLine("(binary contents depicted as base-64 strings)");
        sb.AppendLine();
        return sb;
    }

    internal static StringBuilder ProduceHttpLogPartRequest(PororocaHttpResponse res, Guid? multipartFormDataBoundary = null)
    {
        var req = res.ResolvedRequest!;
        Uri.TryCreate(req.Url, UriKind.Absolute, out var reqUri);
        string formattedHttpVersion = FormatHttpVersion(req.HttpVersion);
        string? customAuthHeaderValue = MakeCustomAuthHeaderValue(req.CustomAuth);
        string? reqBodyStr = GetRequestBodyAsString(req, out string? reqBodyContentType, out int reqBodySize, multipartFormDataBoundary);

        StringBuilder sb = new("-------------------- REQUEST ---------------------");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"{req.HttpMethod} {reqUri?.PathAndQuery} {formattedHttpVersion}");
        sb.AppendLine($"Host: {reqUri?.Host}");
        sb.AppendLine($"Accept-Encoding: gzip, deflate, br");
        if (customAuthHeaderValue is not null)
        {
            sb.AppendLine($"Authorization: {customAuthHeaderValue}");
        }
        if (req.Headers is not null && req.Headers.Count > 0)
        {
            foreach (var header in req.Headers)
            {
                sb.AppendLine($"{header.Key}: {header.Value}");
            }
        }
        if (reqBodyStr is not null)
        {
            sb.AppendLine($"Content-Type: {reqBodyContentType}");
            sb.AppendLine($"Content-Length: {reqBodySize}");
            sb.AppendLine();
            sb.AppendLine(reqBodyStr);
        }
        sb.AppendLine();
        return sb;
    }

    internal static StringBuilder ProduceHttpLogPartResponse(PororocaHttpResponse res)
    {
        var req = res.ResolvedRequest!;
        string formattedHttpVersion = FormatHttpVersion(req.HttpVersion);
        string multipartBoundary = ReadMultipartBoundary(res.ContentType ?? string.Empty);
        string? resBodyStr = GetResponseBodyAsString(res, multipartBoundary);

        StringBuilder sb = new("-------------------- RESPONSE --------------------");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"{formattedHttpVersion} {(int)res.StatusCode!} {Enum.GetName((HttpStatusCode)res.StatusCode)}");
        if (res.Headers is not null && res.Headers.Count > 0)
        {
            foreach (var header in res.Headers)
            {
                sb.AppendLine($"{header.Key}: {header.Value}");
            }
        }
        if (resBodyStr is not null)
        {
            sb.AppendLine();
            sb.Append(resBodyStr);
            sb.AppendLine();
        }
        if (res.Trailers is not null && res.Trailers.Count > 0)
        {
            sb.AppendLine();
            foreach (var trailer in res.Trailers)
            {
                sb.AppendLine($"{trailer.Key}: {trailer.Value}");
            }
        }
        return sb;
    }

    private static string? GetRequestBodyAsString(PororocaHttpRequest req, out string? reqBodyContentType, out int reqBodySize, Guid? multipartFormDataBoundary = null) =>
        req.Body?.Mode switch
        {
            PororocaHttpRequestBodyMode.File => GetFileRequestBodyAsString(req, out reqBodyContentType, out reqBodySize),
            PororocaHttpRequestBodyMode.FormData => GetFormDataRequestBodyAsString(req, out reqBodyContentType, out reqBodySize, multipartFormDataBoundary ?? Guid.NewGuid()),
            _ => GetOtherRequestBodyAsString(req, out reqBodyContentType, out reqBodySize)
        };

    private static string? GetFileRequestBodyAsString(PororocaHttpRequest req, out string? reqBodyContentType, out int reqBodySize)
    {
        reqBodyContentType = req.Body!.ContentType;

        if (File.Exists(req.Body.FileSrcPath!) == false)
        {
            reqBodySize = 0;
            return "(file disappeared)";
        }
        else
        {
            FileInfo fi = new(req.Body.FileSrcPath!);
            reqBodySize = (int)fi.Length;
        }

        if (MimeTypesDetector.IsTextContent(reqBodyContentType))
        {
            return File.ReadAllText(req.Body.FileSrcPath!);
        }
        else
        {
            return Convert.ToBase64String(File.ReadAllBytes(req.Body.FileSrcPath!));
        }
    }

    private static string? GetFormDataRequestBodyAsString(PororocaHttpRequest req, out string? reqBodyContentType, out int reqBodySize, Guid multipartFormDataBoundary)
    {
        reqBodyContentType = "multipart/form-data";
        reqBodySize = 0;

        string boundary = "--" + multipartFormDataBoundary.ToString();
        StringBuilder sbTotal = new();

        foreach (var fdp in req.Body!.FormDataValues!)
        {
            if (fdp.Type == PororocaHttpRequestFormDataParamType.Text)
            {
                StringBuilder sbPart = new();
                sbPart.AppendLine(boundary);
                sbPart.AppendLine("Content-Type: " + fdp.ContentType + "; charset=utf-8");
                sbPart.AppendLine("Content-Disposition: form-data; name=" + fdp.Key);
                sbPart.AppendLine();
                sbPart.AppendLine(fdp.TextValue);
                string part = sbPart.ToString();
                reqBodySize += Encoding.UTF8.GetBytes(part).Length;
                sbTotal.Append(part);
            }
            else if (fdp.Type == PororocaHttpRequestFormDataParamType.File)
            {
                string fileName = Path.GetFileName(fdp.FileSrcPath!)!;
                StringBuilder sbPart = new();
                sbPart.AppendLine(boundary);
                sbPart.AppendLine("Content-Type: " + fdp.ContentType);
                sbPart.AppendLine("Content-Disposition: form-data; name=" + fdp.Key + "; filename=" + fileName + "; filename*=utf-8''" + fileName);
                sbPart.AppendLine();
                string partBoundaryAndHeaders = sbPart.ToString();
                reqBodySize += Encoding.UTF8.GetBytes(partBoundaryAndHeaders).Length;
                sbTotal.Append(partBoundaryAndHeaders);

                if (File.Exists(fdp.FileSrcPath!))
                {
                    byte[] fileBytes = File.ReadAllBytes(fdp.FileSrcPath!);
                    reqBodySize += fileBytes.Length;
                    sbTotal.AppendLine(Convert.ToBase64String(fileBytes).TrimStart('\n').TrimStart('\r'));
                }
                else
                {
                    sbTotal.AppendLine("(file disappeared)");
                }
            }
        }

        sbTotal.Append(boundary);
        reqBodySize += Encoding.UTF8.GetBytes(boundary).Length;
        return sbTotal.ToString();
    }

    private static string? GetOtherRequestBodyAsString(PororocaHttpRequest req, out string? reqBodyContentType, out int reqBodySize)
    {
        if (req.Body is null)
        {
            reqBodyContentType = null;
            reqBodySize = 0;
            return null;
        }

        using var content = MakeRequestContent(req.Body, new());
        reqBodyContentType = content!.Headers.ContentType!.MediaType;
        string contentStr = content.ReadAsStringAsync().GetAwaiter().GetResult();
        reqBodySize = Encoding.UTF8.GetBytes(contentStr).Length;
        return contentStr;
    }

    private static string? GetResponseBodyAsString(PororocaHttpResponse res, string? multipartBoundary)
    {
        if (res.GetBodyAsBinary()?.Length == 0)
        {
            return null;
        }
        else if (MimeTypesDetector.IsTextContent(res.ContentType))
        {
            return res.GetBodyAsString();
        }
        else if (res.MultipartParts is not null)
        {
            return GetFormDataResponseBodyAsString(res, multipartBoundary!);
        }
        else
        {
            return Convert.ToBase64String(res.GetBodyAsBinary()!);
        }
    }

    private static string? GetFormDataResponseBodyAsString(PororocaHttpResponse res, string boundary)
    {
        StringBuilder sbTotal = new();

        foreach (var fdp in res.MultipartParts!)
        {
            StringBuilder sbPart = new();
            sbPart.AppendLine(boundary);
            foreach (var header in fdp.Headers)
            {
                sbPart.AppendLine($"{header.Key}: {header.Value}");
            }
            sbPart.AppendLine();
            bool isTextContent = fdp.Headers.Any(x => x.Key.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase) && MimeTypesDetector.IsTextContent(x.Value));
            if (isTextContent)
            {
                sbPart.AppendLine(Encoding.UTF8.GetString(fdp.BinaryBody!));
            }
            else
            {
                sbPart.AppendLine(Convert.ToBase64String(fdp.BinaryBody!));
            }
            sbTotal.Append(sbPart);
        }

        sbTotal.Append(boundary);
        return sbTotal.ToString();
    }

}