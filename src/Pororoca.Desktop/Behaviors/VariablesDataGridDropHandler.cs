using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Behaviors;

public sealed class VariablesDataGridDropHandler : BaseDataGridDropHandler<VariableViewModel>
{
    protected override VariableViewModel MakeCopy(ObservableCollection<VariableViewModel> parentCollection, VariableViewModel item) =>
        new(parentCollection, item.ToVariable());

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not VariableViewModel sourceItem
         || targetContext is not VariablesDataGridViewModel vm
         || dg.GetVisualAt(e.GetPosition(dg)) is not Control targetControl
         || targetControl.DataContext is not VariableViewModel targetItem)
        {
            return false;
        }

        var items = vm.Items;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
    }
}