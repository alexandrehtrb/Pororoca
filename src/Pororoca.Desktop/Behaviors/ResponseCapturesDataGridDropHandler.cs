using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Behaviors;

public sealed class ResponseCapturesDataGridDropHandler : BaseDataGridDropHandler<HttpResponseCaptureViewModel>
{
    protected override HttpResponseCaptureViewModel MakeCopy(ObservableCollection<HttpResponseCaptureViewModel> parentCollection, HttpResponseCaptureViewModel item) =>
        new(parentCollection, item.ToResponseCapture());

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not HttpResponseCaptureViewModel sourceItem
         || targetContext is not HttpRequestViewModel vm
         || dg.GetVisualAt(e.GetPosition(dg)) is not Control targetControl
         || targetControl.DataContext is not HttpResponseCaptureViewModel targetItem)
        {
            return false;
        }

        var items = vm.ResCapturesTableVm.Items;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
    }
}