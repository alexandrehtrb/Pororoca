using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;
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
    public ReactiveCommand<Unit, Unit> CopyFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteToFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameFolderCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteFolderCmd { get; }

    #endregion

    #region COLLECTION FOLDER

    private readonly IPororocaVariableResolver variableResolver;
    private readonly Guid folderId;
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
        CopyFolderCmd = ReactiveCommand.Create(Copy);
        PasteToFolderCmd = ReactiveCommand.Create(PasteToThis);
        RenameFolderCmd = ReactiveCommand.Create(RenameThis);
        DeleteFolderCmd = ReactiveCommand.Create(Delete);

        #endregion

        #region COLLECTION FOLDER

        this.variableResolver = variableResolver;
        this.folderId = folder.Id;

        Items = new();
        foreach (var subFolder in folder.Folders)
            Items.Add(new CollectionFolderViewModel(this, variableResolver, subFolder));
        foreach (var req in folder.HttpRequests)
            Items.Add(new HttpRequestViewModel(this, variableResolver, req));

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
        }
    }

    private void AddNewFolder()
    {
        PororocaCollectionFolder newFolder = new(Localizer.Instance["Folder/NewFolder"]);
        AddFolder(newFolder);
    }

    private void AddNewHttpRequest()
    {
        PororocaHttpRequest newReq = new(Localizer.Instance["HttpRequest/NewRequest"]);
        AddHttpRequest(newReq);
    }

    public void AddFolder(PororocaCollectionFolder folderToAdd)
    {
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel);
        var existingRequests = Items.Where(i => i is HttpRequestViewModel);
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
    }

    public void AddHttpRequest(PororocaHttpRequest reqToAdd)
    {
        var existingFolders = Items.Where(i => i is CollectionFolderViewModel).ToArray();
        var existingRequests = Items.Where(i => i is HttpRequestViewModel).ToArray();
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
    }

    #endregion

    #region COLLECTION FOLDER

    public PororocaCollectionFolder ToCollectionFolder()
    {
        PororocaCollectionFolder newFolder = new(this.folderId, Name);
        foreach (var colItemVm in Items)
        {
            if (colItemVm is CollectionFolderViewModel colFolderVm)
                newFolder.AddFolder(colFolderVm.ToCollectionFolder());
            else if (colItemVm is HttpRequestViewModel reqVm)
                newFolder.AddRequest(reqVm.ToHttpRequest());
        }
        return newFolder;
    }

    #endregion
}