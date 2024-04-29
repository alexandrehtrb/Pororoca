using Avalonia.Controls;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class EditableTextBlockRobot : BaseRobot
{
    public EditableTextBlockRobot(EditableTextBlock rootView) : base(rootView) { }

    internal EditableTextBlockIcon? Icon => ((EditableTextBlockViewModel)RootView.DataContext!).Icon;
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