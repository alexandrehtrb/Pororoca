using ReactiveUI.Primitives;
using Pororoca.Desktop.Converters;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static Pororoca.Desktop.ExportImport.FileExporterImporter;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class ExportEnvironmentViewModel : ViewModelBase
{
    public EnvironmentViewModel Environment { get; }

    // this property is used only in export collection
    [Reactive]
    public partial bool IncludeInCollectionExport { get; set; }

    [Reactive]
    public partial bool IncludeSecretVariables { get; set; }

    [Reactive]
    public partial int ExportFormatSelectedIndex { get; set; }

    public ExportEnvironmentFormat ExportFormat =>
        ExportEnvironmentFormatMapping.MapIndexToEnum(ExportFormatSelectedIndex);

    public ReactiveCommand<RxVoid, RxVoid> GoBackCmd { get; }

    public ReactiveCommand<RxVoid, RxVoid> ExportEnvironmentCmd { get; }

    public ExportEnvironmentViewModel(EnvironmentViewModel env)
    {
        IncludeInCollectionExport = true;
        IncludeSecretVariables = false; // default selection
        Environment = env;
        GoBackCmd = ReactiveCommand.Create(GoBack);
        ExportEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportEnvironmentAsync);
    }

    private void GoBack() =>
        MainWindowVm.SwitchVisiblePage(Environment);

    private Task ExportEnvironmentAsync() =>
        ShowExportEnvironmentToFileDialogAsync(Environment.ToEnvironment(forExporting: true), ExportFormat);
}