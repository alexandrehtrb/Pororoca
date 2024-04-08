using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Behaviors;

public interface IRequestHeadersDataGridOwner
{
    RequestHeadersDataGridViewModel RequestHeadersTableVm { get; }
}

public sealed class RequestHeadersDataGridDropHandler : BaseDataGridDropHandler<RequestHeaderViewModel>
{
    protected override RequestHeaderViewModel MakeCopy(ObservableCollection<RequestHeaderViewModel> parentCollection, RequestHeaderViewModel item) =>
        new(parentCollection, item.ToKeyValueParam());

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not RequestHeaderViewModel sourceItem
         || targetContext is not IRequestHeadersDataGridOwner vm
         || dg.GetVisualAt(e.GetPosition(dg)) is not Control targetControl
         || targetControl.DataContext is not RequestHeaderViewModel targetItem)
        {
            return false;
        }

        var items = vm.RequestHeadersTableVm.Items;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
    }
}