using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Desktop.ExportImport.FileExporterImporter;

namespace Pororoca.Desktop.ViewModels;

public sealed class ExportCollectionViewModel : ViewModelBase
{
    public CollectionViewModel Collection { get; }

    [Reactive]
    public bool IncludeSecretVariables { get; set; }

    [Reactive]
    public int ExportFormatSelectedIndex { get; set; }

    public ExportCollectionFormat ExportFormat =>
        ExportCollectionFormatMapping.MapIndexToEnum(ExportFormatSelectedIndex);

    public ObservableCollection<EnvironmentViewModel> EnvironmentsToExport { get; }

    public ReactiveCommand<Unit, Unit> GoBackCmd { get; }

    public ReactiveCommand<Unit, Unit> ExportCollectionCmd { get; }

    public ExportCollectionViewModel(CollectionViewModel col)
    {
        Collection = col;
        EnvironmentsToExport = col.EnvironmentsGroupVm.Items;
        GoBackCmd = ReactiveCommand.Create(GoBack);
        ExportCollectionCmd = ReactiveCommand.CreateFromTask(ExportCollectionAsync);
    }

    private void GoBack()
    {
        var mainWindowVm = ((MainWindowViewModel)MainWindow.Instance!.DataContext!);
        mainWindowVm.SwitchVisiblePage(Collection);
    }

    private Task ExportCollectionAsync() =>
        ShowExportCollectionToFileDialogAsync(Collection.ToCollection(forExporting: true), ExportFormat);
}