using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.UITesting.Robots;

internal interface IVariablesEditorRobot
{
    Task EditVariableAt(int index, bool enabled, string key, string value, bool isSecret = false);
    Task SetVariables(IEnumerable<VariableViewModel> vars);
    Task SelectVariables(params int[] indexes);
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

    protected static async Task EditVariableAt(TreeDataGrid variablesDg, int index, bool enabled, string key, string value, bool isSecret = false)
    {
        var colVarsVms = variablesDg.Source!.Items.Cast<VariableViewModel>();
        var colVar = colVarsVms.ElementAt(index);
        colVar.Enabled = enabled;
        colVar.Key = key;
        colVar.Value = value;
        colVar.IsSecret = isSecret;
        await UITestActions.WaitAfterActionAsync();
    }

    protected static async Task SelectVariables(TreeDataGrid variablesDg, params int[] indexes)
    {
        var selection = (TreeDataGridRowSelectionModel<VariableViewModel>)variablesDg.Source!.Selection!;
        foreach (int i in indexes)
        {
            // needs to be one at a time, weird bug in TreeDataGrid
            selection.Select(new(i));
        }
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