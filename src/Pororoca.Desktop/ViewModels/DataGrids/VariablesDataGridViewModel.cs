using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class VariablesDataGridViewModel : BaseDataGridWithOperationsViewModel<VariableViewModel, PororocaVariable>
{
    private static readonly ColumnList<VariableViewModel> gridColumns = MakeGridColumns();

    public override SimpleClipboardArea<PororocaVariable> InnerClipboardArea =>
        VariablesClipboardArea.Instance;

    public VariablesDataGridViewModel(List<PororocaVariable> initialValues) : base(initialValues)
    {
    }

    protected override VariableViewModel ToVm(PororocaVariable domainObj) =>
        new(Items, domainObj);

    protected override PororocaVariable ToDomain(VariableViewModel viewModel) =>
        viewModel.ToVariable();

    protected override PororocaVariable MakeCopy(PororocaVariable domainObj) =>
        domainObj.Copy();
        
    protected override FlatTreeDataGridSource<VariableViewModel> GenerateDataGridSource()
    {
        FlatTreeDataGridSource<VariableViewModel> source = new(Items);
        source.Columns.AddRange(gridColumns);
        return source;
    }

    private static ColumnList<VariableViewModel> MakeGridColumns()
    {
        var enabledColumn = new TemplateColumn<VariableViewModel>(
            Localizer.Instance.CollectionVariables.VariableEnabled,
            "variableEnabledCell",
            width: new(1, GridUnitType.Star));
        
        var nameColumn = new TextColumn<VariableViewModel, string>(
            Localizer.Instance.CollectionVariables.VariableName,
            x => x.Key, (x, v) => x.Key = v ?? string.Empty,
            width: new(2, GridUnitType.Star));
        
        var valueColumn = new TextColumn<VariableViewModel, string>(
            Localizer.Instance.CollectionVariables.VariableValue,
            x => x.Value, (x, v) => x.Value = v ?? string.Empty,
            width: new(2, GridUnitType.Star));

        var isSecretColumn = new TemplateColumn<VariableViewModel>(
            Localizer.Instance.CollectionVariables.VariableIsSecret,
            "variableIsSecretCell",
            width: new(1, GridUnitType.Star));

        var removeVarColumn = new TemplateColumn<VariableViewModel>(
            string.Empty,
            "variableRemoveCell",
            width: new(0.2, GridUnitType.Star));

        Localizer.Instance.SubscribeToLanguageChange(() =>
        {
            enabledColumn.Header = Localizer.Instance.CollectionVariables.VariableEnabled;
            nameColumn.Header = Localizer.Instance.CollectionVariables.VariableName;
            valueColumn.Header = Localizer.Instance.CollectionVariables.VariableValue;
            isSecretColumn.Header = Localizer.Instance.CollectionVariables.VariableIsSecret;
        });

        return new() { enabledColumn, nameColumn, valueColumn, isSecretColumn, removeVarColumn };
    }
}