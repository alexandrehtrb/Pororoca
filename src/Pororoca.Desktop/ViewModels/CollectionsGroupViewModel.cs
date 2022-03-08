using System;
using System.Collections.ObjectModel;
using System.Text;
using ReactiveUI;
using Pororoca.Domain.Features.Entities.Pororoca;

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

        private readonly Action<ViewModelBase?> _onCollectionsGroupItemSelected;

        #endregion

        public CollectionsGroupViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                         Action<ViewModelBase?> onCollectionsGroupItemSelected) : base(parentVm, string.Empty)
        {
            _onCollectionsGroupItemSelected = onCollectionsGroupItemSelected;
            Items = new();
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
        protected override void CopyThis() => throw new NotImplementedException();
        public override void PasteToThis() => throw new NotImplementedException();

        #endregion
    }
}