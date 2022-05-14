using System;
using System.Collections.ObjectModel;
using System.Text;
using ReactiveUI;
using Pororoca.Domain.Features.Entities.Pororoca;
using System.Collections.Specialized;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class CollectionsGroupViewModel : CollectionOrganizationItemParentViewModel<CollectionViewModel>
    {
        #region COLLECTIONS ORGANIZATION

        public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
        public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;

        #endregion

        #region COLLECTIONS GROUP

        public override ObservableCollection<CollectionViewModel> Items { get; }

        public ObservableCollection<ViewModelBase> CollectionGroupSelectedItems { get; }

        private ViewModelBase? _collectionGroupSelectedItem;
        public ViewModelBase? CollectionGroupSelectedItem
        {
            get => _collectionGroupSelectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _collectionGroupSelectedItem, value);
                _onCollectionsGroupItemSelected(_collectionGroupSelectedItem);
            }
        }

        private bool _hasMultipleItemsSelected;
        public bool HasMultipleItemsSelected
        {
            get => _hasMultipleItemsSelected;
            private set
            {
                this.RaiseAndSetIfChanged(ref _hasMultipleItemsSelected, value);
            }
        }

        private readonly Action<ViewModelBase?> _onCollectionsGroupItemSelected;

        #endregion

        #region COPY

        private readonly List<ICloneable> copiedDomainObjs;

        private bool _canPasteEnvironment;
        public bool CanPasteEnvironment
        {
            get => _canPasteEnvironment;
            private set
            {
                this.RaiseAndSetIfChanged(ref _canPasteEnvironment, value);
            }
        }

        private bool _canPasteCollectionFolderOrRequest;
        public bool CanPasteCollectionFolderOrRequest
        {
            get => _canPasteCollectionFolderOrRequest;
            private set
            {
                this.RaiseAndSetIfChanged(ref _canPasteCollectionFolderOrRequest, value);
            }
        }

        #endregion

        public CollectionsGroupViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                         Action<ViewModelBase?> onCollectionsGroupItemSelected) : base(parentVm, string.Empty)
        {
            _onCollectionsGroupItemSelected = onCollectionsGroupItemSelected;
            Items = new();
            CollectionGroupSelectedItems = new();
            CollectionGroupSelectedItems.CollectionChanged += OnCollectionGroupSelectedItemsChanged;
            copiedDomainObjs = new();
        }

        #region COLLECTIONS ORGANIZATION

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
        private void OnCollectionGroupSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
            HasMultipleItemsSelected = CollectionGroupSelectedItems.Count > 1;

        private bool HasAnyParentFolderAlsoSelected(CollectionOrganizationItemViewModel itemVm)
        {
            if (itemVm is not RequestViewModel && itemVm is not CollectionFolderViewModel)
                return false;

            var parent = itemVm.Parent;
            while (parent != null)
            {
                if (CollectionGroupSelectedItems.Contains((ViewModelBase)parent))
                    return true;
                else if (parent is CollectionFolderViewModel parentAsItem)
                    parent = parentAsItem.Parent;
                else
                    break;
            }
            return false;
        }

        #endregion

        #region COPY AND PASTE

        protected override void CopyThis() => throw new NotImplementedException();

        public override void PasteToThis() => throw new NotImplementedException();

        public void PushToCopy(params ICloneable[] domainObjsToCopy)
        {
            copiedDomainObjs.Clear();
            copiedDomainObjs.AddRange(domainObjsToCopy);
            CanPasteEnvironment = copiedDomainObjs.Any(o => o is PororocaEnvironment);
            CanPasteCollectionFolderOrRequest = copiedDomainObjs.Any(o => o is PororocaCollectionFolder || o is PororocaRequest);
        }

        public IList<PororocaCollectionItem> FetchCopiesOfFoldersAndReqs() =>
            copiedDomainObjs.Where(o => o is PororocaCollectionFolder || o is PororocaRequest)
                            .Select(o => o.Clone())
                            .Cast<PororocaCollectionItem>()
                            .ToList();

        public IList<PororocaEnvironment> FetchCopiesOfEnvironments() =>
            copiedDomainObjs.Where(o => o is PororocaEnvironment)
                            .Select(o => o.Clone())
                            .Cast<PororocaEnvironment>()
                            .ToList();

        public void CopyMultiple()
        {
            var reqsToCopy = CollectionGroupSelectedItems
                             .Where(i => i is RequestViewModel reqVm && !HasAnyParentFolderAlsoSelected(reqVm))
                             .Select(r => (ICloneable)((RequestViewModel)r).ToRequest());
            var foldersToCopy = CollectionGroupSelectedItems
                                .Where(i => i is CollectionFolderViewModel folderVm && !HasAnyParentFolderAlsoSelected(folderVm))
                                .Select(f => (ICloneable)((CollectionFolderViewModel)f).ToCollectionFolder());
            var envsToCopy = CollectionGroupSelectedItems
                             .Where(i => i is EnvironmentViewModel)
                             .Select(e => (ICloneable)((EnvironmentViewModel)e).ToEnvironment());

            var itemsToCopy = reqsToCopy.Concat(foldersToCopy).Concat(envsToCopy).ToArray();

            PushToCopy(itemsToCopy);
        }

        #endregion

        #region DELETE

        public void DeleteMultiple() =>
            CollectionGroupSelectedItems
            .Where(i => i is CollectionViewModel
                     || i is CollectionFolderViewModel
                     || i is EnvironmentViewModel
                     || i is RequestViewModel)
            .Cast<CollectionOrganizationItemViewModel>()
            .ToList()
            .ForEach(i => i.DeleteThis());

        #endregion
    }
}