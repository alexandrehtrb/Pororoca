using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Desktop.Converters;

internal static class WebSocketMessageRawContentSyntaxMapping
{
    internal static PororocaWebSocketMessageRawContentSyntax? MapIndexToEnum(int index) =>
        index switch
        {
            0 => PororocaWebSocketMessageRawContentSyntax.Json,
            1 => PororocaWebSocketMessageRawContentSyntax.Other,
            _ => PororocaWebSocketMessageRawContentSyntax.Other
        };

    internal static int MapEnumToIndex(PororocaWebSocketMessageRawContentSyntax? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaWebSocketMessageRawContentSyntax.Json => 0,
            PororocaWebSocketMessageRawContentSyntax.Other => 1,
            _ => 0
        };
}
/*
public class WebSocketMessageRawContentSyntaxMatchConverter : EnumMatchConverter<PororocaWebSocketMessageRawContentSyntax>
{
    protected override PororocaWebSocketMessageRawContentSyntax? MapIndexToEnum(int index) =>
        WebSocketMessageRawContentSyntaxMapping.MapIndexToEnum(index);
}
*/