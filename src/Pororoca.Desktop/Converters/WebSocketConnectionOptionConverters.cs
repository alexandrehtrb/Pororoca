namespace Pororoca.Desktop.Converters;

public enum WebSocketConnectionOption
{
    Headers = 1,
    Subprotocols = 2,
    Compression = 3
}

internal static class WebSocketConnectionOptionMapping
{
    internal static WebSocketConnectionOption MapIndexToEnum(int index) =>
        index switch
        {
            0 => WebSocketConnectionOption.Headers,
            1 => WebSocketConnectionOption.Subprotocols,
            2 => WebSocketConnectionOption.Compression,
            _ => WebSocketConnectionOption.Headers
        };

    internal static int MapEnumToIndex(WebSocketConnectionOption? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            WebSocketConnectionOption.Headers => 0,
            WebSocketConnectionOption.Subprotocols => 1,
            WebSocketConnectionOption.Compression => 2,
            _ => 0
        };
}

public class WebSocketConnectionOptionMatchConverter : EnumMatchConverter<WebSocketConnectionOption>
{
    protected override WebSocketConnectionOption? MapIndexToEnum(int index) =>
        WebSocketConnectionOptionMapping.MapIndexToEnum(index);
}