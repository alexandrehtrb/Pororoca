using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Desktop.ExportImport.FileExporterImporter;

namespace Pororoca.Desktop.ViewModels;

public sealed class ExportEnvironmentViewModel : ViewModelBase
{
    public EnvironmentViewModel Environment { get; }

    // this property is used only in export collection
    [Reactive]
    public bool IncludeInCollectionExport { get; set; }

    [Reactive]
    public bool IncludeSecretVariables { get; set; }

    [Reactive]
    public int ExportFormatSelectedIndex { get; set; }

    public ExportEnvironmentFormat ExportFormat =>
        ExportEnvironmentFormatMapping.MapIndexToEnum(ExportFormatSelectedIndex);

    public ReactiveCommand<Unit, Unit> GoBackCmd { get; }

    public ReactiveCommand<Unit, Unit> ExportEnvironmentCmd { get; }

    public ExportEnvironmentViewModel(EnvironmentViewModel env)
    {
        IncludeInCollectionExport = true;
        IncludeSecretVariables = false; // default selection
        Environment = env;
        GoBackCmd = ReactiveCommand.Create(GoBack);
        ExportEnvironmentCmd = ReactiveCommand.CreateFromTask(ExportEnvironmentAsync);
    }

    private void GoBack()
    {
        var mainWindowVm = ((MainWindowViewModel)MainWindow.Instance!.DataContext!);
        mainWindowVm.SwitchVisiblePage(Environment);
    }

    private Task ExportEnvironmentAsync() =>
        ShowExportEnvironmentToFileDialogAsync(Environment.ToEnvironment(forExporting: true), ExportFormat);
}