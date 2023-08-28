using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using Pororoca.Desktop.Controls;

namespace Pororoca.Desktop.UITesting;

internal static class UITestActions
{
    private static readonly TimeSpan defaultWaitingTimeAfterActions = TimeSpan.FromSeconds(0.2);

    private static Task WaitAfterActionAsync() => Task.Delay(defaultWaitingTimeAfterActions);

    internal static TreeViewItem? GetTreeViewItemViewAtIndex(this TreeView parentView, int index) =>
        (TreeViewItem?) parentView.ItemsView[index];

    internal static async Task ClickOn(this Button control)
    {
        control.Command?.Execute(null);
        await WaitAfterActionAsync();
    }

    internal static async Task ClickOn(this IconButton control)
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

    internal static async Task ClickOn(this CheckBox cb)
    {
        cb.IsChecked = !cb.IsChecked;
        await WaitAfterActionAsync();
    }

    internal static async Task<TreeViewItem> Select(this TreeView tree, string pathSeparatedBySlashes)
    {
        string[] items = pathSeparatedBySlashes.Split('/');
        TreeViewItem? tempTvi = null;
        foreach (string item in items)
        {
            if (tempTvi is null)
            {
                tempTvi = (TreeViewItem) tree.Items.First(i => i is TreeViewItem tvi && ((string)tvi.Header!) == item)!;
            }
            else
            {
                tempTvi = (TreeViewItem) tempTvi.Items.First(i => i is TreeViewItem tvi && ((string)tvi.Header!) == item)!;
            }

            if (tempTvi is not null)
            {
                tree.SelectedItem = tempTvi;
                await WaitAfterActionAsync();
            }
        }

        return tempTvi!;
    }

    internal static async Task ClearText(this TextBox txtBox)
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
        args.RoutedEvent = InputElement.TextInputEvent;
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