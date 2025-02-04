using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.UITesting;

internal static class PororocaUITestActions
{
    internal static TreeViewItem? GetTreeViewItemViewAtIndex(this TreeView parentView, int index) =>
        (TreeViewItem?)parentView.ItemsView[index];

    internal static async Task RaiseClickEvent(this Button button)
    {
        button.RaiseEvent(new() { RoutedEvent = Button.ClickEvent });
        await WaitAfterActionAsync();
    }

    internal static async Task ClickOn(this IconButton control)
    {
        control.Command?.Execute(null);
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
                else if (item == "AUTH")
                {
                    pvm1.IsExpanded = true;
                    tempTvi = pvm1.Items.FirstOrDefault(i => i is CollectionScopedAuthViewModel)!;
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
        cb.SelectedIndex = cb.Items.IndexOf(
            cb.Items.First(x =>
                (x is string s && s == item)
             || (x is CollectionOrganizationItemViewModel vm && vm.Name == item)
            ));
        cb.IsDropDownOpen = false;
        await WaitAfterActionAsync();
    }

    internal static async Task ClearText(this TextEditor txtEditor)
    {
        txtEditor.Document.Remove(0, txtEditor.Document.TextLength);
        await WaitAfterActionAsync();
    }

    internal static async Task TypeText(this TextEditor editor, string txt)
    {
        foreach (char c in txt)
        {
            editor.Document.Insert(editor.Document.TextLength, c.ToString());
        }
        await WaitAfterActionAsync();
    }

    internal static async Task ClearAndTypeText(this TextEditor editor, string newTxt)
    {
        await editor.ClearText();
        await editor.TypeText(newTxt);
    }
}