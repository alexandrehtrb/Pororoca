using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Pororoca.Desktop.Converters;

public enum EditableTextBlockIcon
{
    Folder = 0,
    HttpRequest = 1,
    DisconnectedWebSocket = 2,
    ConnectedWebSocket = 3,
    HttpRepeater = 4,
    HttpMinigun = 5
}

public sealed class EditableTextBlockIconConverter : IValueConverter
{
    private static readonly Lazy<GeometryDrawing> FolderIcon =
        new(() => LoadGeometryDrawing("IconFolder"), true);

    private static readonly Lazy<GeometryDrawing> HttpRequestIcon =
        new(() => LoadGeometryDrawing("IconHttp"), true);

    private static readonly Lazy<GeometryDrawing> DisconnectedWebSocketIcon =
        new(() => LoadGeometryDrawing("IconWebsocket"), true);

    private static readonly Lazy<GeometryDrawing> ConnectedWebSocketIcon =
        new(() => LoadGeometryDrawing("IconWebsocketConnected"), true);

    private static readonly Lazy<GeometryDrawing> HttpRepeaterIcon =
        new(() => LoadGeometryDrawing("IconRifleGun"), true);

    private static readonly Lazy<GeometryDrawing> HttpMinigunIcon =
        new(() => LoadGeometryDrawing("IconMinigun"), true);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EditableTextBlockIcon icon)
        {
            return icon switch
            {
                EditableTextBlockIcon.Folder => FolderIcon.Value,
                EditableTextBlockIcon.HttpRequest => HttpRequestIcon.Value,
                EditableTextBlockIcon.DisconnectedWebSocket => DisconnectedWebSocketIcon.Value,
                EditableTextBlockIcon.ConnectedWebSocket => ConnectedWebSocketIcon.Value,
                EditableTextBlockIcon.HttpRepeater => HttpRepeaterIcon.Value,
                EditableTextBlockIcon.HttpMinigun => HttpMinigunIcon.Value,
                _ => new BindingNotification(new InvalidCastException(), BindingErrorType.Error)
            };
        }
        else
        {
            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new BindingNotification(new NotSupportedException(), BindingErrorType.Error);

    private static GeometryDrawing LoadGeometryDrawing(string resourceKey) =>
        (GeometryDrawing)(Application.Current!.TryGetResource(resourceKey, null, out object? icon) ? icon! : null!);
}