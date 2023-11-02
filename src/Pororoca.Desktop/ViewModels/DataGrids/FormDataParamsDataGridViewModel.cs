using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class FormDataParamsDataGridViewModel : BaseDataGridWithOperationsViewModel<FormDataParamViewModel, PororocaHttpRequestFormDataParam>
{
    private static readonly ColumnList<FormDataParamViewModel> gridColumns = MakeGridColumns();

    public override SimpleClipboardArea<PororocaHttpRequestFormDataParam> InnerClipboardArea =>
        FormDataParamsClipboardArea.Instance;

    public ReactiveCommand<Unit, Unit> AddNewFormDataTextParamCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataFileParamCmd { get; }

    public FormDataParamsDataGridViewModel(List<PororocaHttpRequestFormDataParam>? initialValues = null) : base(initialValues)
    {
        AddNewFormDataTextParamCmd = ReactiveCommand.Create(AddNewFormDataTextParam);
        AddNewFormDataFileParamCmd = ReactiveCommand.CreateFromTask(AddNewFormDataFileParam);
    }

    protected override FormDataParamViewModel ToVm(PororocaHttpRequestFormDataParam domainObj) =>
        new(Items, domainObj);

    protected override PororocaHttpRequestFormDataParam ToDomain(FormDataParamViewModel viewModel) =>
        viewModel.ToFormDataParam();

    protected override PororocaHttpRequestFormDataParam MakeCopy(PororocaHttpRequestFormDataParam domainObj) =>
        domainObj.Copy();

    private void AddNewFormDataTextParam()
    {
        var p = PororocaHttpRequestFormDataParam.MakeTextParam(true, string.Empty, string.Empty, MimeTypesDetector.DefaultMimeTypeForText);
        Items.Add(new(Items, p));
    }

    private async Task AddNewFormDataFileParam()
    {
        string? fileSrcPath = await FileExporterImporter.SelectFileFromStorageAsync();
        if (fileSrcPath != null)
        {
            MimeTypesDetector.TryFindMimeTypeForFile(fileSrcPath, out string? mimeType);
            mimeType ??= MimeTypesDetector.DefaultMimeTypeForBinary;

            var p = PororocaHttpRequestFormDataParam.MakeFileParam(true, string.Empty, fileSrcPath, mimeType);
            Items.Add(new(Items, p));
        }
    }

    protected override FlatTreeDataGridSource<FormDataParamViewModel> GenerateDataGridSource()
    {
        FlatTreeDataGridSource<FormDataParamViewModel> source = new(Items);
        source.Columns.AddRange(gridColumns);
        return source;
    }

    private static ColumnList<FormDataParamViewModel> MakeGridColumns()
    {
        var enabledColumn = new TemplateColumn<FormDataParamViewModel>(
            Localizer.Instance.HttpRequest.BodyFormDataParamEnabled,
            "fdpEnabledCell",
            width: new(0.11, GridUnitType.Star));
        
        var typeColumn = new TextColumn<FormDataParamViewModel, string>(
            Localizer.Instance.HttpRequest.BodyFormDataParamType,
            x => x.Type, (x, v) => x.Type = v ?? string.Empty,
            width: new(0.13, GridUnitType.Star));
        
        var nameColumn = new TextColumn<FormDataParamViewModel, string>(
            Localizer.Instance.HttpRequest.BodyFormDataParamName,
            x => x.Key, (x, v) => x.Key = v ?? string.Empty,
            width: new(0.20, GridUnitType.Star));
        
        var valueColumn = new TextColumn<FormDataParamViewModel, string>(
            Localizer.Instance.HttpRequest.BodyFormDataParamTextOrSrcPath,
            x => x.Value, (x, v) => x.Value = v ?? string.Empty,
            width: new(0.21, GridUnitType.Star));

        var contentTypeColumn = new TextColumn<FormDataParamViewModel, string>(
            Localizer.Instance.HttpRequest.BodyFormDataParamContentType,
            x => x.ContentType, (x, v) => x.ContentType = v ?? string.Empty,
            width: new(0.23, GridUnitType.Star));

        var removeVarColumn = new TemplateColumn<FormDataParamViewModel>(
            string.Empty,
            "fdpRemoveCell",
            width: new(0.12, GridUnitType.Star));

        Localizer.Instance.SubscribeToLanguageChange(() =>
        {
            enabledColumn.Header = Localizer.Instance.HttpRequest.BodyFormDataParamEnabled;
            typeColumn.Header = Localizer.Instance.HttpRequest.BodyFormDataParamType;
            nameColumn.Header = Localizer.Instance.HttpRequest.BodyFormDataParamName;
            valueColumn.Header = Localizer.Instance.HttpRequest.BodyFormDataParamTextOrSrcPath;
            contentTypeColumn.Header = Localizer.Instance.HttpRequest.BodyFormDataParamContentType;
        });

        return new() { enabledColumn, typeColumn, nameColumn, valueColumn, contentTypeColumn, removeVarColumn };
    }
}