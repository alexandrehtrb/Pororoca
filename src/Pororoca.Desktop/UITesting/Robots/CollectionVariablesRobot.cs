using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class CollectionVariablesRobot : BaseRobot, IVariablesEditorRobot
{
    public CollectionVariablesRobot(CollectionVariablesView rootView) : base(rootView){}

    internal Button AddVariable => GetChildView<Button>("btAddVariable")!;
    public DataGrid Variables => GetChildView<DataGrid>("dgVariables")!;

    public Task SetVariables(IEnumerable<VariableViewModel> vars)
    {
        var vm = (CollectionVariablesViewModel)RootView!.DataContext!;
        return IVariablesEditorRobot.SetVariables(vm.VariablesTableVm, vars);
    }
        
    public Task EditVariableAt(int index, bool enabled, string key, string value, bool isSecret = false) =>
        IVariablesEditorRobot.EditVariableAt(Variables, index, enabled, key, value, isSecret);
}