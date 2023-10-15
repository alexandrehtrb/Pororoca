using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Desktop.Converters;

internal static class HttpRequestBodyModeMapping
{
    internal static PororocaHttpRequestBodyMode? MapIndexToEnum(int index) =>
        index switch
        {
            0 => null,
            1 => PororocaHttpRequestBodyMode.Raw,
            2 => PororocaHttpRequestBodyMode.File,
            3 => PororocaHttpRequestBodyMode.UrlEncoded,
            4 => PororocaHttpRequestBodyMode.FormData,
            5 => PororocaHttpRequestBodyMode.GraphQl,
            _ => null
        };

    internal static int MapEnumToIndex(PororocaHttpRequestBodyMode? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaHttpRequestBodyMode.Raw => 1,
            PororocaHttpRequestBodyMode.File => 2,
            PororocaHttpRequestBodyMode.UrlEncoded => 3,
            PororocaHttpRequestBodyMode.FormData => 4,
            PororocaHttpRequestBodyMode.GraphQl => 5,
            _ => 0
        };
}

public class HttpRequestBodyModeMatchConverter : EnumMatchConverter<PororocaHttpRequestBodyMode>
{
    protected override PororocaHttpRequestBodyMode? MapIndexToEnum(int index) =>
        HttpRequestBodyModeMapping.MapIndexToEnum(index);
}