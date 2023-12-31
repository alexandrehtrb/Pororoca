using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Behaviors;

public abstract class BaseDataGridDropHandler<T> : DropHandlerBase
    where T : ViewModelBase
{
    private const string dataGridRowDraggingStyleClass = "Dragging";

    protected abstract T MakeCopy(ObservableCollection<T> parentCollection, T item);

    protected abstract bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute);

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        ClearDraggingStyleFromAllRows(sender);
        if (e.Source is Control c && sender is DataGrid dg)
        {
            bool valid = Validate(dg, e, sourceContext, targetContext, false);
            if (valid)
            {
                var row = FindDataGridRowFromChildView(c);
                ApplyDraggingStyleToRow(row!);
            }
            return valid;
        }
        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        ClearDraggingStyleFromAllRows(sender);
        if (e.Source is Control && sender is DataGrid dg)
        {
            return Validate(dg, e, sourceContext, targetContext, true);
        }
        return false;
    }

    public override void Cancel(object? sender, RoutedEventArgs e)
    {
        base.Cancel(sender, e);
        // this is necessary to clear adorner borders
        // when mouse exists DataGrid
        // they would remain even after changing screens
        ClearDraggingStyleFromAllRows(sender);
    }

    protected bool RunDropAction(DataGrid dg, DragEventArgs e, bool bExecute, T sourceItem, T targetItem, ObservableCollection<T> items)
    {
        int sourceIndex = items.IndexOf(sourceItem);
        int targetIndex = items.IndexOf(targetItem);

        if (sourceIndex < 0 || targetIndex < 0)
        {
            return false;
        }

        switch (e.DragEffects)
        {
            case DragDropEffects.Copy:
            {
                if (bExecute)
                {
                    var clone = MakeCopy(items, sourceItem);
                    InsertItem(items, clone, targetIndex + 1);
                    dg.SelectedIndex = targetIndex + 1;
                }
                return true;
            }
            case DragDropEffects.Move:
            {
                if (bExecute)
                {
                    MoveItem(items, sourceIndex, targetIndex);
                    dg.SelectedIndex = targetIndex;
                }
                return true;
            }
            case DragDropEffects.Link:
            {
                if (bExecute)
                {
                    SwapItem(items, sourceIndex, targetIndex);
                    dg.SelectedIndex = targetIndex;
                }
                return true;
            }
            default:
                return false;
        }
    }

    private static DataGridRow? FindDataGridRowFromChildView(StyledElement sourceChild)
    {
        int maxDepth = 16;
        DataGridRow? row = null;
        StyledElement? current = sourceChild;
        while (maxDepth --> 0 || row is null)
        {
            if (current is DataGridRow dgr)
                row = dgr;

            current = current?.Parent;
        }
        return row;
    }

    private static DataGridRowsPresenter? GetRowsPresenter(Visual v)
    {
        foreach (var cv in v.GetVisualChildren())
        {
            if (cv is DataGridRowsPresenter dgrp)
                return dgrp;
            else if (GetRowsPresenter(cv) is DataGridRowsPresenter dgrp2)
                return dgrp2;                
        }
        return null;
    }

    private static void ClearDraggingStyleFromAllRows(object? sender)
    {
        if (sender is DataGrid dg)
        {
            var presenter = GetRowsPresenter(dg);
            if (presenter is null) return;

            foreach (var r in presenter.Children)
            {
                if (r.Classes.Contains(dataGridRowDraggingStyleClass))
                {
                    r?.Classes?.Remove(dataGridRowDraggingStyleClass);
                }
            }
        }
    }

    private static void ApplyDraggingStyleToRow(DataGridRow row) =>
        row?.Classes?.Add(dataGridRowDraggingStyleClass);
}