using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
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

    [Reactive]
    public bool HasMultipleItemsSelected { get; set; }

    private readonly Action<ViewModelBase?> onCollectionsGroupItemSelected;

    #endregion

    #region COPY

    private readonly List<ICloneable> copiedDomainObjs;

    [Reactive]
    public bool CanPasteEnvironment { get; set; }

    [Reactive]
    public bool CanPasteCollectionFolderOrRequest { get; set; }

    [Reactive]
    public bool CanPasteWebSocketClientMessage { get; set; }

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

    private bool HasAnyParentAlsoSelected(CollectionOrganizationItemViewModel itemVm)
    {
        if (itemVm is not HttpRequestViewModel
         && itemVm is not WebSocketConnectionViewModel
         && itemVm is not WebSocketClientMessageViewModel
         && itemVm is not CollectionFolderViewModel)
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
        CanPasteWebSocketClientMessage = this.copiedDomainObjs.Any(o => o is PororocaWebSocketClientMessage);
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

    public IList<PororocaWebSocketClientMessage> FetchCopiesOfWebSocketClientMessages() =>
        this.copiedDomainObjs.Where(o => o is PororocaWebSocketClientMessage)
                        .Select(o => o.Clone())
                        .Cast<PororocaWebSocketClientMessage>()
                        .ToList();

    public void CopyMultiple()
    {
        var reqsToCopy = CollectionGroupSelectedItems
                         .Where(i => i is HttpRequestViewModel reqVm && !HasAnyParentAlsoSelected(reqVm))
                         .Select(r => (ICloneable)((HttpRequestViewModel)r).ToHttpRequest());
        var wssToCopy = CollectionGroupSelectedItems
                         .Where(i => i is WebSocketConnectionViewModel wsVm && !HasAnyParentAlsoSelected(wsVm))
                         .Select(wsVm => (ICloneable)((WebSocketConnectionViewModel)wsVm).ToWebSocketConnection());
        var foldersToCopy = CollectionGroupSelectedItems
                            .Where(i => i is CollectionFolderViewModel folderVm && !HasAnyParentAlsoSelected(folderVm))
                            .Select(f => (ICloneable)((CollectionFolderViewModel)f).ToCollectionFolder());
        var envsToCopy = CollectionGroupSelectedItems
                         .Where(i => i is EnvironmentViewModel)
                         .Select(e => (ICloneable)((EnvironmentViewModel)e).ToEnvironment());

        var itemsToCopy = reqsToCopy.Concat(wssToCopy).Concat(foldersToCopy).Concat(envsToCopy).ToArray();

        PushToCopy(itemsToCopy);
    }

    #endregion

    #region DELETE

    public void DeleteMultiple() =>
        CollectionGroupSelectedItems
        .Where(i => i is CollectionViewModel
                 || i is CollectionFolderViewModel
                 || i is EnvironmentViewModel
                 || i is HttpRequestViewModel
                 || i is WebSocketConnectionViewModel
                 || i is WebSocketClientMessageViewModel)
        .Cast<CollectionOrganizationItemViewModel>()
        .ToList()
        .ForEach(i => i.DeleteThis());

    #endregion
}