using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.UITesting;

internal static class UITestActions
{
    // increase the time below to make the tests run slower
    private static readonly TimeSpan defaultWaitingTimeAfterActions = TimeSpan.FromSeconds(0.05);

    internal static Task WaitAfterActionAsync() => Task.Delay(defaultWaitingTimeAfterActions);

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

    internal static object? GetChildView(this TreeView tree, string pathSeparatedBySlashes)
    {
        string[] items = pathSeparatedBySlashes.Split('/');
        object? tempTvi = null;
        foreach (string item in items)
        {
            if (items.ToList().IndexOf(item) == 0)
            {
                tempTvi = tree.Items.FirstOrDefault(i => i is CollectionViewModel p && p.Name == item)!;
            }
            else if (tempTvi is null)
            {
                return null;
            }
            else if (tempTvi is CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel> pvm1)
            {
                if (item == "ENVS")
                {
                    tempTvi = pvm1.Items.FirstOrDefault(i => i is EnvironmentsGroupViewModel egvm)!;
                    if (tempTvi is EnvironmentsGroupViewModel egvm)
                    {
                        egvm.IsExpanded = true;
                    }
                }
                else if (item == "VARS")
                {
                    pvm1.IsExpanded = true;
                    tempTvi = pvm1.Items.FirstOrDefault(i => i is CollectionVariablesViewModel)!;
                }
                else
                {
                    tempTvi = pvm1.Items.FirstOrDefault(i => i.Name == item)!;
                    pvm1.IsExpanded = true;
                }
            }
            else if (tempTvi is CollectionOrganizationItemParentViewModel<EnvironmentViewModel> egvm)
            {
                tempTvi = egvm.Items.FirstOrDefault(i => i.Name == item)!;
                egvm.IsExpanded = true;
            }
            else if (tempTvi is CollectionOrganizationItemParentViewModel<WebSocketClientMessageViewModel> wsvm)
            {
                tempTvi = wsvm.Items.FirstOrDefault(i => i.Name == item)!;
                wsvm.IsExpanded = true;
            }
            else
            {
                return null;
            }
        }

        return tempTvi!;
    }

    internal static async Task<CollectionOrganizationItemViewModel> Select(this TreeView tree, string pathSeparatedBySlashes)
    {
        object tvi = tree.GetChildView(pathSeparatedBySlashes)!;
        tree.SelectedItem = tvi;
        await WaitAfterActionAsync();
        return (CollectionOrganizationItemViewModel)tvi;
    }

    internal static async Task<List<CollectionOrganizationItemViewModel>> SelectMultiple(this TreeView tree, params string[] pathsSeparatedBySlashes)
    {
        List<CollectionOrganizationItemViewModel> items = new();
        tree.SelectedItem = null;
        foreach (string pathSeparatedBySlashes in pathsSeparatedBySlashes)
        {
            var item = (CollectionOrganizationItemViewModel)(await tree.Select(pathSeparatedBySlashes)); 
            items.Add(item);
        }        
        tree.SelectedItems.Clear();
        foreach (var item in items)
        {
            tree.SelectedItems.Add(item);
        }
        await WaitAfterActionAsync();
        return items;
    }

    internal static async Task Select(this ComboBox cb, string item)
    {
        cb.IsDropDownOpen = true;
        cb.SelectedIndex = cb.Items.IndexOf(cb.Items.First(x => x is string s && s == item));
        cb.IsDropDownOpen = false;
        await WaitAfterActionAsync();
    }

    internal static async Task Select(this ComboBox cb, ComboBoxItem item)
    {
        cb.IsDropDownOpen = true;
        cb.SelectedIndex = cb.Items.IndexOf(item);
        cb.IsDropDownOpen = false;
        await WaitAfterActionAsync();
    }

    internal static async Task Select(this AutoCompleteBox acb, string? item)
    {
        acb.SelectedItem = item;
        await WaitAfterActionAsync();
    }

    internal static async Task Select(this TabControl tc, TabItem ti)
    {
        tc.SelectedItem = ti;
        await WaitAfterActionAsync();
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

    internal static async Task TypeText(this TextBox control, string txt)
    {
        TextInputEventArgs args = new();
        args.RoutedEvent = InputElement.TextInputEvent;
        args.Text = txt;
        control.RaiseEvent(args);
        await WaitAfterActionAsync();
    }

    internal static async Task TypeText(this TextEditor editor, string txt)
    {
        editor.Document.Insert(editor.Document.TextLength, txt);
        await WaitAfterActionAsync();
    }

    internal static async Task ClearAndTypeText(this TextBox txtBox, string newTxt)
    {
        await txtBox.ClearText();
        await txtBox.TypeText(newTxt);
    }

    internal static async Task ClearAndTypeText(this TextEditor editor, string newTxt)
    {
        await editor.ClearText();
        await editor.TypeText(newTxt);
    }

    internal static async Task PressKey(this Control control, Key key, KeyModifiers keyModifiers = KeyModifiers.None)
    {
        KeyEventArgs args = new() { Key = key, KeyModifiers = keyModifiers };
        args.RoutedEvent = InputElement.KeyDownEvent;
        control.RaiseEvent(args);
        args.RoutedEvent = InputElement.KeyUpEvent;
        control.RaiseEvent(args);
        await WaitAfterActionAsync();
    }
}