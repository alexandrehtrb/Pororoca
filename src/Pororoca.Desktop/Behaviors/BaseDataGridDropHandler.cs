using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Behaviors;

public abstract class BaseDataGridDropHandler<T> : DropHandlerBase
    where T : ViewModelBase
{
    protected abstract T MakeCopy(ObservableCollection<T> parentCollection, T item);

    protected abstract bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute);

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control && sender is DataGrid dg)
        {
            return Validate(dg, e, sourceContext, targetContext, false);
        }
        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control && sender is DataGrid dg)
        {
            return Validate(dg, e, sourceContext, targetContext, true);
        }
        return false;
    }

    protected bool RunDropAction(DragEventArgs e, bool bExecute, T sourceItem, T targetItem, ObservableCollection<T> items)
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
                }
                return true;
            }
            case DragDropEffects.Move:
            {
                if (bExecute)
                {
                    MoveItem(items, sourceIndex, targetIndex);
                }
                return true;
            }
            case DragDropEffects.Link:
            {
                if (bExecute)
                {
                    SwapItem(items, sourceIndex, targetIndex);
                }
                return true;
            }
            default:
                return false;
        }
    }
}