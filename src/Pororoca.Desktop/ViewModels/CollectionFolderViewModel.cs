using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionFolderViewModel : RequestsAndFoldersParentViewModel
{

    public CollectionFolderViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                     PororocaCollectionFolder folder) : base(parentVm, folder.Name)
    {
        #region COLLECTION FOLDER

        AddInitialFoldersAndRequests(folder.Folders, folder.Requests);

        #endregion
    }

    #region COLLECTION ORGANIZATION

    public override void RefreshSubItemsAvailableMovements()
    {
        for (int x = 0; x < Items.Count; x++)
        {
            var colItemVm = Items[x];
            int indexOfLastSubfolder = Items.GetLastIndexOf<CollectionFolderViewModel>();
            if (colItemVm is CollectionFolderViewModel)
            {
                colItemVm.CanMoveUp = x > 0;
                colItemVm.CanMoveDown = x < indexOfLastSubfolder;
            }
            else // http requests and websockets
            {
                colItemVm.CanMoveUp = x > (indexOfLastSubfolder + 1);
                colItemVm.CanMoveDown = x < Items.Count - 1;
            }
        }
    }

    protected override void CopyThis() =>
        ClipboardArea.Instance.PushToCopy(ToCollectionFolder());

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
            else if (colItemVm is HttpRepeaterViewModel repVm)
                newFolder.AddRequest(repVm.ToHttpRepetition());
        }
        return newFolder;
    }

    #endregion
}