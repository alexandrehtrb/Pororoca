using Avalonia.Controls;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.UITesting.Robots;

internal interface IVariablesEditorRobot
{
    Task EditVariableAt(int index, bool enabled, string key, string value, bool isSecret = false);
    Task SetVariables(IEnumerable<VariableViewModel> vars);
    Task SelectVariables(params VariableViewModel[] vars);
    Task CutSelectedVariables();
    Task CopySelectedVariables();
    Task PasteVariables();
    Task DeleteSelectedVariables();

    protected static async Task SetVariables(VariablesDataGridViewModel variablesDgVm, IEnumerable<VariableViewModel> vars)
    {
        variablesDgVm.Items.Clear();
        foreach (var v in vars)
        {
            variablesDgVm.Items.Add(v);
        }
        await UITestActions.WaitAfterActionAsync();
    }

    protected static async Task EditVariableAt(DataGrid variablesDg, int index, bool enabled, string key, string value, bool isSecret = false)
    {
        var colVarsVms = variablesDg.ItemsSource.Cast<VariableViewModel>();
        var colVar = colVarsVms.ElementAt(index);
        colVar.Enabled = enabled;
        colVar.Key = key;
        colVar.Value = value;
        colVar.IsSecret = isSecret;
        await UITestActions.WaitAfterActionAsync();
    }

    protected static async Task SelectVariables(DataGrid variablesDg, params VariableViewModel[] vars)
    {
        variablesDg.SelectedItems.Clear();
        foreach (var v in vars) variablesDg.SelectedItems.Add(v);
        await UITestActions.WaitAfterActionAsync();
    }

    protected static async Task CutSelectedVariables(VariablesDataGridViewModel variablesVm)
    {
        variablesVm.CutOrCopySelected(false);
        await UITestActions.WaitAfterActionAsync();
    }

    protected static async Task CopySelectedVariables(VariablesDataGridViewModel variablesVm)
    {
        variablesVm.CutOrCopySelected(true);
        await UITestActions.WaitAfterActionAsync();
    }

    protected static async Task PasteVariables(VariablesDataGridViewModel variablesVm)
    {
        variablesVm.Paste();
        await UITestActions.WaitAfterActionAsync();
    }

    protected static async Task DeleteSelectedVariables(VariablesDataGridViewModel variablesVm)
    {
        variablesVm.DeleteSelected();
        await UITestActions.WaitAfterActionAsync();
    }
}