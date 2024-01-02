using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Pororoca.Domain.Features.VariableResolution.IPororocaVariableResolver;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionViewModel : CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel>, IPororocaVariableResolver
{
    #region COLLECTION ORGANIZATION

    public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
    public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
    public ReactiveCommand<Unit, Unit> AddNewFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewHttpRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewWebSocketConnectionCmd { get; }
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

    public override ObservableCollection<CollectionOrganizationItemViewModel> Items { get; }

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

        AddNewFolderCmd = ReactiveCommand.Create(AddNewFolder);
        AddNewHttpRequestCmd = ReactiveCommand.Create(AddNewHttpRequest);
        AddNewWebSocketConnectionCmd = ReactiveCommand.Create(AddNewWebSocketConnection);
        AddNewEnvironmentCmd = ReactiveCommand.Create(AddNewEnvironment);
        ImportEnvironmentsCmd = ReactiveCommand.CreateFromTask(ImportEnvironmentsAsync);
        ExportCollectionCmd = ReactiveCommand.CreateFromTask(ExportCollectionAsync);
        ExportAsPororocaCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPororocaCollectionAsync);
        ExportAsPostmanCollectionCmd = ReactiveCommand.CreateFromTask(ExportAsPostmanCollectionAsync);

        #endregion

        #region COLLECTION

        this.colId = col.Id;
        this.colCreatedAt = col.CreatedAt;
        Items = new()
        {
            new CollectionVariablesViewModel(this, col),
            new CollectionScopedAuthViewModel(this, col),
            new EnvironmentsGroupViewModel(this, col.Environments)
        };
        foreach (var folder in col.Folders)
            Items.Add(new CollectionFolderViewModel(this, this, folder));
        foreach (var req in col.Requests)
        {
            if (req is PororocaHttpRequest httpReq)
                Items.Add(new HttpRequestViewModel(this, this, httpReq));
            else if (req is PororocaWebSocketConnection ws)
                Items.Add(new WebSocketConnectionViewModel(this, this, ws));
        }

        RefreshSubItemsAvailableMovements();

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
            else // http requests and websockets
            {
                colItemVm.CanMoveUp = x > (indexOfLastSubfolder == -1 ? numberOfFixedItems : (indexOfLastSubfolder + 1));
                colItemVm.CanMoveDown = x < Items.Count - 1;
            }
        }
    }

    private void AddNewFolder()
    {
        PororocaCollectionFolder newFolder = new(Localizer.Instance.Folder.NewFolder);
        AddFolder(newFolder, showItemInScreen: true);
    }

    private void AddNewHttpRequest()
    {
        PororocaHttpRequest newReq = new(Localizer.Instance.HttpRequest.NewRequest);
        AddHttpRequest(newReq, showItemInScreen: true);
    }

    private void AddNewWebSocketConnection()
    {
        PororocaWebSocketConnection newWs = new(Localizer.Instance.WebSocketConnection.NewConnection);
        AddWebSocketConnection(newWs, showItemInScreen: true);
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

    public void AddFolder(PororocaCollectionFolder folderToAdd, bool showItemInScreen = false)
    {
        const int numberOfFixedItems = 3; // variables, auth, environments
        CollectionFolderViewModel folderToAddVm = new(this, this, folderToAdd);

        int indexOfLastFolder = Items.GetLastIndexOf<CollectionFolderViewModel>();
        int indexToInsertAt = indexOfLastFolder == -1 ? numberOfFixedItems : (indexOfLastFolder + 1);
        Items.Insert(indexToInsertAt, folderToAddVm);

        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(folderToAddVm, showItemInScreen);
    }

    public void AddHttpRequest(PororocaHttpRequest reqToAdd, bool showItemInScreen = false)
    {
        HttpRequestViewModel reqToAddVm = new(this, this, reqToAdd);
        Items.Add(reqToAddVm); // always at the end

        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(reqToAddVm, showItemInScreen);
    }

    public void AddWebSocketConnection(PororocaWebSocketConnection wsToAdd, bool showItemInScreen = false)
    {
        WebSocketConnectionViewModel wsToAddVm = new(this, this, wsToAdd);
        Items.Add(wsToAddVm); // always at the end

        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(wsToAddVm, showItemInScreen);
    }

    protected override void CopyThis() =>
        throw new NotImplementedException();

    public override void PasteToThis()
    {
        var itemsToPaste = ClipboardArea.Instance.FetchCopiesOfFoldersAndReqs();
        foreach (var itemToPaste in itemsToPaste)
        {
            if (itemToPaste is PororocaCollectionFolder folderToPaste)
                AddFolder(folderToPaste);
            else if (itemToPaste is PororocaHttpRequest httpReqToPaste)
                AddHttpRequest(httpReqToPaste);
            else if (itemToPaste is PororocaWebSocketConnection wsToPaste)
                AddWebSocketConnection(wsToPaste);
        }

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
        var reqs = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel)
                        .Select(i =>
                        {
                            if (i is HttpRequestViewModel httpReqVm)
                                return (PororocaRequest)httpReqVm.ToHttpRequest();
                            if (i is WebSocketConnectionViewModel wsConnVm)
                                return (PororocaRequest)wsConnVm.ToWebSocketConnection();
                            else
                                throw new InvalidDataException();
                        }).ToList();

        return new PororocaCollection(this.colId, Name, this.colCreatedAt, Variables, CollectionScopedAuth, Environments, folders, reqs);
    }

    public EnvironmentViewModel? GetCurrentEnvironment() =>
        ((EnvironmentsGroupViewModel)Items.First(i => i is EnvironmentsGroupViewModel))
        .Items.FirstOrDefault(evm => evm.IsCurrentEnvironment);

    #endregion
}