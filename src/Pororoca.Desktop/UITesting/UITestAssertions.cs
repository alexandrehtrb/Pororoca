using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;

namespace Pororoca.Desktop.UITesting;

public abstract partial class UITest
{
    internal void AssertIsHidden(Control control) => Assert(control.IsMeasureValid == false);

    internal void AssertIsVisible(Control control) => Assert(control.IsMeasureValid == true);

    internal void AssertHasText(TextBlock txtBlock, string txt) => Assert(txtBlock.Text == txt);

    internal void AssertHasText(AutoCompleteBox txtBox, string txt) => Assert(txtBox.Text == txt);

    internal void AssertHasText(TextBox txtBox, string txt) => Assert(txtBox.Text == txt);
    
    internal void AssertHasText(TextEditor txtEditor, string txt) => Assert(txtEditor.Text == txt);

    internal void AssertHasText(ComboBoxItem cbItem, string txt) => Assert(((string)cbItem.Content!) == txt);

    internal void AssertHasText(MenuItem menuItem, string txt) => Assert(((string)menuItem.Header!) == txt);

    internal void AssertHasIconVisible(MenuItem menuItem) => Assert(((Image)menuItem.Icon!).IsVisible == true);

    internal void AssertHasIconHidden(MenuItem menuItem) => Assert(((Image)menuItem.Icon!).IsVisible == false);
    
    internal void AssertBackgroundColor(Panel panel, string hexColor) => Assert(panel.Background is SolidColorBrush scb && ToHexString(scb.Color) == hexColor);

    private static string ToHexString(Color c) =>
        $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}