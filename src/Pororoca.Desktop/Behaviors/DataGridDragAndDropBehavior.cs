using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace Pororoca.Desktop.Behaviors;

public class DataGridDragAndDropBehavior<T> : DropHandlerBase where T : class
{
    private enum DragDirection
    {
        Up,
        Down
    }

    private struct DndData
    {
        public DndData() { }
        public DataGrid? SrcDataGrid = null;
        public DataGrid DestDataGrid = null!;
        public IList<T> SrcList = null!;
        public IList<T> DestList = null!;
        public int SrcIndex = -1;

        public int DestIndex = -1;
        public DragDirection Direction;
    }

    private const string DraggingUpClassName = "dragging-up";
    private const string DraggingDownClassName = "dragging-down";

    private DndData dnd = new();

    private bool Validate(object? sender, DragEventArgs e, object? sourceContext)
    {
        if (this.dnd.SrcDataGrid is not { } srcDg ||
            sender is not DataGrid destDg ||
            sourceContext is not T src ||
            srcDg.Items is not IList<T> srcList ||
            destDg.Items is not IList<T> destList ||
            destDg.GetVisualAt(e.GetPosition(destDg),
              v => v.FindDescendantOfType<DataGridCell>() is not null) is not Control
              {
                  DataContext: T dest
              } visual)
        {
            return false;
        }

        var cell = visual.FindDescendantOfType<DataGridCell>()!;
        var pos = e.GetPosition(cell);

        this.dnd.SrcDataGrid = srcDg;
        this.dnd.DestDataGrid = destDg;
        this.dnd.SrcList = srcList;
        this.dnd.DestList = destList;
        this.dnd.Direction = cell.DesiredSize.Height / 2 > pos.Y ? DragDirection.Up : DragDirection.Down;
        this.dnd.SrcIndex = srcList.IndexOf(src);
        this.dnd.DestIndex = destList.IndexOf(dest);

        return true;
    }

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext,
                                  object? targetContext, object? state) => Validate(sender, e, sourceContext);

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext,
                                 object? targetContext, object? state)
    {
        if (!Validate(sender, e, sourceContext))
            return false;

        if (this.dnd.SrcDataGrid != this.dnd.DestDataGrid && this.dnd.Direction == DragDirection.Down)
            this.dnd.DestIndex++;
        else if (this.dnd.SrcIndex > this.dnd.DestIndex && this.dnd.Direction == DragDirection.Down)
            this.dnd.DestIndex++;
        else if (this.dnd.SrcIndex < this.dnd.DestIndex && this.dnd.Direction == DragDirection.Up)
            this.dnd.DestIndex--;

        MoveItem(this.dnd.SrcList, this.dnd.DestList, this.dnd.SrcIndex, this.dnd.DestIndex);
        this.dnd.DestDataGrid.SelectedIndex = this.dnd.DestIndex;
        this.dnd.DestDataGrid.ScrollIntoView(this.dnd.DestList[this.dnd.DestIndex], null);
        this.dnd.SrcDataGrid = null;
        return true;
    }

    public override void Enter(object? sender, DragEventArgs e, object? sourceContext,
                               object? targetContext)
    {
        this.dnd.SrcDataGrid ??= sender as DataGrid;
        if (!Validate(sender, e, sourceContext))
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        string className = this.dnd.Direction switch
        {
            DragDirection.Down => DraggingDownClassName,
            DragDirection.Up => DraggingUpClassName,
            _ => throw new UnreachableException($"Invalid drag direction: {this.dnd.Direction}")
        };
        this.dnd.DestDataGrid.Classes.Add(className);

        e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
        e.Handled = true;
    }

    public override void Over(object? sender, DragEventArgs e, object? sourceContext,
                              object? targetContext)
    {
        if (!Validate(sender, e, sourceContext))
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.DragEffects |= DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
        e.Handled = true;

        (string toAdd, string toRemove) = this.dnd.Direction switch
        {
            DragDirection.Down => (DraggingDownClassName, DraggingUpClassName),
            DragDirection.Up => (DraggingUpClassName, DraggingDownClassName),
            _ => throw new UnreachableException($"Invalid drag direction: {this.dnd.Direction}")
        };
        if (this.dnd.DestDataGrid.Classes.Contains(toAdd))
            return;

        this.dnd.DestDataGrid.Classes.Remove(toRemove);
        this.dnd.DestDataGrid.Classes.Add(toAdd);
    }

    public override void Leave(object? sender, RoutedEventArgs e)
    {
        base.Leave(sender, e);
        RemoveDraggingClass(sender as DataGrid);
    }

    public override void Drop(object? sender, DragEventArgs e, object? sourceContext,
                              object? targetContext)
    {
        RemoveDraggingClass(sender as DataGrid);
        base.Drop(sender, e, sourceContext, targetContext);
        this.dnd.SrcDataGrid = null;
    }

    private static void RemoveDraggingClass(DataGrid? dg)
    {
        if (dg is not null && !dg.Classes.Remove(DraggingUpClassName))
            dg.Classes.Remove(DraggingDownClassName);
    }
}