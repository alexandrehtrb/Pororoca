using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class WebsocketResponseDataGridViewModel : KeyValueParamsDataGridViewModel
{
    private static readonly ColumnList<KeyValueParamViewModel> gridColumns = MakeGridColumns();

    public WebsocketResponseDataGridViewModel(List<PororocaKeyValueParam>? initialValues = null) : base(initialValues)
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
            Localizer.Instance.HttpRequest.HeaderName,
            x => x.Key, (x, v) => x.Key = v ?? string.Empty,
            width: new(1, GridUnitType.Star));

        var valueColumn = new TextColumn<KeyValueParamViewModel, string>(
            Localizer.Instance.HttpRequest.HeaderValue,
            x => x.Value, (x, v) => x.Value = v ?? string.Empty,
            width: new(1, GridUnitType.Star));


        Localizer.Instance.SubscribeToLanguageChange(() =>
        {
            nameColumn.Header = Localizer.Instance.HttpRequest.HeaderName;
            valueColumn.Header = Localizer.Instance.HttpRequest.HeaderValue;
        });

        return new() { nameColumn, valueColumn };
    }
}