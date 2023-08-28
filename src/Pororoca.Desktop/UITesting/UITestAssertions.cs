using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting;

public abstract partial class UITest
{
    internal void AssertIsHidden(Control control) => Assert((control.IsMeasureValid && control.IsEffectivelyVisible) == false);

    internal void AssertIsVisible(Control control) => Assert((control.IsMeasureValid && control.IsEffectivelyVisible) == true);

    internal void AssertHasText(TextBlock txtBlock, string txt) => Assert(txtBlock.Text == txt);

    internal void AssertHasText(AutoCompleteBox txtBox, string txt) => Assert(txtBox.Text == txt);

    internal void AssertHasText(TextBox txtBox, string txt) => Assert(txtBox.Text == txt);
    
    internal void AssertHasText(TextEditor txtEditor, string txt) => Assert(txtEditor.Text == txt);

    internal void AssertHasText(ComboBoxItem cbItem, string txt) => Assert(((string)cbItem.Content!) == txt);

    internal void AssertHasText(MenuItem menuItem, string txt) => Assert(((string)menuItem.Header!) == txt);

    internal void AssertHasText(TreeViewItem tvi, string txt) => Assert(((string)tvi.Header!) == txt);

    internal void AssertHasText(CheckBox cb, string txt) => Assert((string)cb.Content! == txt);
    
    internal void AssertHasText(IconButton ib, string txt) => Assert(ib.Text == txt);

    internal void AssertHasIconVisible(MenuItem menuItem) => Assert(((Image)menuItem.Icon!).IsVisible == true);

    internal void AssertHasIconHidden(MenuItem menuItem) => Assert(((Image)menuItem.Icon!).IsVisible == false);
    
    internal void AssertBackgroundColor(Panel panel, string hexColor) => Assert(panel.Background is SolidColorBrush scb && ToHexString(scb.Color) == hexColor);

    private static string ToHexString(Color c) =>
        $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    internal void AssertIsChecked(CheckBox cb) => Assert(cb.IsChecked == true);

    internal void AssertIsNotChecked(CheckBox cb) => Assert(cb.IsChecked == false);

    internal void AssertTreeItemExists(CollectionsGroupView cgv, string itemPathSeparatedBySlashes)
    {
        var treeView = cgv.FindControl<TreeView>("itemsTree")!;
        var item = treeView.GetChildView(itemPathSeparatedBySlashes);
        Assert(item is not null);
    }

    internal void AssertTreeItemNotExists(CollectionsGroupView cgv, string itemPathSeparatedBySlashes)
    {
        var treeView = cgv.FindControl<TreeView>("itemsTree")!;
        var item = treeView.GetChildView(itemPathSeparatedBySlashes);
        Assert(item is null);
    }
}