using System.Net;

namespace Pororoca.Domain.Features.Common;

public static class HttpStatusCodeFormatter
{
    public static string FormatHttpStatusCodeText(HttpStatusCode statusCode) =>
        $"{(int)statusCode} {Enum.GetName(statusCode)}";
}