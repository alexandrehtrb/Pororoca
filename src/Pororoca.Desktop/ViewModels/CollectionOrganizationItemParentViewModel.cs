using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels
{
    public interface ICollectionOrganizationItemParentViewModel
    {
        Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected { get; }
        Action OnAfterItemDeleted { get; }
        void MoveSubItem(ICollectionOrganizationItemViewModel colItemVm, MoveableItemMovementDirection direction);
        void DeleteSubItem(ICollectionOrganizationItemViewModel item);
    }

    public abstract class CollectionOrganizationItemParentViewModel<T> : CollectionOrganizationItemViewModel, ICollectionOrganizationItemParentViewModel where T : ICollectionOrganizationItemViewModel
    {
        public abstract ObservableCollection<T> Items { get; }
        public abstract Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected { get; }
        public abstract Action OnAfterItemDeleted { get; }

        public CollectionOrganizationItemParentViewModel(ICollectionOrganizationItemParentViewModel parentVm, string name) : base(parentVm, name)
        {
        }

        public void MoveSubItem(ICollectionOrganizationItemViewModel colItemVm, MoveableItemMovementDirection direction)
        {
            int currentIndex = Items.IndexOf((T)colItemVm);
            if (currentIndex != -1)
            {
                if (direction == MoveableItemMovementDirection.Up && colItemVm.CanMoveUp)
                {
                    int newIndex = currentIndex - 1;
                    Items.Move(currentIndex, newIndex);
                    RefreshSubItemsAvailableMovements();
                }
                else if (direction == MoveableItemMovementDirection.Down && colItemVm.CanMoveDown)
                {
                    int newIndex = currentIndex + 1;
                    Items.Move(currentIndex, newIndex);
                    RefreshSubItemsAvailableMovements();
                }
            }
        }

        public abstract void RefreshSubItemsAvailableMovements();
        
        public abstract void PasteToThis();

        public void DeleteSubItem(ICollectionOrganizationItemViewModel item)
        {
            Items.Remove((T)item);
            RefreshSubItemsAvailableMovements();
            OnAfterItemDeleted();
        }        
    }
}