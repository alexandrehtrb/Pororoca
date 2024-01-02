using System.Reactive;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Views;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class WelcomeViewModel : ViewModelBase
{
    private static MainWindowViewModel MainWindowVm =>
        (MainWindowViewModel)MainWindow.Instance!.DataContext!;

    public static readonly WelcomeViewModel Instance = new();

    public ReactiveCommand<Unit, Unit> AddNewCollectionCmd =>
        MainWindowVm.AddNewCollectionCmd;

    public ReactiveCommand<Unit, Unit> ImportCollectionCmd =>
        MainWindowVm.ImportCollectionsFromFileCmd;

    public ReactiveCommand<Unit, Unit> ImportOpenAPICmd { get; }

    public ReactiveCommand<Unit, Unit> GoToDocsWebSiteCmd =>
        MainWindowVm.OpenDocsInWebBrowserCmd;

    public ReactiveCommand<Unit, Unit> VisitGitHubRepoCmd =>
        MainWindowVm.OpenGitHubRepoInWebBrowserCmd;

    private WelcomeViewModel() =>
        ImportOpenAPICmd = ReactiveCommand.CreateFromTask(ImportOpenAPIAsync);

    private Task ImportOpenAPIAsync() =>
        FileExporterImporter.ImportCollectionsAsync(MainWindowVm);
}