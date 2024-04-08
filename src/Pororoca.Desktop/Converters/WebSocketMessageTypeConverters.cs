using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Desktop.Converters;

internal static class WebSocketMessageTypeMapping
{
    internal static PororocaWebSocketMessageType MapIndexToEnum(int index) =>
        index switch
        {
            0 => PororocaWebSocketMessageType.Text,
            1 => PororocaWebSocketMessageType.Binary,
            2 => PororocaWebSocketMessageType.Close,
            _ => PororocaWebSocketMessageType.Text
        };

    internal static int MapEnumToIndex(PororocaWebSocketMessageType? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaWebSocketMessageType.Text => 0,
            PororocaWebSocketMessageType.Binary => 1,
            PororocaWebSocketMessageType.Close => 2,
            _ => 0
        };
}
/*
public sealed class WebSocketMessageTypeMatchConverter : EnumMatchConverter<PororocaWebSocketMessageType>
{
    protected override PororocaWebSocketMessageType? MapIndexToEnum(int index) =>
        WebSocketMessageTypeMapping.MapIndexToEnum(index);
}
*/