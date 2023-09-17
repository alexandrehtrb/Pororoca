using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Controls;

public class EditableTextBlock : UserControl
{
    public EditableTextBlock() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    protected override void OnDataContextBeginUpdate()
    {
        if (DataContext is EditableTextBlockViewModel vm)
        {
            vm.OnIsEditingChanged = null;
        }
        base.OnDataContextBeginUpdate();
    }

    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();
        if (DataContext is EditableTextBlockViewModel vm)
        {
            vm.OnIsEditingChanged = OnIsEditingChanged;
        }
    }

    public void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            var vm = (EditableTextBlockViewModel)DataContext!;
            vm.EditOrApplyTxtChange();
        }
    }

    private void OnIsEditingChanged(bool isEditing)
    {
        if (isEditing)
        {
            var txtBox = this.FindControl<TextBox>("txtBox")!;
            txtBox.Focus();
            txtBox.CaretIndex = txtBox.Text is null ? 0 : txtBox.Text.Length;
        }
    }
}