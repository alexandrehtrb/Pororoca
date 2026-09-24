using System.Collections.ObjectModel;
using ReactiveUI.Primitives;
using Pororoca.Desktop.Converters;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static Pororoca.Desktop.ExportImport.FileExporterImporter;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class ExportCollectionViewModel : ViewModelBase
{
    public CollectionViewModel Collection { get; }

    [Reactive]
    public partial bool IncludeSecretVariables { get; set; }

    [Reactive]
    public partial int ExportFormatSelectedIndex { get; set; }

    public ExportCollectionFormat ExportFormat =>
        ExportCollectionFormatMapping.MapIndexToEnum(ExportFormatSelectedIndex);

    public ObservableCollection<EnvironmentViewModel> EnvironmentsToExport { get; }

    public ReactiveCommand<RxVoid, RxVoid> GoBackCmd { get; }

    public ReactiveCommand<RxVoid, RxVoid> ExportCollectionCmd { get; }

    public ExportCollectionViewModel(CollectionViewModel col)
    {
        Collection = col;
        EnvironmentsToExport = col.EnvironmentsGroupVm.Items;
        GoBackCmd = ReactiveCommand.Create(GoBack);
        ExportCollectionCmd = ReactiveCommand.CreateFromTask(ExportCollectionAsync);
    }

    private void GoBack() =>
        MainWindowVm.SwitchVisiblePage(Collection);

    private Task ExportCollectionAsync() =>
        ShowExportCollectionToFileDialogAsync(Collection.ToCollection(forExporting: true), ExportFormat);
}