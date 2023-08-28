using Avalonia.Controls;
using Pororoca.Desktop.Controls;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class EditableTextBlockRobot : BaseRobot
{
    public EditableTextBlockRobot(EditableTextBlock rootView) : base(rootView) { }
    
    internal Image IconHttp => GetChildView<Image>("imgIconHttp")!;
    internal Image IconDisconnectedWebSocket => GetChildView<Image>("imgIconDisconnectedWebSocket")!;
    internal Image IconConnectedWebSocket => GetChildView<Image>("imgIconConnectedWebSocket")!;
    internal TextBlock AppliedText => GetChildView<TextBlock>("tbText")!;
    internal TextBox TextBeingEdited => GetChildView<TextBox>("txtBox")!;
    internal Button ButtonEditOrApply => GetChildView<Button>("btEditOrApplyTxt")!;
    internal Image ButtonIconEdit => GetChildView<Image>("imgEdit")!;
    internal Image ButtonIconApply => GetChildView<Image>("imgApplyTxt")!;

    public async Task Edit(string newText)
    {
        await ButtonEditOrApply.ClickOn();
        await TextBeingEdited.ClearText();
        await TextBeingEdited.TypeText(newText);
        await ButtonEditOrApply.ClickOn();
    }
}