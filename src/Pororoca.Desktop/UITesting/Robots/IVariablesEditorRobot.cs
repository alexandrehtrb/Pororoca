using Avalonia.Controls;
using Pororoca.Desktop.ViewModels.DataGrids;

namespace Pororoca.Desktop.UITesting.Robots;

internal interface IVariablesEditorRobot
{
    Task EditVariableAt(int index, bool enabled, string key, string value, bool isSecret = false);
    Task SetVariables(IEnumerable<VariableViewModel> vars);

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
}