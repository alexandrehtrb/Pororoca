using Avalonia.Controls;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop.Views;

public abstract class BaseVariablesListView : UserControl
{
    public void OnSelectedVariablesChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedVars = ((BaseVariablesListViewModel)DataContext!).SelectedVariables;
        foreach (object av in e.AddedItems)
        {
            selectedVars.TryAdd(((VariableViewModel)av), true);
        }
        foreach (object rv in e.RemovedItems)
        {
            selectedVars.TryRemove(((VariableViewModel)rv), out _);
        }
    }
}