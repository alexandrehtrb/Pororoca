using System.Globalization;

namespace Pororoca.Domain.Features.Common;

public static class HttpVersionFormatter
{
    public static string FormatHttpVersion(decimal version) =>
        version switch
        {
            1.0m => "HTTP/1.0",
            1.1m => "HTTP/1.1",
            2.0m => "HTTP/2",
            3.0m => "HTTP/3",
            _ => string.Format(CultureInfo.InvariantCulture, "HTTP/{0:0.0}", version)
        };
}