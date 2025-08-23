using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net.Quic;
using Pororoca.Domain.Features.TranslateRequest;

namespace Pororoca.Domain.Features.Common;

[ExcludeFromCodeCoverage(Justification = "This class only holds available options and the check HTTP versions methods depend on the machine.")]
public static class AvailablePororocaRequestSelectionOptions
{
    public static readonly ImmutableList<HttpMethod> AvailableHttpMethods =
    [
        HttpMethod.Get,
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Patch,
        HttpMethod.Delete,
        HttpMethod.Head,
        HttpMethod.Options,
        HttpMethod.Connect,
        HttpMethod.Trace
    ];

    public static readonly ImmutableList<decimal> AvailableHttpVersionsForHttp =
    [
        1.0m,
        1.1m,
        2.0m,
        3.0m
    ];

    public static readonly ImmutableList<decimal> AvailableHttpVersionsForWebSockets =
    [
        1.1m,
        2.0m
    ];

    public static bool IsHttpVersionAvailableInOS(decimal httpVersion, out string? errorCode)
    {
#pragma warning disable CA1416
        if (httpVersion == 3.0m && !(OperatingSystem.IsLinux() || QuicConnection.IsSupported))
#pragma warning restore CA1416
        {
            // https://docs.microsoft.com/en-us/windows/win32/sysinfo/operating-system-version
            // https://en.wikipedia.org/wiki/Windows_11_version_history
            // https://devblogs.microsoft.com/dotnet/http-3-support-in-dotnet-6/#prerequisites
            // Windows 11 Build 22000 still uses major version as 10, but build number is 22000 or higher
            // Min Windows version retrieved from:
            // https://github.com/dotnet/runtime/blob/34ea77705ba9a7fe5ecaf752880a310ba8768d5d/src/libraries/System.Net.Quic/src/System/Net/Quic/Internal/MsQuicApi.cs#L19
            // Windows Server 2022 can use HTTP/3 and Windows version is 10.0.20348
            // If Linux, requires msquic installed
            errorCode = TranslateRequestErrors.Http3UnavailableInOSVersion;
            return false;
        }
        else if (httpVersion == 2.0m && !(OperatingSystem.IsLinux() || OperatingSystem.IsWindowsVersionAtLeast(10) || OperatingSystem.IsMacOS()))
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http2
            // .NET support for HTTP/2 in Windows 8 is limited, may not always work.
            // In MacOS, HTTP/2 is supported, but only for client-side, which is our case here
            // https://github.com/dotnet/runtime/discussions/75096
            errorCode = TranslateRequestErrors.Http2UnavailableInOSVersion;
            return false;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }

    public static bool IsWebSocketHttpVersionAvailableInOS(decimal httpVersion, out string? errorCode)
    {
        if (httpVersion == 3.0m)
        {
            // .NET 7 supports WebSockets over HTTP/1.1 and HTTP/2
            // https://devblogs.microsoft.com/dotnet/dotnet-7-networking-improvements/#websockets-over-http-2
            errorCode = TranslateRequestErrors.WebSocketHttpVersionUnavailable;
            return false;
        }
        else if (httpVersion == 2.0m && !(OperatingSystem.IsLinux() || OperatingSystem.IsWindowsVersionAtLeast(10) || OperatingSystem.IsMacOS()))
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http2
            // .NET support for HTTP/2 in Windows 8 is limited, may not always work.
            // In MacOS, HTTP/2 is supported, but only for client-side, which is our case here
            // https://github.com/dotnet/runtime/discussions/75096
            errorCode = TranslateRequestErrors.Http2UnavailableInOSVersion;
            return false;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }

    public static readonly FrozenSet<string> MostCommonHeaders = new[]
    {
        "Accept",
        "Accept-Datetime",
        "Accept-Encoding",
        "Accept-Language",
        "Access-Control-Request-Method",
        "Access-Control-Request-Headers",
        "Cache-Control",
        "Connection",
        "Content-Encoding",
        "Content-Language",
        "Cookie",
        "Date",
        "From",
        "Host",
        "If-Match",
        "If-Modified-Since",
        "If-None-Match",
        "If-Range",
        "If-Unmodified-Since",
        "Max-Forwards",
        "Origin",
        "Pragma",
        "Proxy-Authorization",
        "Range",
        "Referer",
        "Prefer",
        "X-Request-ID",
        "X-Correlation-ID",
        "Save-Data",
        "Sec-GPC",
        "User-Agent",
        "Via",
        "DNT"
    }.ToFrozenSet();

    public static readonly string[] ExampleLcids =
        ["pt-BR", "pt-PT", "en-GB", "en-US", "it-IT", "ru-RU", "uk-UA", "es-ES", "es-AR", "es-MX", "ko-KR", "ja-JP"];

    private const string sampleValueEncodingHeader = "gzip, br, zstd";

    private static string GetSampleValueForDateHeader() =>
        DateTime.Now.ToUniversalTime().ToString("r");

    private static string GetSampleValueForLanguageHeader() =>
        Random.Shared.GetItems(ExampleLcids, 1)[0];

    public static string ProvideSampleValueForHeader(string headerName) =>
        headerName switch
        {
            "Accept" => "*/*",

            "Accept-Datetime" or
            "Date" or
            "If-Modified-Since" or
            "If-Unmodified-Since" => GetSampleValueForDateHeader(),

            "Accept-Encoding" or
            "Content-Encoding" => sampleValueEncodingHeader,

            "Accept-Language" or
            "Content-Language" => GetSampleValueForLanguageHeader(),

            "Access-Control-Request-Method" => "GET",
            "Access-Control-Request-Headers" => "origin, x-requested-with",
            "Cache-Control" => "no-cache",
            "Connection" => "keep-alive",
            "Cookie" => "$Version=1; Skin=new;",
            "From" => "user@example.com",
            "Host" => "en.wikipedia.org",

            "If-Match" or
            "If-None-Match" or
            "If-Range" => string.Empty,

            "Max-Forwards" => "10",
            "Origin" => "http://www.pudim.com.br",
            "Pragma" => "no-cache",
            "Proxy-Authorization" => "Basic {{proxy_credentials_here}}",
            "Range" => "bytes=500-999",
            "Referer" => "http://en.wikipedia.org/wiki/Main_Page",
            "Prefer" => "return=representation",

            "X-Request-ID" or
            "X-Correlation-ID" => Guid.NewGuid().ToString(),

            "Save-Data" => "on",
            "Sec-GPC" => "1",
            "User-Agent" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/119.0",
            "Via" => "1.0 fred, 1.1 example.com (Apache/1.1)",
            "DNT" => "1",
            _ => string.Empty
        };
}