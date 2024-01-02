using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Behaviors;

public sealed class FormDataParamsDataGridDropHandler : BaseDataGridDropHandler<FormDataParamViewModel>
{
    protected override FormDataParamViewModel MakeCopy(ObservableCollection<FormDataParamViewModel> parentCollection, FormDataParamViewModel item) =>
        new(parentCollection, item.ToFormDataParam());

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not FormDataParamViewModel sourceItem
         || targetContext is not HttpRequestViewModel vm
         || dg.GetVisualAt(e.GetPosition(dg)) is not Control targetControl
         || targetControl.DataContext is not FormDataParamViewModel targetItem)
        {
            return false;
        }

        var items = vm.FormDataParamsTableVm.Items;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
    }
}