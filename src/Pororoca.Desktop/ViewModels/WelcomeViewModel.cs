using ReactiveUI.Primitives;
using Pororoca.Desktop.ExportImport;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class WelcomeViewModel : ViewModelBase
{
    public static readonly WelcomeViewModel Instance = new();

    public ReactiveCommand<RxVoid, RxVoid> AddNewCollectionCmd =>
        MainWindowVm.AddNewCollectionCmd;

    public ReactiveCommand<RxVoid, RxVoid> ImportCollectionCmd =>
        MainWindowVm.ImportCollectionsFromFileCmd;

    public ReactiveCommand<RxVoid, RxVoid> ImportOpenAPICmd { get; }

    public ReactiveCommand<RxVoid, RxVoid> GoToDocsWebSiteCmd =>
        MainWindowVm.OpenDocsInWebBrowserCmd;

    public ReactiveCommand<RxVoid, RxVoid> VisitGitHubRepoCmd =>
        MainWindowVm.OpenGitHubRepoInWebBrowserCmd;

    public ReactiveCommand<RxVoid, RxVoid> OpenDonationsPageCmd =>
        MainWindowVm.OpenDonationsPageInWebBrowserCmd;

    private WelcomeViewModel() =>
        ImportOpenAPICmd = ReactiveCommand.CreateFromTask(ImportOpenAPIAsync);

    private Task ImportOpenAPIAsync() =>
        FileExporterImporter.ImportCollectionsAsync(MainWindowVm);
}