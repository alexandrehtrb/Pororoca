using Avalonia.Controls;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class EnvironmentRobot : BaseNamedRobot, IVariablesEditorRobot
{
    public EnvironmentRobot(EnvironmentView rootView) : base(rootView) { }

    internal Button SetAsCurrentEnvironment => GetChildView<Button>("btSetAsCurrentEnvironment")!;
    internal Button AddVariable => GetChildView<Button>("btAddVariable")!;
    public DataGrid Variables => GetChildView<VariablesTableView>("vtvVariables")!.FindControl<DataGrid>("datagrid")!;

    internal VariablesDataGridViewModel VariablesVm => ((EnvironmentViewModel)RootView!.DataContext!).VariablesTableVm;

    public Task SetVariables(IEnumerable<VariableViewModel> vars)
    {
        var vm = (EnvironmentViewModel)RootView!.DataContext!;
        return IVariablesEditorRobot.SetVariables(vm.VariablesTableVm, vars);
    }

    public Task EditVariableAt(int index, bool enabled, string key, string value, bool isSecret = false) =>
        IVariablesEditorRobot.EditVariableAt(Variables, index, enabled, key, value, isSecret);

    public Task SelectVariables(params VariableViewModel[] vars) =>
        IVariablesEditorRobot.SelectVariables(Variables, vars);

    public Task CutSelectedVariables() => IVariablesEditorRobot.CutSelectedVariables(VariablesVm);

    public Task CopySelectedVariables() => IVariablesEditorRobot.CopySelectedVariables(VariablesVm);

    public Task PasteVariables() => IVariablesEditorRobot.PasteVariables(VariablesVm);

    public Task DeleteSelectedVariables() => IVariablesEditorRobot.DeleteSelectedVariables(VariablesVm);
}