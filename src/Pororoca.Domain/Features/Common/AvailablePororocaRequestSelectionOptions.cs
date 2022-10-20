using System.Collections.Immutable;
using Pororoca.Domain.Features.TranslateRequest;

namespace Pororoca.Domain.Features.Common;

public static class AvailablePororocaRequestSelectionOptions
{
    public const string PororocaCollectionExtension = "pororoca_collection.json";
    public const string PostmanCollectionExtension = "postman_collection.json";
    public const string PororocaEnvironmentExtension = "pororoca_environment.json";
    public const string PostmanEnvironmentExtension = "postman_environment.json";

    public static readonly IImmutableList<HttpMethod> AvailableHttpMethods = ImmutableList.Create(
        HttpMethod.Get,
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Patch,
        HttpMethod.Delete,
        HttpMethod.Head,
        HttpMethod.Options,
        HttpMethod.Trace);

    public static readonly IImmutableList<decimal> AvailableHttpVersionsForHttp = ImmutableList.Create(
        1.0m,
        1.1m,
        2.0m,
        3.0m);
    
    public static readonly IImmutableList<decimal> AvailableHttpVersionsForWebSockets = ImmutableList.Create(
        1.1m);

    public static bool IsHttpVersionAvailableInOS(decimal httpVersion, out string? errorCode)
    {
        if (httpVersion == 3.0m && !(OperatingSystem.IsLinux() || OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)))
        {
            // https://docs.microsoft.com/en-us/windows/win32/sysinfo/operating-system-version
            // https://en.wikipedia.org/wiki/Windows_11_version_history
            // https://devblogs.microsoft.com/dotnet/http-3-support-in-dotnet-6/#prerequisites
            // Windows 11 Build 22000 still uses major version as 10, but build number is 22000 or higher
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
        if (httpVersion != 1.1m)
        {
            // Currently, in .NET 6, only WebSockets over HTTP/1.1 are available
            // .NET 7 will support WebSocket over HTTP/2
            errorCode = TranslateRequestErrors.WebSocketHttpVersionUnavailable;
            return false;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }
}