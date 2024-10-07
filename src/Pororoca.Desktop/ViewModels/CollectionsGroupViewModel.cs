using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Pororoca.Desktop.HotKeys;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionsGroupViewModel : CollectionOrganizationItemParentViewModel<CollectionViewModel>
{
    #region COLLECTIONS GROUP

    public override ObservableCollection<CollectionViewModel> Items { get; }

    public ObservableCollection<CollectionOrganizationItemViewModel> CollectionGroupSelectedItems { get; }

    private CollectionOrganizationItemViewModel? collectionGroupSelectedItemField;
    public CollectionOrganizationItemViewModel? CollectionGroupSelectedItem
    {
        get => this.collectionGroupSelectedItemField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.collectionGroupSelectedItemField, value);
            this.onCollectionsGroupItemSelected(this.collectionGroupSelectedItemField);
        }
    }

    private readonly Action<CollectionOrganizationItemViewModel?> onCollectionsGroupItemSelected;

    #endregion

    public CollectionsGroupViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                     Action<CollectionOrganizationItemViewModel?> onCollectionsGroupItemSelected) : base(parentVm, string.Empty)
    {
        this.onCollectionsGroupItemSelected = onCollectionsGroupItemSelected;
        Items = new();
        CollectionGroupSelectedItems = new();
        CollectionGroupSelectedItems.CollectionChanged += OnCollectionGroupSelectedItemsChanged;
    }

    #region COLLECTIONS ORGANIZATION

    private void OnCollectionGroupSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        KeyboardShortcuts.Instance.HasMultipleItemsSelected = CollectionGroupSelectedItems.Count > 1;

    #endregion

    #region COPY AND PASTE

    public override void PasteToThis() => throw new NotImplementedException();

    #endregion
}