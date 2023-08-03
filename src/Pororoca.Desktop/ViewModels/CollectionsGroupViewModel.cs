using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Pororoca.Desktop.HotKeys;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionsGroupViewModel : CollectionOrganizationItemParentViewModel<CollectionViewModel>
{
    #region COLLECTIONS ORGANIZATION

    public override Action OnAfterItemDeleted => Parent.OnAfterItemDeleted;
    public override Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => Parent.OnRenameSubItemSelected;

    #endregion

    #region COLLECTIONS GROUP

    public override ObservableCollection<CollectionViewModel> Items { get; }

    public ObservableCollection<ViewModelBase> CollectionGroupSelectedItems { get; }

    private ViewModelBase? collectionGroupSelectedItemField;
    public ViewModelBase? CollectionGroupSelectedItem
    {
        get => this.collectionGroupSelectedItemField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.collectionGroupSelectedItemField, value);
            this.onCollectionsGroupItemSelected(this.collectionGroupSelectedItemField);
        }
    }

    private readonly Action<ViewModelBase?> onCollectionsGroupItemSelected;

    #endregion

    public CollectionsGroupViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                     Action<ViewModelBase?> onCollectionsGroupItemSelected) : base(parentVm, string.Empty)
    {
        this.onCollectionsGroupItemSelected = onCollectionsGroupItemSelected;
        Items = new();
        CollectionGroupSelectedItems = new();
        CollectionGroupSelectedItems.CollectionChanged += OnCollectionGroupSelectedItemsChanged;
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
        KeyboardShortcuts.Instance.HasMultipleItemsSelected = CollectionGroupSelectedItems.Count > 1;

    #endregion

    #region COPY AND PASTE

    protected override void CopyThis() => throw new NotImplementedException();

    public override void PasteToThis() => throw new NotImplementedException();

    #endregion
}