using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionViewModel : RequestsAndFoldersParentViewModel, IPororocaVariableResolver
{
    #region COLLECTION ORGANIZATION

    public ReactiveCommand<Unit, Unit> ShowCollectionScopedHeadersCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewEnvironmentCmd { get; }
    public ReactiveCommand<Unit, Unit> ImportEnvironmentsCmd { get; }
    public ReactiveCommand<Unit, Unit> ExportCollectionCmd { get; }

    #endregion

    #region COLLECTION

    private readonly Guid colId;

    private readonly DateTimeOffset colCreatedAt;

    public ObservableCollection<string> HttpRequestsPaths { get; }

    // collection variables
    public List<PororocaVariable> Variables =>
        CollectionVariablesVm.VariablesTableVm.GetVariables(includeSecretVariables: true);

    public PororocaRequestAuth? CollectionScopedAuth =>
        ((CollectionScopedAuthViewModel)Items.First(x => x is CollectionScopedAuthViewModel))
        .AuthVm.ToCustomAuth();

    private CollectionScopedRequestHeadersViewModel CollectionScopedRequestHeadersVm { get; }

    public List<PororocaKeyValueParam>? CollectionScopedRequestHeaders =>
        CollectionScopedRequestHeadersVm.ToCollectionScopedRequestHeaders();

    public List<PororocaEnvironment> Environments =>
        EnvironmentsGroupVm.ToEnvironments().ToList(); // collection environments

    internal CollectionVariablesViewModel CollectionVariablesVm =>
        (CollectionVariablesViewModel)Items.First(x => x is CollectionVariablesViewModel);

    internal EnvironmentsGroupViewModel EnvironmentsGroupVm =>
        (EnvironmentsGroupViewModel)Items.First(x => x is EnvironmentsGroupViewModel);

    public EnvironmentViewModel? CurrentEnvironmentVm =>
        EnvironmentsGroupVm.Items.FirstOrDefault(evm => evm.IsCurrentEnvironment);

    public ExportCollectionViewModel ExportCollectionVm { get; }

    #endregion

    public CollectionViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                               PororocaCollection col) : base(parentVm, col.Name)
    {
        #region COLLECTION ORGANIZATION

        ShowCollectionScopedHeadersCmd = ReactiveCommand.Create(ShowCollectionScopedHeaders);
        AddNewEnvironmentCmd = ReactiveCommand.Create(AddNewEnvironment);
        ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
        ExportCollectionCmd = ReactiveCommand.Create(GoToExportCollection);

        #endregion

        #region COLLECTION

        this.colId = col.Id;
        this.colCreatedAt = col.CreatedAt;

        // IMPORTANT: this needs to be done before instantiating HttpRepeaterViewModels
        HttpRequestsPaths = new(col.ListHttpRequestsPathsInCollection());

        Items.Add(new CollectionVariablesViewModel(this, col));
        Items.Add(new CollectionScopedAuthViewModel(this, col));
        CollectionScopedRequestHeadersVm = new(this, col);
        Items.Add(new EnvironmentsGroupViewModel(this, col.Environments));
        AddInitialFoldersAndRequests(col.Folders, col.Requests);
        ExportCollectionVm = new(this);

        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void RefreshSubItemsAvailableMovements()
    {
        const int numberOfFixedItems = 3; // variables, auth, environments
        for (int x = 0; x < Items.Count; x++)
        {
            var colItemVm = Items[x];
            int indexOfLastSubfolder = GetLastIndexOf<CollectionFolderViewModel>(Items);
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

    private void ShowCollectionScopedHeaders() =>
        MainWindowVm.SwitchVisiblePage(CollectionScopedRequestHeadersVm);

    private void AddNewEnvironment() =>
        EnvironmentsGroupVm.AddNewEnvironment();

    private Task ImportEnvironmentsAsync() =>
        EnvironmentsGroupVm.ImportEnvironmentsAsync();

    public override void PasteToThis()
    {
        base.PasteToThis();
        EnvironmentsGroupVm.PasteToThis();
    }

    #region EXPORT COLLECTION

    private void GoToExportCollection() =>
        MainWindowVm.SwitchVisiblePage(ExportCollectionVm);

    #endregion


    #endregion

    #region COLLECTION

    public PororocaCollection ToCollection(bool forExporting = false)
    {
        bool includeSecretColVars = !forExporting || ExportCollectionVm.IncludeSecretVariables;
        var colVars = CollectionVariablesVm.VariablesTableVm.GetVariables(includeSecretColVars);
        var envs = EnvironmentsGroupVm.Items
            .Where(e => !forExporting || e.ExportEnvironmentVm.IncludeInCollectionExport)
            .Select(e => e.ToEnvironment(forExporting)).ToList();

        var folders = Items.OfType<CollectionFolderViewModel>().Select(x => x.ToCollectionFolder()).ToList();
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

        return new PororocaCollection(this.colId, Name, this.colCreatedAt, colVars, CollectionScopedAuth, CollectionScopedRequestHeaders, envs, folders, reqs);
    }

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
            var httpReqs = folder.Items.OfType<HttpRequestViewModel>();
            var subFolders = folder.Items.OfType<CollectionFolderViewModel>();
            var paths = httpReqs.Select(r => r.GetRequestPathInCollection()).ToList();
            foreach (var subFolder in subFolders)
            {
                paths.AddRange(ListHttpRequestsPathsInFolder(subFolder));
            }
            return paths;
        }

        var httpReqs = Items.OfType<HttpRequestViewModel>();
        var folders = Items.OfType<CollectionFolderViewModel>();
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