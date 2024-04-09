using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Behaviors;

public sealed class SubprotocolsDataGridDropHandler : BaseDataGridDropHandler<KeyValueParamViewModel>
{
    protected override KeyValueParamViewModel MakeCopy(ObservableCollection<KeyValueParamViewModel> parentCollection, KeyValueParamViewModel item) =>
        new(parentCollection, item.ToKeyValueParam());

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not KeyValueParamViewModel sourceItem
         || targetContext is not WebSocketConnectionViewModel vm
         || dg.GetVisualAt(e.GetPosition(dg)) is not Control targetControl
         || targetControl.DataContext is not KeyValueParamViewModel targetItem)
        {
            return false;
        }

        var items = vm.SubprotocolsTableVm.Items;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
    }
}