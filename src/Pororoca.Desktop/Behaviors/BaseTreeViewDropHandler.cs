using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace Pororoca.Desktop.Behaviors;

public abstract class BaseTreeViewDropHandler : DropHandlerBase
{
    private const string rowDraggingUpStyleClass = "DraggingUp";
    private const string rowDraggingDownStyleClass = "DraggingDown";
    private const string targetHighlightStyleClass = "TargetHighlight";

    protected abstract (bool Valid, bool WillSourceItemBeMovedToDifferentParent) Validate(TreeView tv, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute);

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control && sender is TreeView tv)
        {
            var (valid, willSourceItemChangeParent) = Validate(tv, e, sourceContext, targetContext, false);
            var targetVisual = tv.GetVisualAt(e.GetPosition(tv));
            if (valid)
            {
                var targetItem = FindTreeViewItemFromChildView(targetVisual);
                // If its a movement within the same tree level,
                // then an adorner layer will be applied.
                // But, if the source item will move to a different level,
                // the level's owner will receive a background highlight.
                // In the case of being moved to a root target item,
                // (with targetItem.Parent not being another TreeViewItem, 
                // e.g., CollectionViewModel), then this root target item
                // will receive this style.
                var itemToApplyStyle = (willSourceItemChangeParent && targetItem?.Parent is TreeViewItem tviParent) ?
                                       tviParent : targetItem;
                string direction = e.Data.Contains("direction") ? (string)e.Data.Get("direction")! : "down";
                ApplyDraggingStyleToItem(itemToApplyStyle!, direction, willSourceItemChangeParent);
                ClearDraggingStyleFromAllItems(sender, exceptThis: itemToApplyStyle);
            }
            return valid;
        }
        ClearDraggingStyleFromAllItems(sender);
        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        ClearDraggingStyleFromAllItems(sender);
        if (e.Source is Control && sender is TreeView tv)
        {
            var (valid, _) = Validate(tv, e, sourceContext, targetContext, true);
            return valid;
        }
        return false;
    }

    public override void Cancel(object? sender, RoutedEventArgs e)
    {
        base.Cancel(sender, e);
        // This is necessary to clear styles
        // when mouse exists TreeView, else,
        // they would remain even after changing screens.
        ClearDraggingStyleFromAllItems(sender);
    }

    private static TreeViewItem? FindTreeViewItemFromChildView(StyledElement? sourceChild)
    {
        if (sourceChild is null)
            return null;

        int maxDepth = 16;
        StyledElement? current = sourceChild;
        while (maxDepth-- > 0)
        {
            if (current is TreeViewItem tvi)
                return tvi;
            else
                current = current?.Parent;
        }
        return null;
    }

    private static void ClearDraggingStyleFromAllItems(object? sender, TreeViewItem? exceptThis = null)
    {
        if (sender is not Visual rootVisual)
            return;

        foreach (var item in rootVisual.GetLogicalChildren().OfType<TreeViewItem>())
        {
            if (item == exceptThis)
                continue;

            if (item.Classes is not null)
            {
                item.Classes.Remove(rowDraggingUpStyleClass);
                item.Classes.Remove(rowDraggingDownStyleClass);
                item.Classes.Remove(targetHighlightStyleClass);
            }
            ClearDraggingStyleFromAllItems(item, exceptThis);
        }
    }

    private static void ApplyDraggingStyleToItem(TreeViewItem? item, string direction, bool willSourceItemBeMovedToDifferentParent)
    {
        if (item is null)
            return;

        // Avalonia's Classes.Add() verifies
        // if a class has already been added
        // (avoiding duplications); no need to
        // verify .Contains() here.
        if (willSourceItemBeMovedToDifferentParent)
        {
            item.Classes.Remove(rowDraggingDownStyleClass);
            item.Classes.Remove(rowDraggingUpStyleClass);
            item.Classes.Add(targetHighlightStyleClass);
        }
        else if (direction == "up")
        {
            item.Classes.Remove(rowDraggingDownStyleClass);
            item.Classes.Remove(targetHighlightStyleClass);
            item.Classes.Add(rowDraggingUpStyleClass);
        }
        else if (direction == "down")
        {
            item.Classes.Remove(rowDraggingUpStyleClass);
            item.Classes.Remove(targetHighlightStyleClass);
            item.Classes.Add(rowDraggingDownStyleClass);
        }
    }
}