using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.ExportImport;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionViewModel : RequestsAndFoldersParentViewModel, IPororocaVariableResolver
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> AddNewEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> ImportEnvironmentsCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPororocaCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportAsPostmanCollectionCmd { get; }

    #endregion

    #region COLLECTION

    private readonly Guid colId;

    private readonly DateTimeOffset colCreatedAt;

    [Reactive]
    public bool IncludeSecretVariables { get; set; }

    public ObservableCollection<string> HttpRequestsPaths { get; }

    public List<PororocaVariable> Variables =>
        ((CollectionVariablesViewModel)Items.First(x => x is CollectionVariablesViewModel))
        .ToVariables().ToList(); // collection variables

    public PororocaRequestAuth? CollectionScopedAuth =>
        ((CollectionScopedAuthViewModel)Items.First(x => x is CollectionScopedAuthViewModel))
        .AuthVm.ToCustomAuth();

    public List<PororocaEnvironment> Environments =>
        ((EnvironmentsGroupViewModel)Items.First(x => x is EnvironmentsGroupViewModel))
        .ToEnvironments().ToList(); // collection environments

    #endregion

    #region OTHERS

    [Reactive]
    public bool IsOperatingSystemMacOsx { get; set; }

    #endregion

    public CollectionViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                               PororocaCollection col,
                               Func<bool>? isOperatingSystemMacOsx = null) : base(parentVm, col.Name)
    {
        #region OTHERS
        IsOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
        #endregion

        #region COLLECTION ORGANIZATION

        AddNewEnvironmentCmd = ReactiveCommand.Create(AddNewEnvironment);
        ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
        ExportCollectionCmd = ReactiveCommand.CreateFromTask(ExportCollectionAsync);
        ExportAsPororocaCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPororocaCollectionAsync);
        ExportAsPostmanCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPostmanCollectionAsync);

        #endregion

        #region COLLECTION

        this.colId = col.Id;
        this.colCreatedAt = col.CreatedAt;

        // IMPORTANT: this needs to be done before instantiating HttpRepeaterViewModels
        HttpRequestsPaths = new(col.ListHttpRequestsPathsInCollection());

        Items.Add(new CollectionVariablesViewModel(this, col));
        Items.Add(new CollectionScopedAuthViewModel(this, col));
        Items.Add(new EnvironmentsGroupViewModel(this, col.Environments));
        AddInitialFoldersAndRequests(col.Folders, col.Requests);

        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void RefreshSubItemsAvailableMovements()
    {
        const int numberOfFixedItems = 3; // variables, auth, environments
        for (int x = 0; x < Items.Count; x++)
        {
            var colItemVm = Items[x];
            int indexOfLastSubfolder = Items.GetLastIndexOf<CollectionFolderViewModel>();
            if (colItemVm is CollectionVariablesViewModel || colItemVm is CollectionScopedAuthViewModel || colItemVm is EnvironmentsGroupViewModel)
            {
                // Variables and Environments must remain at their positions
                colItemVm.CanMoveUp = colItemVm.CanMoveDown = false;
            }
            else if (colItemVm is CollectionFolderViewModel)
            {
                colItemVm.CanMoveUp = x > numberOfFixedItems;
                colItemVm.CanMoveDown = x < indexOfLastSubfolder;
            }
            else // http requests, websockets and repetitions
            {
                colItemVm.CanMoveUp = x > (indexOfLastSubfolder == -1 ? numberOfFixedItems : (indexOfLastSubfolder + 1));
                colItemVm.CanMoveDown = x < Items.Count - 1;
            }
        }
    }

    private void AddNewEnvironment()
    {
        var environmentsGroup = (EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel);
        environmentsGroup.AddNewEnvironment();
    }

    private Task ImportEnvironmentsAsync()
    {
        var environmentsGroup = (EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel);
        return environmentsGroup.ImportEnvironmentsAsync();
    }

    protected override void CopyThis() =>
        throw new NotImplementedException();

    public override void PasteToThis()
    {
        base.PasteToThis();
        var envGpVm = (EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel);
        envGpVm.PasteToThis();
    }

    #region EXPORT COLLECTION

    private Task ExportCollectionAsync() =>
        FileExporterImporter.ExportCollectionAsync(this);

    private Task ExportAsPororocaCollectionAsync() =>
        FileExporterImporter.ExportAsPororocaCollectionAsync(this);

    private Task ExportAsPostmanCollectionAsync() =>
        FileExporterImporter.ExportAsPostmanCollectionAsync(this);

    #endregion


    #endregion

    #region COLLECTION

    public PororocaCollection ToCollection()
    {
        var folders = Items.Where(i => i is CollectionFolderViewModel).Cast<CollectionFolderViewModel>().Select(x => x.ToCollectionFolder()).ToList();
        var reqs = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel || i is HttpRepeaterViewModel)
                        .Select(i =>
                        {
                            if (i is HttpRequestViewModel httpReqVm)
                                return (PororocaRequest)httpReqVm.ToHttpRequest();
                            if (i is WebSocketConnectionViewModel wsConnVm)
                                return (PororocaRequest)wsConnVm.ToWebSocketConnection();
                            if (i is HttpRepeaterViewModel repVm)
                                return (PororocaRequest)repVm.ToHttpRepetition();
                            else
                                throw new InvalidDataException();
                        }).ToList();

        return new PororocaCollection(this.colId, Name, this.colCreatedAt, Variables, CollectionScopedAuth, Environments, folders, reqs);
    }

    public EnvironmentViewModel? GetCurrentEnvironment() =>
        ((EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel))
        .Items.FirstOrDefault(evm => evm.IsCurrentEnvironment);

    #region HTTP REQUESTS PATHS

    internal void UpdateListOfHttpRequestsPaths()
    {
        var updatedList = ListHttpRequestsPathsInCollection();

        if (!Enumerable.SequenceEqual(updatedList, HttpRequestsPaths))
        {
            HttpRequestsPaths.Clear();
            foreach (string path in ListHttpRequestsPathsInCollection())
            {
                HttpRequestsPaths.Add(path);
            }
        }
    }

    internal void AddHttpRequestPathToList(string path)
    {
        if (!HttpRequestsPaths.Contains(path))
        {
            HttpRequestsPaths.Add(path);
        }
    }

    internal void RemoveHttpRequestPathFromList(string path) =>
        HttpRequestsPaths.Remove(path);

    private List<string> ListHttpRequestsPathsInCollection()
    {
        static IEnumerable<string> ListHttpRequestsPathsInFolder(CollectionFolderViewModel folder)
        {
            var httpReqs = folder.Items.Where(i => i is HttpRequestViewModel).Cast<HttpRequestViewModel>();
            var subFolders = folder.Items.Where(i => i is CollectionFolderViewModel).Cast<CollectionFolderViewModel>();
            var paths = httpReqs.Select(r => r.GetRequestPathInCollection()).ToList();
            foreach (var subFolder in subFolders)
            {
                paths.AddRange(ListHttpRequestsPathsInFolder(subFolder));
            }
            return paths;
        }

        var httpReqs = Items.Where(i => i is HttpRequestViewModel).Cast<HttpRequestViewModel>();
        var folders = Items.Where(i => i is CollectionFolderViewModel).Cast<CollectionFolderViewModel>();
        var paths = httpReqs.Select(r => r.GetRequestPathInCollection()).ToList();
        foreach (var folder in folders)
        {
            paths.AddRange(ListHttpRequestsPathsInFolder(folder));
        }
        return paths;
    }

    #endregion

    #endregion
}