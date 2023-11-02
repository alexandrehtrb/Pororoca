using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class HttpResponseCapturesDataGridViewModel : BaseDataGridWithOperationsViewModel<HttpResponseCaptureViewModel, PororocaHttpResponseValueCapture>
{
    private static readonly ColumnList<HttpResponseCaptureViewModel> gridColumns = MakeGridColumns();

    public override SimpleClipboardArea<PororocaHttpResponseValueCapture> InnerClipboardArea =>
        HttpResponseCapturesClipboardArea.Instance;

    public ReactiveCommand<Unit, Unit> AddNewHeaderCaptureCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewBodyCaptureCmd { get; }

    public HttpResponseCapturesDataGridViewModel(List<PororocaHttpResponseValueCapture>? initialValues) : base(initialValues)
    {
        AddNewHeaderCaptureCmd = ReactiveCommand.Create(AddNewHeaderCapture);
        AddNewBodyCaptureCmd = ReactiveCommand.Create(AddNewBodyCapture);
    }

    protected override HttpResponseCaptureViewModel ToVm(PororocaHttpResponseValueCapture domainObj) =>
        new(Items, domainObj);

    protected override PororocaHttpResponseValueCapture ToDomain(HttpResponseCaptureViewModel viewModel) =>
        viewModel.ToResponseCapture();

    protected override PororocaHttpResponseValueCapture MakeCopy(PororocaHttpResponseValueCapture domainObj) =>
        domainObj.Copy();

    private void AddNewHeaderCapture() =>
        Items.Add(new(Items, new(PororocaHttpResponseValueCaptureType.Header, "var", "header_name", null)));

    private void AddNewBodyCapture() =>
        Items.Add(new(Items, new(PororocaHttpResponseValueCaptureType.Body, "var", null, "$.myProp")));

    protected override FlatTreeDataGridSource<HttpResponseCaptureViewModel> GenerateDataGridSource()
    {
        FlatTreeDataGridSource<HttpResponseCaptureViewModel> source = new(Items);
        source.Columns.AddRange(gridColumns);
        return source;
    }

    private static ColumnList<HttpResponseCaptureViewModel> MakeGridColumns()
    {        
        var targetVarColumn = new TextColumn<HttpResponseCaptureViewModel, string>(
            Localizer.Instance.HttpResponse.CaptureTargetVariable,
            x => x.Type, (x, v) => x.Type = v ?? string.Empty,
            width: new(0.25, GridUnitType.Star));
        
        var typeColumn = new TextColumn<HttpResponseCaptureViewModel, string>(
            Localizer.Instance.HttpResponse.CaptureType,
            x => x.Type, (x, v) => x.Type = v ?? string.Empty,
            width: new(0.15, GridUnitType.Star));
        
        var headerNameOrBodyPathColumn = new TextColumn<HttpResponseCaptureViewModel, string>(
            Localizer.Instance.HttpResponse.CaptureHeaderNameOrBodyPath,
            x => x.HeaderNameOrBodyPath, (x, v) => x.HeaderNameOrBodyPath = v ?? string.Empty,
            width: new(0.30, GridUnitType.Star));

        var capturedValueColumn = new TextColumn<HttpResponseCaptureViewModel, string>(
            Localizer.Instance.HttpResponse.CaptureCapturedValue,
            x => x.CapturedValue, (x, v) => x.CapturedValue = v ?? string.Empty,
            width: new(0.22, GridUnitType.Star));

        var removeCaptureColumn = new TemplateColumn<HttpResponseCaptureViewModel>(
            string.Empty,
            "resCaptureRemoveCell",
            width: new(0.08, GridUnitType.Star));

        Localizer.Instance.SubscribeToLanguageChange(() =>
        {
            targetVarColumn.Header = Localizer.Instance.HttpResponse.CaptureTargetVariable;
            typeColumn.Header = Localizer.Instance.HttpResponse.CaptureType;
            headerNameOrBodyPathColumn.Header = Localizer.Instance.HttpResponse.CaptureHeaderNameOrBodyPath;
            capturedValueColumn.Header = Localizer.Instance.HttpResponse.CaptureCapturedValue;
        });

        return new() { targetVarColumn, typeColumn, headerNameOrBodyPathColumn, capturedValueColumn, removeCaptureColumn };
    }
}