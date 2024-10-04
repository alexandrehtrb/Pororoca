using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public interface ICollectionOrganizationItemParentViewModel
{
    void MoveSubItemUp(CollectionOrganizationItemViewModel colItemVm);
    void MoveSubItemDown(CollectionOrganizationItemViewModel colItemVm);
    void DeleteSubItem(CollectionOrganizationItemViewModel item);
}

public abstract class CollectionOrganizationItemParentViewModel<T> : CollectionOrganizationItemViewModel, ICollectionOrganizationItemParentViewModel where T : CollectionOrganizationItemViewModel
{
    [Reactive]
    public bool IsExpanded { get; set; }

    public abstract ObservableCollection<T> Items { get; }

    public CollectionOrganizationItemParentViewModel(ICollectionOrganizationItemParentViewModel parentVm, string name) : base(parentVm, name)
    {
    }

    public void MoveSubItemUp(CollectionOrganizationItemViewModel colItemVm)
    {
        int currentIndex = Items.IndexOf((T)colItemVm);
        if (currentIndex != -1 && colItemVm.CanMoveUp)
        {
            int newIndex = currentIndex - 1;
            Items.Move(currentIndex, newIndex);
            RefreshSubItemsAvailableMovements();
        }
    }

    public void MoveSubItemDown(CollectionOrganizationItemViewModel colItemVm)
    {
        int currentIndex = Items.IndexOf((T)colItemVm);
        if (currentIndex != -1 && colItemVm.CanMoveDown)
        {
            int newIndex = currentIndex + 1;
            Items.Move(currentIndex, newIndex);
            RefreshSubItemsAvailableMovements();
        }
    }

    public virtual void RefreshSubItemsAvailableMovements()
    {
        for (int x = 0; x < Items.Count; x++)
        {
            if (Items[x] is CollectionOrganizationItemViewModel colItemVm)
            {
                colItemVm.CanMoveUp = x > 0;
                colItemVm.CanMoveDown = x < Items.Count - 1;
            }
        }
    }

    public abstract void PasteToThis();

    public void DeleteSubItem(CollectionOrganizationItemViewModel item)
    {
        Items.Remove((T)item);
        RefreshSubItemsAvailableMovements();
        MainWindowVm.HideAllPages();
    }
}