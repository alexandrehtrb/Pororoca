using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;

namespace Pororoca.Desktop.UITesting;

internal static class ControlExtensionsForUITesting
{
    private static readonly TimeSpan defaultWaitingTimeAfterActions = TimeSpan.FromSeconds(0.25);

    private static Task WaitAfterActionAsync() => Task.Delay(defaultWaitingTimeAfterActions);

    internal static TreeViewItem? GetTreeViewItemViewAtIndex(this TreeView parentView, int index) =>
        (TreeViewItem?) parentView.ItemsView[index];

    internal static async Task ClickOn(this Button control)
    {
        RoutedEventArgs args = new(Button.ClickEvent);
        control.RaiseEvent(args);
        await WaitAfterActionAsync();
    }

    internal static async Task ClickOn(this MenuItem control)
    {
        // if it is a parent menu item (drawer), then just open
        if (control.Items.Count > 0)
        {
            control.Open();
        }
        else
        {
            RoutedEventArgs args = new(MenuItem.ClickEvent);
            control.RaiseEvent(args);
        }
        await WaitAfterActionAsync();
    }

    internal static async Task ClickOn(this TreeViewItem control)
    {
        RoutedEventArgs args = new(TreeViewItem.TappedEvent);
        control.RaiseEvent(args);
        await WaitAfterActionAsync();
    }

    internal static async Task TypeText(this Control control, string txt)
    {
        TextInputEventArgs args = new();
        args.Text = txt;
        control.RaiseEvent(args);
        await WaitAfterActionAsync();
    }

    internal static async Task PressKey(this Control control, Key key, KeyModifiers keyModifiers = KeyModifiers.None)
    {
        KeyEventArgs args = new() { Key = key, KeyModifiers = keyModifiers };
        control.RaiseEvent(args);
        await WaitAfterActionAsync();
    }

    internal static void AssertIsHidden(this Control control, UITest test) => test.Assert(control.IsMeasureValid == false);

    internal static void AssertIsVisible(this Control control, UITest test) => test.Assert(control.IsMeasureValid == true);

    internal static void AssertHasText(this TextBlock txtBlock, UITest test, string txt) => test.Assert(txtBlock.Text == txt);

    internal static void AssertHasText(this AutoCompleteBox txtBox, UITest test, string txt) => test.Assert(txtBox.Text == txt);

    internal static void AssertHasText(this TextBox txtBox, UITest test, string txt) => test.Assert(txtBox.Text == txt);
    
    internal static void AssertHasText(this TextEditor txtEditor, UITest test, string txt) => test.Assert(txtEditor.Text == txt);

    internal static void AssertHasText(this ComboBoxItem cbItem, UITest test, string txt) => test.Assert(((string)cbItem.Content!) == txt);

    internal static void AssertHasText(this MenuItem menuItem, UITest test, string txt) => test.Assert(((string)menuItem.Header!) == txt);

    internal static void AssertHasIconVisible(this MenuItem menuItem, UITest test) => test.Assert(((Image)menuItem.Icon!).IsVisible == true);

    internal static void AssertHasIconHidden(this MenuItem menuItem, UITest test) => test.Assert(((Image)menuItem.Icon!).IsVisible == false);
    
    internal static void AssertBackgroundColor(this Panel panel, UITest test, string hexColor) => test.Assert(panel.Background is SolidColorBrush scb && scb.Color.ToHexString() == hexColor);

    private static String ToHexString(this Color c) =>
        $"#{c.R.ToString("X2")}{c.G.ToString("X2")}{c.B.ToString("X2")}";
}