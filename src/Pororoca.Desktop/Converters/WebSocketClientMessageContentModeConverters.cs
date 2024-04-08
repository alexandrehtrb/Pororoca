using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Desktop.Converters;

internal static class WebSocketClientMessageContentModeMapping
{
    internal static PororocaWebSocketClientMessageContentMode MapIndexToEnum(int index) =>
        index switch
        {
            1 => PororocaWebSocketClientMessageContentMode.File,
            0 => PororocaWebSocketClientMessageContentMode.Raw,
            _ => PororocaWebSocketClientMessageContentMode.Raw
        };

    internal static int MapEnumToIndex(PororocaWebSocketClientMessageContentMode? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaWebSocketClientMessageContentMode.File => 1,
            PororocaWebSocketClientMessageContentMode.Raw => 0,
            _ => 0
        };
}

public sealed class WebSocketClientMessageContentModeMatchConverter : EnumMatchConverter<PororocaWebSocketClientMessageContentMode>
{
    protected override PororocaWebSocketClientMessageContentMode? MapIndexToEnum(int index) =>
        WebSocketClientMessageContentModeMapping.MapIndexToEnum(index);
}