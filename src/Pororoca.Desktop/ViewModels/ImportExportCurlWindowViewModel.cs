using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.ImportRequest;
using ReactiveUI.SourceGenerators;

namespace Pororoca.Desktop.ViewModels;

public abstract partial class ImportExportCurlWindowViewModel : ViewModelBase
{
    [Reactive]
    public partial string? Title { get; set; }

    [Reactive]
    public partial bool IsErrorMessageVisible { get; set; }

    [Reactive]
    public partial bool IsExporting { get; set; }

    [Reactive]
    public partial string? CurlCommandLine { get; set; }

    internal abstract bool RunOkClicked();
}

internal sealed class ImportCurlWindowViewModel : ImportExportCurlWindowViewModel
{
    private readonly RequestsAndFoldersParentViewModel parentVm;

    internal ImportCurlWindowViewModel(RequestsAndFoldersParentViewModel parentVm)
    {
        this.parentVm = parentVm;
        Title = Localizer.Instance.ImportExportCurlCommandLineDialog.ImportTitle;
        IsExporting = false;
        IsErrorMessageVisible = false;
    }

    internal override bool RunOkClicked()
    {
        var req = CurlRequestImporter.ImportCurlRequest(CurlCommandLine);
        if (req is not null)
        {
            this.parentVm.AddHttpRequest(req, isNewItem: true);
            return true;
        }
        else
        {
            IsErrorMessageVisible = true;
            return false;
        }
    }
}

internal sealed class ExportCurlWindowViewModel : ImportExportCurlWindowViewModel
{
    internal ExportCurlWindowViewModel(string exportedCurlReq)
    {
        CurlCommandLine = exportedCurlReq;
        Title = Localizer.Instance.ImportExportCurlCommandLineDialog.ExportTitle;
        IsExporting = true;
        IsErrorMessageVisible = false;
    }

    internal override bool RunOkClicked() => true;
}