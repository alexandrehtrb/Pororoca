using System.Reactive;
using Pororoca.Desktop.ExportImport;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class WelcomeViewModel : ViewModelBase
{
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

    public ReactiveCommand<Unit, Unit> OpenDonationsPageCmd =>
        MainWindowVm.OpenDonationsPageInWebBrowserCmd;

    private WelcomeViewModel() =>
        ImportOpenAPICmd = ReactiveCommand.CreateFromTask(ImportOpenAPIAsync);

    private Task ImportOpenAPIAsync() =>
        FileExporterImporter.ImportCollectionsAsync(MainWindowVm);
}