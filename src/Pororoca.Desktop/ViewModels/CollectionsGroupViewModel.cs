using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;

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

    private bool hasMultipleItemsSelectedField;
    public bool HasMultipleItemsSelected
    {
        get => this.hasMultipleItemsSelectedField;
        private set => this.RaiseAndSetIfChanged(ref this.hasMultipleItemsSelectedField, value);
    }

    private readonly Action<ViewModelBase?> onCollectionsGroupItemSelected;

    #endregion

    #region COPY

    private readonly List<ICloneable> copiedDomainObjs;

    private bool canPasteEnvironmentField;
    public bool CanPasteEnvironment
    {
        get => this.canPasteEnvironmentField;
        private set => this.RaiseAndSetIfChanged(ref this.canPasteEnvironmentField, value);
    }

    private bool canPasteCollectionFolderOrRequestField;
    public bool CanPasteCollectionFolderOrRequest
    {
        get => this.canPasteCollectionFolderOrRequestField;
        private set => this.RaiseAndSetIfChanged(ref this.canPasteCollectionFolderOrRequestField, value);
    }

    #endregion

    public CollectionsGroupViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                     Action<ViewModelBase?> onCollectionsGroupItemSelected) : base(parentVm, string.Empty)
    {
        this.onCollectionsGroupItemSelected = onCollectionsGroupItemSelected;
        Items = new();
        CollectionGroupSelectedItems = new();
        CollectionGroupSelectedItems.CollectionChanged += OnCollectionGroupSelectedItemsChanged;
        this.copiedDomainObjs = new();
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
        if (itemVm is not HttpRequestViewModel && itemVm is not CollectionFolderViewModel)
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
        this.copiedDomainObjs.Clear();
        this.copiedDomainObjs.AddRange(domainObjsToCopy);
        CanPasteEnvironment = this.copiedDomainObjs.Any(o => o is PororocaEnvironment);
        CanPasteCollectionFolderOrRequest = this.copiedDomainObjs.Any(o => o is PororocaCollectionFolder || o is PororocaRequest);
    }

    public IList<PororocaCollectionItem> FetchCopiesOfFoldersAndReqs() =>
        this.copiedDomainObjs.Where(o => o is PororocaCollectionFolder || o is PororocaRequest)
                             .Select(o => o.Clone())
                             .Cast<PororocaCollectionItem>()
                             .ToList();

    public IList<PororocaEnvironment> FetchCopiesOfEnvironments() =>
        this.copiedDomainObjs.Where(o => o is PororocaEnvironment)
                        .Select(o => o.Clone())
                        .Cast<PororocaEnvironment>()
                        .ToList();

    public void CopyMultiple()
    {
        var reqsToCopy = CollectionGroupSelectedItems
                         .Where(i => i is HttpRequestViewModel reqVm && !HasAnyParentFolderAlsoSelected(reqVm))
                         .Select(r => (ICloneable)((HttpRequestViewModel)r).ToHttpRequest());
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
                 || i is HttpRequestViewModel)
        .Cast<CollectionOrganizationItemViewModel>()
        .ToList()
        .ForEach(i => i.DeleteThis());

    #endregion
}