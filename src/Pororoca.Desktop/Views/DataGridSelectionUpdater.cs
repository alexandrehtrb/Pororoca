using Avalonia.Controls;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.Views;

public static class DataGridSelectionUpdater
{
    public static void UpdateVmSelectedItems<VM, D>(BaseDataGridWithOperationsViewModel<VM, D> tableVm, SelectionChangedEventArgs e)
        where VM : notnull
        where D : notnull, new()
    {
        var selectedVars = tableVm.SelectedItems;
        foreach (object av in e.AddedItems)
        {
            selectedVars.TryAdd((VM)av, true);
        }
        foreach (object rv in e.RemovedItems)
        {
            selectedVars.TryRemove((VM)rv, out _);
        }
    }
}