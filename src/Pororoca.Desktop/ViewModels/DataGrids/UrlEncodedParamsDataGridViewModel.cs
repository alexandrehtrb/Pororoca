using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class UrlEncodedParamsDataGridViewModel : KeyValueParamsDataGridViewModel
{
    private static readonly ColumnList<KeyValueParamViewModel> gridColumns = MakeGridColumns();

    public UrlEncodedParamsDataGridViewModel(List<PororocaKeyValueParam>? initialValues = null) : base(initialValues)
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
            Localizer.Instance.HttpRequest.BodyUrlEncodedParamEnabled,
            "kvpEnabledCell",
            width: new(0.19, GridUnitType.Star));
        
        var nameColumn = new TextColumn<KeyValueParamViewModel, string>(
            Localizer.Instance.HttpRequest.BodyUrlEncodedParamName,
            x => x.Key, (x, v) => x.Key = v ?? string.Empty,
            width: new(0.32, GridUnitType.Star));
        
        var valueColumn = new TextColumn<KeyValueParamViewModel, string>(
            Localizer.Instance.HttpRequest.BodyUrlEncodedParamValue,
            x => x.Value, (x, v) => x.Value = v ?? string.Empty,
            width: new(0.37, GridUnitType.Star));

        var removeVarColumn = new TemplateColumn<KeyValueParamViewModel>(
            string.Empty,
            "kvpRemoveCell",
            width: new(0.12, GridUnitType.Star));

        Localizer.Instance.SubscribeToLanguageChange(() =>
        {
            enabledColumn.Header = Localizer.Instance.HttpRequest.BodyUrlEncodedParamEnabled;
            nameColumn.Header = Localizer.Instance.HttpRequest.BodyUrlEncodedParamName;
            valueColumn.Header = Localizer.Instance.HttpRequest.BodyUrlEncodedParamValue;
        });

        return new() { enabledColumn, nameColumn, valueColumn, removeVarColumn };
    }
}