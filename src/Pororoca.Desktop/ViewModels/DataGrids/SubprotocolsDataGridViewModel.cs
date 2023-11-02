using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class SubprotocolsDataGridViewModel : KeyValueParamsDataGridViewModel
{
    private static readonly ColumnList<KeyValueParamViewModel> gridColumns = MakeGridColumns();

    public SubprotocolsDataGridViewModel(List<PororocaKeyValueParam>? initialValues = null) : base(initialValues)
    {
    }

    protected override FlatTreeDataGridSource<KeyValueParamViewModel> GenerateDataGridSource()
    {
        FlatTreeDataGridSource<KeyValueParamViewModel> source = new(Items);
        source.Columns.AddRange(gridColumns);
        return source;
    }

    private static ColumnList<KeyValueParamViewModel> MakeGridColumns()
    {
        var enabledColumn = new TemplateColumn<KeyValueParamViewModel>(
            Localizer.Instance.WebSocketConnection.ConnectionRequestSubprotocolEnabled,
            "kvpEnabledCell",
            width: new(0.34, GridUnitType.Star));
        
        var nameColumn = new TextColumn<KeyValueParamViewModel, string>(
            Localizer.Instance.WebSocketConnection.ConnectionRequestSubprotocolName,
            x => x.Key, (x, v) => x.Key = v ?? string.Empty,
            width: new(0.66, GridUnitType.Star));

        var removeVarColumn = new TemplateColumn<KeyValueParamViewModel>(
            string.Empty,
            "kvpRemoveCell",
            width: new(0.2, GridUnitType.Star));

        Localizer.Instance.SubscribeToLanguageChange(() =>
        {
            enabledColumn.Header = Localizer.Instance.WebSocketConnection.ConnectionRequestSubprotocolEnabled;
            nameColumn.Header = Localizer.Instance.WebSocketConnection.ConnectionRequestSubprotocolName;
        });

        return new() { enabledColumn, nameColumn, removeVarColumn };
    }
}