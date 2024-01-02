using Avalonia.Controls;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class ItemsTreeRobot : BaseRobot
{
    public ItemsTreeRobot(CollectionsGroupView rootView) : base(rootView) { }

    private TreeView Tree => GetChildView<TreeView>("itemsTree")!;

    internal Task Select(string pathSeparatedBySlashes) => Tree.Select(pathSeparatedBySlashes);

    internal Task SelectMultiple(params string[] pathsSeparatedBySlashes) => Tree.SelectMultiple(pathsSeparatedBySlashes);

    internal async Task Cut()
    {
        KeyboardShortcuts.Instance.CutSelectedItems();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task Copy()
    {
        KeyboardShortcuts.Instance.CopySelectedItems();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task Paste()
    {
        KeyboardShortcuts.Instance.PasteCopiedItems();
        await UITestActions.WaitAfterActionAsync();
    }

    internal async Task Delete()
    {
        KeyboardShortcuts.Instance.DeleteSelectedItems();
        await UITestActions.WaitAfterActionAsync();
    }
}