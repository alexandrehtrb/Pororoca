using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class ResponseHeadersAndTrailersDataGridViewModel : KeyValueParamsDataGridViewModel
{
    private static readonly ColumnList<KeyValueParamViewModel> gridColumns = MakeGridColumns();

    public ResponseHeadersAndTrailersDataGridViewModel(List<PororocaKeyValueParam>? initialValues = null) : base(initialValues)
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
        var nameColumn = new TextColumn<KeyValueParamViewModel, string>(
            Localizer.Instance.HttpResponse.HeaderName,
            x => x.Key, (x, v) => x.Key = v ?? string.Empty,
            width: new(1, GridUnitType.Star));
        
        var valueColumn = new TextColumn<KeyValueParamViewModel, string>(
            Localizer.Instance.HttpResponse.HeaderValue,
            x => x.Value, (x, v) => x.Value = v ?? string.Empty,
            width: new(1, GridUnitType.Star));

        Localizer.Instance.SubscribeToLanguageChange(() =>
        {
            nameColumn.Header = Localizer.Instance.HttpResponse.HeaderName;
            valueColumn.Header = Localizer.Instance.HttpResponse.HeaderValue;
        });

        return new() { nameColumn, valueColumn };
    }
}