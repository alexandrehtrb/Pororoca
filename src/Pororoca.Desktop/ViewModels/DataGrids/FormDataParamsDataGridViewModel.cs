using System.Reactive;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class FormDataParamsDataGridViewModel : BaseDataGridWithOperationsViewModel<HttpRequestFormDataParamViewModel, PororocaHttpRequestFormDataParam>
{
    protected override SimpleClipboardArea<PororocaHttpRequestFormDataParam> InnerClipboardArea =>
        FormDataParamsClipboardArea.Instance;

    public ReactiveCommand<Unit, Unit> AddNewFormDataTextParamCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewFormDataFileParamCmd { get; }

    public FormDataParamsDataGridViewModel(List<PororocaHttpRequestFormDataParam>? initialValues = null) : base(initialValues)
    {
        AddNewFormDataTextParamCmd = ReactiveCommand.Create(AddNewFormDataTextParam);
        AddNewFormDataFileParamCmd = ReactiveCommand.CreateFromTask(AddNewFormDataFileParam);
    }

    protected override HttpRequestFormDataParamViewModel ToVm(PororocaHttpRequestFormDataParam domainObj) =>
        new(Items, domainObj);

    protected override PororocaHttpRequestFormDataParam ToDomain(HttpRequestFormDataParamViewModel viewModel) =>
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
}