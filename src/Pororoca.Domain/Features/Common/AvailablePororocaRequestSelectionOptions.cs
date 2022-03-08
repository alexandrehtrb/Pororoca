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

    public static readonly IImmutableList<decimal> AvailableHttpVersions = ImmutableList.Create(
        1.0m,
        1.1m,
        2.0m,
        3.0m);

    public static bool IsHttpVersionAvailableInOS(decimal httpVersion, out string? errorCode)
    {
        if (httpVersion == 3.0m && !(OperatingSystem.IsLinux() || OperatingSystem.IsWindowsVersionAtLeast(11)))
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3
            // If Linux, requires msquic installed
            errorCode = TranslateRequestErrors.Http3UnavailableInOSVersion;
            return false;
        }
        else if (httpVersion == 2.0m && !(OperatingSystem.IsLinux() || OperatingSystem.IsWindowsVersionAtLeast(10)))
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http2
            // .NET support for HTTP/2 in Windows 8 is limited, may not always work.
            errorCode = TranslateRequestErrors.Http2UnavailableInOSVersion;
            return false;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }
}