using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionFolderViewModel : CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel>
{
    #region COLLECTION ORGANIZATION

    public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
    public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewHttpRequestCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewWebSocketConnectionCmd { get; }
    public ReactiveCommand<Unit, Unit> CopyFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteToFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteFolderCmd { get; }

    #endregion

    #region COLLECTION FOLDER

    private readonly IPororocaVariableResolver variableResolver;
    public override ObservableCollection<CollectionOrganizationItemViewModel> Items { get; }

    #endregion

    public CollectionFolderViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                     IPororocaVariableResolver variableResolver,
                                     PororocaCollectionFolder folder) : base(parentVm, folder.Name)
    {
        #region COLLECTION ORGANIZATION

        MoveUpCmd = ReactiveCommand.Create(MoveThisUp);
        MoveDownCmd = ReactiveCommand.Create(MoveThisDown);
        AddNewFolderCmd = ReactiveCommand.Create(AddNewFolder);
        AddNewHttpRequestCmd = ReactiveCommand.Create(AddNewHttpRequest);
        AddNewWebSocketConnectionCmd = ReactiveCommand.Create(AddNewWebSocketConnection);
        CopyFolderCmd = ReactiveCommand.Create(Copy);
        PasteToFolderCmd = ReactiveCommand.Create(PasteToThis);
        RenameFolderCmd = ReactiveCommand.Create(RenameThis);
        DeleteFolderCmd = ReactiveCommand.Create(Delete);

        #endregion

        #region COLLECTION FOLDER

        this.variableResolver = variableResolver;

        Items = new();
        foreach (var subFolder in folder.Folders)
            Items.Add(new CollectionFolderViewModel(this, variableResolver, subFolder));
        foreach (var req in folder.HttpRequests)
            Items.Add(new HttpRequestViewModel(this, variableResolver, req));
        foreach (var ws in folder.WebSocketConnections)
            Items.Add(new WebSocketConnectionViewModel(this, variableResolver, ws));

        RefreshSubItemsAvailableMovements();

        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void RefreshSubItemsAvailableMovements()
    {
        for (int x = 0; x < Items.Count; x++)
        {
            var colItemVm = Items[x];
            bool canMoveUp = x > 0;
            bool canMoveDown = x < Items.Count - 1;
            colItemVm.CanMoveUp = canMoveUp;
            colItemVm.CanMoveDown = canMoveDown;
        }
    }

    protected override void CopyThis() =>
        CollectionsGroupDataCtx.PushToCopy(ToCollectionFolder());

    public override void PasteToThis()
    {
        var itemsToPaste = CollectionsGroupDataCtx.FetchCopiesOfFoldersAndReqs();
        foreach (var itemToPaste in itemsToPaste)
        {
            if (itemToPaste is PororocaCollectionFolder folderToPaste)
                AddFolder(folderToPaste);
            else if (itemToPaste is PororocaHttpRequest httpReqToPaste)
                AddHttpRequest(httpReqToPaste);
            else if (itemToPaste is PororocaWebSocketConnection wsToPaste)
                AddWebSocketConnection(wsToPaste);
        }
    }

    private void AddNewFolder()
    {
        PororocaCollectionFolder newFolder = new(Localizer.Instance["Folder/NewFolder"]);
        AddFolder(newFolder, showItemInScreen: true);
    }

    private void AddNewHttpRequest()
    {
        PororocaHttpRequest newReq = new(Localizer.Instance["HttpRequest/NewRequest"]);
        AddHttpRequest(newReq, showItemInScreen: true);
    }

    private void AddNewWebSocketConnection()
    {
        PororocaWebSocketConnection newWs = new(Localizer.Instance["WebSocketConnection/NewConnection"]);
        AddWebSocketConnection(newWs, showItemInScreen: true);
    }

    public void AddFolder(PororocaCollectionFolder folderToAdd, bool showItemInScreen = false)
    {
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel);
        var existingRequests = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel);
        CollectionFolderViewModel folderToAddVm = new(this, this.variableResolver, folderToAdd);

        var rearrangedItems = existingFolders.Append(folderToAddVm)
                                             .Concat(existingRequests)
                                             .ToArray();
        Items.Clear();
        foreach (var item in rearrangedItems)
        {
            Items.Add(item);
        }
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(folderToAddVm, showItemInScreen);
    }

    public void AddHttpRequest(PororocaHttpRequest reqToAdd, bool showItemInScreen = false)
    {
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel).ToArray();
        var existingRequests = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel).ToArray();
        HttpRequestViewModel reqToAddVm = new(this, this.variableResolver, reqToAdd);

        var rearrangedItems = existingFolders.Concat(existingRequests)
                                             .Append(reqToAddVm)
                                             .ToArray();
        Items.Clear();
        foreach (var item in rearrangedItems)
        {
            Items.Add(item);
        }
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(reqToAddVm, showItemInScreen);
    }

    public void AddWebSocketConnection(PororocaWebSocketConnection wsToAdd, bool showItemInScreen = false)
    {
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel).ToArray();
        var existingRequests = Items.Where(i => i is HttpRequestViewModel || i is WebSocketConnectionViewModel).ToArray();
        WebSocketConnectionViewModel wsToAddVm = new(this, this.variableResolver, wsToAdd);

        var rearrangedItems = existingFolders.Concat(existingRequests)
                                             .Append(wsToAddVm)
                                             .ToArray();
        Items.Clear();
        foreach (var item in rearrangedItems)
        {
            Items.Add(item);
        }
        IsExpanded = true;
        RefreshSubItemsAvailableMovements();
        SetAsItemInFocus(wsToAddVm, showItemInScreen);
    }

    #endregion

    #region COLLECTION FOLDER

    public PororocaCollectionFolder ToCollectionFolder()
    {
        PororocaCollectionFolder newFolder = new(Name);
        foreach (var colItemVm in Items)
        {
            if (colItemVm is CollectionFolderViewModel colFolderVm)
                newFolder.AddFolder(colFolderVm.ToCollectionFolder());
            else if (colItemVm is HttpRequestViewModel reqVm)
                newFolder.AddRequest(reqVm.ToHttpRequest());
            else if (colItemVm is WebSocketConnectionViewModel wsVm)
                newFolder.AddRequest(wsVm.ToWebSocketConnection());
        }
        return newFolder;
    }

    #endregion
}