using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class CollectionFolderViewModel : CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel>
    {
        #region COLLECTION ORGANIZATION

        public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
        public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;
        public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
        public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
        public ReactiveCommand<Unit, Unit> AddNewFolderCmd { get; }
        public ReactiveCommand<Unit, Unit> AddNewRequestCmd { get; }
        public ReactiveCommand<Unit, Unit> CopyFolderCmd { get; }
        public ReactiveCommand<Unit, Unit> PasteToFolderCmd { get; }
        public ReactiveCommand<Unit, Unit> RenameFolderCmd { get; }
        public ReactiveCommand<Unit, Unit> DeleteFolderCmd { get; }

        #endregion

        #region COLLECTION FOLDER

        private readonly IPororocaVariableResolver _variableResolver;
        private readonly Guid _folderId;
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
            AddNewRequestCmd = ReactiveCommand.Create(AddNewRequest);
            CopyFolderCmd = ReactiveCommand.Create(Copy);
            PasteToFolderCmd= ReactiveCommand.Create(PasteToThis);
            RenameFolderCmd = ReactiveCommand.Create(RenameThis);
            DeleteFolderCmd = ReactiveCommand.Create(Delete);

            #endregion

            #region COLLECTION FOLDER

            _variableResolver = variableResolver;
            _folderId = folder.Id;

            Items = new();
            foreach (PororocaCollectionFolder subFolder in folder.Folders)
                Items.Add(new CollectionFolderViewModel(this, variableResolver, subFolder));
            foreach (PororocaRequest req in folder.Requests)
                Items.Add(new RequestViewModel(this, variableResolver, req));

            RefreshSubItemsAvailableMovements();

            #endregion
        }

        #region COLLECTION ORGANIZATION

        public override void RefreshSubItemsAvailableMovements()
        {
            for (int x = 0; x < Items.Count; x++)
            {
                CollectionOrganizationItemViewModel colItemVm = Items[x];
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
                else if (itemToPaste is PororocaRequest reqToPaste)
                    AddRequest(reqToPaste);
            }
        }

        private void AddNewFolder()
        {
            PororocaCollectionFolder newFolder = new(Localizer.Instance["Folder/NewFolder"]);
            AddFolder(newFolder);
        }

        private void AddNewRequest()
        {
            PororocaRequest newReq = new(Localizer.Instance["Request/NewRequest"]);
            AddRequest(newReq);
        }

        public void AddFolder(PororocaCollectionFolder folderToAdd)
        {
            IEnumerable<CollectionOrganizationItemViewModel> existingFolders = Items.Where(i => i is CollectionFolderViewModel);
            IEnumerable<CollectionOrganizationItemViewModel> existingRequests = Items.Where(i => i is RequestViewModel);
            CollectionFolderViewModel folderToAddVm = new(this, _variableResolver, folderToAdd);

            CollectionOrganizationItemViewModel[] rearrangedItems = existingFolders.Append(folderToAddVm)
                                                                                   .Concat(existingRequests)
                                                                                   .ToArray();
            Items.Clear();
            foreach (CollectionOrganizationItemViewModel item in rearrangedItems)
            {
                Items.Add(item);
            }
            this.IsExpanded = true;
            RefreshSubItemsAvailableMovements();
        }

        public void AddRequest(PororocaRequest reqToAdd)
        {
            CollectionOrganizationItemViewModel[] existingFolders = Items.Where(i => i is CollectionFolderViewModel).ToArray();
            CollectionOrganizationItemViewModel[] existingRequests = Items.Where(i => i is RequestViewModel).ToArray();
            RequestViewModel reqToAddVm = new(this, _variableResolver, reqToAdd);

            CollectionOrganizationItemViewModel[] rearrangedItems = existingFolders.Concat(existingRequests)
                                                                                   .Append(reqToAddVm)
                                                                                   .ToArray();
            Items.Clear();
            foreach (CollectionOrganizationItemViewModel item in rearrangedItems)
            {
                Items.Add(item);
            }
            this.IsExpanded = true;
            RefreshSubItemsAvailableMovements();
        }

        #endregion

        #region COLLECTION FOLDER

        public PororocaCollectionFolder ToCollectionFolder()
        {
            PororocaCollectionFolder newFolder = new(_folderId, Name);
            foreach (CollectionOrganizationItemViewModel colItemVm in Items)
            {
                if (colItemVm is CollectionFolderViewModel colFolderVm)
                    newFolder.AddFolder(colFolderVm.ToCollectionFolder());
                else if (colItemVm is RequestViewModel reqVm)
                    newFolder.AddRequest(reqVm.ToRequest());
            }
            return newFolder;
        }

        #endregion
    }
}