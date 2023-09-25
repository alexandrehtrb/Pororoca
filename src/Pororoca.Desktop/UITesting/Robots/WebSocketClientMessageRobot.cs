using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class WebSocketClientMessageRobot : BaseNamedRobot
{
    public WebSocketClientMessageRobot(WebSocketClientMessageView rootView) : base(rootView) { }

    internal ComboBoxItem MessageTypeOptionBinary => GetChildView<ComboBoxItem>("cbiMessageTypeBinary")!;
    internal ComboBoxItem MessageTypeOptionClose => GetChildView<ComboBoxItem>("cbiMessageTypeClose")!;
    internal ComboBoxItem MessageTypeOptionText => GetChildView<ComboBoxItem>("cbiMessageTypeText")!;
    internal ComboBoxItem ContentModeOptionFile => GetChildView<ComboBoxItem>("cbiContentModeFile")!;
    internal ComboBoxItem ContentModeOptionRaw => GetChildView<ComboBoxItem>("cbiContentModeRaw")!;
    internal ComboBoxItem ContentRawSyntaxOptionJson => GetChildView<ComboBoxItem>("cbiContentRawSyntaxJson")!;
    internal ComboBoxItem ContentRawSyntaxOptionOther => GetChildView<ComboBoxItem>("cbiContentRawSyntaxOther")!;
    internal ComboBox MessageType => GetChildView<ComboBox>("cbMessageType")!;
    internal ComboBox ContentMode => GetChildView<ComboBox>("cbContentMode")!;
    internal ComboBox ContentRawSyntax => GetChildView<ComboBox>("cbContentRawSyntax")!;
    internal CheckBox DisableCompressionForThisMessage => GetChildView<CheckBox>("chkbDisableCompressionForThisMessage")!;
    internal TextBox ContentFileSrcPath => GetChildView<TextBox>("tbContentFileSrcPath")!;
    internal TextEditor ContentRaw => GetChildView<TextEditor>("teContentRaw")!;

    internal async Task SetRawJsonContent(string content)
    {
        await MessageType.Select(MessageTypeOptionText);
        await ContentMode.Select(ContentModeOptionRaw);
        await ContentRawSyntax.Select(ContentRawSyntaxOptionJson);
        await ContentRaw.ClearAndTypeText(content);
    }

    internal async Task SetFileBinaryContent(string fileSrcPath)
    {
        await MessageType.Select(MessageTypeOptionBinary);
        await ContentMode.Select(ContentModeOptionFile);
        await ContentFileSrcPath.ClearAndTypeText(fileSrcPath);
    }
}