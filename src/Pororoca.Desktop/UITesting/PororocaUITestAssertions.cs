using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting;

public static class PororocaUITestAssertions
{
    public static void AssertHasText(this TextEditor txtEditor, string txt) => AssertCondition(txtEditor.Document.Text == txt);

    public static void AssertContainsText(this TextEditor txtEditor, string txt) => AssertCondition(txtEditor.Document.Text.Contains(txt));

    public static void AssertTreeItemExists(this CollectionsGroupView cgv, string itemPathSeparatedBySlashes)
    {
        var treeView = cgv.FindControl<TreeView>("itemsTree")!;
        object? item = treeView.GetChildView(itemPathSeparatedBySlashes);
        AssertCondition(item is not null);
    }

    public static void AssertTreeItemNotExists(this CollectionsGroupView cgv, string itemPathSeparatedBySlashes)
    {
        var treeView = cgv.FindControl<TreeView>("itemsTree")!;
        object? item = treeView.GetChildView(itemPathSeparatedBySlashes);
        AssertCondition(item is null);
    }
}