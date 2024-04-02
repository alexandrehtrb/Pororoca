using System.Collections.ObjectModel;
using System.Reactive;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public abstract class RequestsAndFoldersParentViewModel : CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel>
{
    #region COLLECTION ORGANIZATION

    public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
    public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
    public ReactiveCommand<Unit, Unit> AddNewFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewHttpRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewWebSocketConnectionCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewHttpRepeaterCmd { get; }

    #endregion

    #region COLLECTION FOLDER

    internal CollectionViewModel Collection { get; }
    public override ObservableCollection<CollectionOrganizationItemViewModel> Items { get; }

    #endregion

    public RequestsAndFoldersParentViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                             string name) : base(parentVm, name)
    {
        #region COLLECTION ORGANIZATION

        AddNewFolderCmd = ReactiveCommand.Create(AddNewFolder);
        AddNewHttpRequestCmd = ReactiveCommand.Create(AddNewHttpRequest);
        AddNewWebSocketConnectionCmd = ReactiveCommand.Create(AddNewWebSocketConnection);
        AddNewHttpRepeaterCmd = ReactiveCommand.Create(AddNewHttpRepeater);

        #endregion

        #region COLLECTION FOLDER

        this.Collection = GetCollectionByClimbingParents();
        Items = new();

        #endregion
    }

    #region COLLECTION ORGANIZATION

    private CollectionViewModel GetCollectionByClimbingParents()
    {
        if (this is CollectionViewModel c) return c;

        var vm = Parent;
        while (vm is not CollectionViewModel)
        {
            vm = ((RequestsAndFoldersParentViewModel)vm).Parent;
        }
        return (CollectionViewModel)vm;
    }

    protected void AddInitialFoldersAndRequests(IEnumerable<PororocaCollectionFolder> folders, IEnumerable<PororocaRequest> requests)
    {
        foreach (var folder in folders)
        {
            Items.Add(new CollectionFolderViewModel(this, folder));
        }
        foreach (var req in requests)
        {
            if (req is PororocaHttpRequest httpReq)
                Items.Add(new HttpRequestViewModel(this, Collection, httpReq));
            else if (req is PororocaWebSocketConnection ws)
                Items.Add(new WebSocketConnectionViewModel(this, Collection, ws));
            else if (req is PororocaHttpRepetition rep)
                Items.Add(new HttpRepeaterViewModel(this, Collection, rep));
        }

        RefreshSubItemsAvailableMovements();
    }

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
            else if (itemToPaste is PororocaHttpRepetition httpRepToPaste)
                AddHttpRepeater(httpRepToPaste);
        }
    }

    private void AddNewFolder()
    {
        PororocaCollectionFolder newFolder = new(Localizer.Instance.Folder.NewFolder);
        AddFolder(newFolder, isNewItem: true);
    }

    private void AddNewHttpRequest()
    {
        PororocaHttpRequest newReq = new(Localizer.Instance.HttpRequest.NewRequest);
        AddHttpRequest(newReq, isNewItem: true);
    }

    private void AddNewWebSocketConnection()
    {
        PororocaWebSocketConnection newWs = new(Localizer.Instance.WebSocketConnection.NewConnection);
        AddWebSocketConnection(newWs, isNewItem: true);
    }

    private void AddNewHttpRepeater()
    {
        PororocaHttpRepetition newRep = new(Localizer.Instance.HttpRepeater.NewRepeater, Localizer.Instance.HttpRepeater.InputDataTypeRawComment);
        AddHttpRepeater(newRep, isNewItem: true);
    }

    public void AddFolder(PororocaCollectionFolder folderToAdd, bool isNewItem = false)
    {
        int indexToInsertAt;
        if (this is CollectionViewModel)
        {
            const int numberOfFixedItems = 3; // variables, auth, environments
            int indexOfLastFolder = Items.GetLastIndexOf<CollectionFolderViewModel>();
            indexToInsertAt = indexOfLastFolder == -1 ? numberOfFixedItems : (indexOfLastFolder + 1);
        }
        else
        {
            indexToInsertAt = Items.GetLastIndexOf<CollectionFolderViewModel>() + 1;
        }

        CollectionFolderViewModel folderToAddVm = new(this, folderToAdd);
        Items.Insert(indexToInsertAt, folderToAddVm);

        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(folderToAddVm, isNewItem);
    }

    public void AddHttpRequest(PororocaHttpRequest reqToAdd, bool isNewItem = false)
    {
        HttpRequestViewModel reqToAddVm = new(this, this.Collection, reqToAdd);
        Items.Add(reqToAddVm); // always at the end

        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(reqToAddVm, isNewItem);
        if (isNewItem)
        {
            Collection.AddHttpRequestPathToList(reqToAddVm.GetRequestPathInCollection());
        }
    }

    public void AddWebSocketConnection(PororocaWebSocketConnection wsToAdd, bool isNewItem = false)
    {
        WebSocketConnectionViewModel wsToAddVm = new(this, this.Collection, wsToAdd);
        Items.Add(wsToAddVm); // always at the end

        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(wsToAddVm, isNewItem);
    }

    public void AddHttpRepeater(PororocaHttpRepetition repToAdd, bool isNewItem = false)
    {
        HttpRepeaterViewModel repToAddVm = new(this, this.Collection, repToAdd);
        Items.Add(repToAddVm); // always at the end

        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(repToAddVm, isNewItem);
    }

    #endregion
}