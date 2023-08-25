using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;

namespace Pororoca.Desktop.UITesting;

internal static class UITestActions
{
    private static readonly TimeSpan defaultWaitingTimeAfterActions = TimeSpan.FromSeconds(0.25);

    private static Task WaitAfterActionAsync() => Task.Delay(defaultWaitingTimeAfterActions);

    internal static TreeViewItem? GetTreeViewItemViewAtIndex(this TreeView parentView, int index) =>
        (TreeViewItem?) parentView.ItemsView[index];

    internal static async Task ClickOn(this Button control)
    {
        control.Command?.Execute(null);
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

    internal static async Task ClearText(this TextBox txtBox, string txt)
    {
        txtBox.Clear();
        await WaitAfterActionAsync();
    }

    internal static async Task ClearText(this TextEditor txtEditor)
    {
        txtEditor.Document.Remove(0, txtEditor.Document.TextLength);
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
}