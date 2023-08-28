using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Threading;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using ReactiveUI;
using Avalonia.Platform;
using Pororoca.Desktop.Localization;
using Avalonia.Media.Imaging;
using ReactiveUI.Fody.Helpers;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using System.Security.Cryptography;

namespace Pororoca.Desktop.HotKeys;

public sealed class KeyboardShortcuts : ViewModelBase
{
    internal static readonly KeyboardShortcuts Instance = new();

    [Reactive]
    public bool HasMultipleItemsSelected { get; set; }

    public ReactiveCommand<Unit, Unit> CutCmd { get; }
    public ReactiveCommand<Unit, Unit> CopyCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteCmd { get; }
    public ReactiveCommand<Unit, Unit> DuplicateCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> SwitchToPreviousItemCmd { get; }
    public ReactiveCommand<Unit, Unit> SwitchToNextItemCmd { get; }
    public ReactiveCommand<Unit, Unit> ShowHelpCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameCmd { get; }
    public ReactiveCommand<Unit, Unit> SendRequestOrConnectWebSocketCmd { get; }
    public ReactiveCommand<Unit, Unit> CancelRequestOrDisconnectWebSocketCmd { get; }
    public ReactiveCommand<Unit, Unit> CyclePreviousEnvironmentToActiveCmd { get; }
    public ReactiveCommand<Unit, Unit> CycleNextEnvironmentToActiveCmd { get; }
    public ReactiveCommand<Unit, Unit> SaveResponseToFileCmd { get; }
    public ReactiveCommand<Unit, Unit> FocusOnUrlCmd { get; }

    #region HELPER PROPERTIES

    private MainWindowViewModel MainWindowVm => 
        ((MainWindowViewModel)MainWindow.Instance!.DataContext!);

    private CollectionsGroupViewModel CollectionsGroupVm => 
        MainWindowVm.CollectionsGroupViewDataCtx;

    private CollectionOrganizationItemViewModel? SelectedItem =>
        CollectionsGroupVm.CollectionGroupSelectedItem;

    private ObservableCollection<CollectionOrganizationItemViewModel> SelectedItems =>
        CollectionsGroupVm.CollectionGroupSelectedItems;
    
    private List<CollectionOrganizationItemViewModel> GetItemsTreeLinearized()
    {
        static List<CollectionOrganizationItemViewModel> GetSubItemsLinearized(CollectionOrganizationItemViewModel parentVm)
        {
            List<CollectionOrganizationItemViewModel> linearizedItems = new();
            if (parentVm is CollectionsGroupViewModel d)
            {
                linearizedItems.Add(d);
                linearizedItems.AddRange(d.Items.SelectMany(x => GetSubItemsLinearized(x)));
            }
            else if (parentVm is CollectionViewModel a)
            {
                linearizedItems.Add(a);
                linearizedItems.Add(a.Items[0]);// collection variables
                linearizedItems.AddRange(((EnvironmentsGroupViewModel)a.Items[1]).Items);// collection environments
                
                // small optimization
                for (int i = 2; i < a.Items.Count; i++)
                {
                    var subItem = a.Items[i];
                    if (subItem is HttpRequestViewModel)
                    {
                        linearizedItems.Add(subItem);
                    }
                    else
                    {
                        linearizedItems.AddRange(GetSubItemsLinearized(subItem));
                    }
                }
            }
            else if (parentVm is CollectionFolderViewModel e)
            {
                linearizedItems.Add(e);
                if (e.Items.All(x => x is HttpRequestViewModel))
                {
                    // small optimization
                    linearizedItems.AddRange(e.Items);
                }
                else
                {
                    linearizedItems.AddRange(e.Items.SelectMany(x => GetSubItemsLinearized(x)));
                }
            }
            else if (parentVm is WebSocketConnectionViewModel c)
            {
                linearizedItems.Add(c);
                linearizedItems.AddRange(c.Items); // all subitems are ws client msgs
            }
            else
            {
                linearizedItems.Add(parentVm);
            }

            return linearizedItems;
        }

        return GetSubItemsLinearized(CollectionsGroupVm);
    }

    #endregion

    private KeyboardShortcuts()
    {
        // TODO: Add keyboard shortcut for undo (Ctrl+Z)
        CutCmd = ReactiveCommand.Create(CutSelectedItems);
        CopyCmd = ReactiveCommand.Create(CopySelectedItems);
        PasteCmd = ReactiveCommand.Create(PasteCopiedItems);
        DeleteCmd = ReactiveCommand.Create(AskUserToConfirmDeleteItems);
        DuplicateCmd = ReactiveCommand.Create(DuplicateSelectedItem);
        // Moving items up and down is not great yet
        MoveUpCmd = ReactiveCommand.Create(MoveSelectedItemUp);
        MoveDownCmd = ReactiveCommand.Create(MoveSelectedItemDown);
        SwitchToPreviousItemCmd = ReactiveCommand.Create(SwitchToPreviousItem);
        SwitchToNextItemCmd = ReactiveCommand.Create(SwitchToNextItem);
        ShowHelpCmd = ReactiveCommand.Create(ShowHelpDialog);
        RenameCmd = ReactiveCommand.Create(RenameSelectedItem);
        FocusOnUrlCmd = ReactiveCommand.Create(FocusOnUrl);
        SendRequestOrConnectWebSocketCmd = ReactiveCommand.Create(SendRequestOrConnectWebSocket);
        CancelRequestOrDisconnectWebSocketCmd = ReactiveCommand.Create(CancelRequestOrDisconnectWebSocket);
        CyclePreviousEnvironmentToActiveCmd = ReactiveCommand.Create(() => CycleActiveEnvironments(false));
        CycleNextEnvironmentToActiveCmd = ReactiveCommand.Create(() => CycleActiveEnvironments(true));
        SaveResponseToFileCmd = ReactiveCommand.Create(SaveResponseToFile);
    }

    #region COPY AND CUT

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
            if (SelectedItems.Contains((ViewModelBase)parent))
                return true;
            else if (parent is CollectionFolderViewModel parentAsItem)
                parent = parentAsItem.Parent;
            else
                break;
        }
        return false;
    }

    public void CutSelectedItems()
    {
        // IMPORTANT: needs to be a new list, not just a reference
        // IMPORTANT: Collections can not be marked for cut deletion (after pasting), 
        // because they cannot be pasted into other collections
        if (AreAnyCollectionsBeingCopiedOrCut())
        {
            ShowCollectionsCannotBeCopiedOrCutDialog();
            return;
        }

        ClipboardArea.ItemsMarkedForCut = SelectedItems.ToList();

        PushSelectedItemsToClipboardArea();
    }

    public void CopySelectedItems()
    {
        // we must clear items marked for cut if it's just a copy
        if (AreAnyCollectionsBeingCopiedOrCut())
        {
            ShowCollectionsCannotBeCopiedOrCutDialog();
            return;
        }

        ClipboardArea.ItemsMarkedForCut = null;

        PushSelectedItemsToClipboardArea();
    }

    private bool AreAnyCollectionsBeingCopiedOrCut() =>
        SelectedItems.Any(x => x is CollectionViewModel);

    private void ShowCollectionsCannotBeCopiedOrCutDialog()
    {
        Bitmap bitmap = new(AssetLoader.Open(new("avares://Pororoca.Desktop/Assets/Images/pororoca.png")));
        var msgbox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams()
            {
                ContentTitle = Localizer.Instance.CannotCopyOrCutCollectionDialog.Title,
                ContentMessage = Localizer.Instance.CannotCopyOrCutCollectionDialog.Content,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = new(bitmap),
                ButtonDefinitions = ButtonEnum.Ok
            });
        Dispatcher.UIThread.Post(async () => await msgbox.ShowAsync());
    }

    private void PushSelectedItemsToClipboardArea()
    {
        var reqsToCopy = SelectedItems
                         .Where(i => i is HttpRequestViewModel reqVm && !HasAnyParentAlsoSelected(reqVm))
                         .Select(r => (ICloneable)((HttpRequestViewModel)r).ToHttpRequest());
        var wssToCopy = SelectedItems
                         .Where(i => i is WebSocketConnectionViewModel wsVm && !HasAnyParentAlsoSelected(wsVm))
                         .Select(wsVm => (ICloneable)((WebSocketConnectionViewModel)wsVm).ToWebSocketConnection());
        var wsMsgsToCopy = SelectedItems
                           .Where(i => i is WebSocketClientMessageViewModel wsMsgVm && !HasAnyParentAlsoSelected(wsMsgVm))
                           .Select(wsMsgVm => (ICloneable)((WebSocketClientMessageViewModel)wsMsgVm).ToWebSocketClientMessage());
        var foldersToCopy = SelectedItems
                            .Where(i => i is CollectionFolderViewModel folderVm && !HasAnyParentAlsoSelected(folderVm))
                            .Select(f => (ICloneable)((CollectionFolderViewModel)f).ToCollectionFolder());
        var envsToCopy = SelectedItems
                         .Where(i => i is EnvironmentViewModel)
                         .Select(e => (ICloneable)((EnvironmentViewModel)e).ToEnvironment());

        var itemsToCopy = reqsToCopy.Concat(wssToCopy).Concat(wsMsgsToCopy).Concat(foldersToCopy).Concat(envsToCopy).ToArray();

        ClipboardArea.Instance.PushToCopy(itemsToCopy);
    }

    #endregion

    #region PASTE

    public void PasteCopiedItems()
    {
        if (ClipboardArea.ItemsMarkedForCut?.Any(x => x == SelectedItem) == true)
        {
            ShowCannotPasteCutItemToItselfDialog();
            return;
        }

        PasteCopiedItemsToSpecificVm();

        if (ClipboardArea.ItemsMarkedForCut is not null)
        {
            DeleteMultiple(ClipboardArea.ItemsMarkedForCut);
            ClipboardArea.ClearCopiedItems();
            ClipboardArea.ItemsMarkedForCut = null;
        }
    }

    private void PasteCopiedItemsToSpecificVm()
    {
        if (SelectedItem is CollectionViewModel cvm)
        {
            cvm.PasteToThis();
        }
        else if (SelectedItem is EnvironmentsGroupViewModel egvm)
        {
            egvm.PasteToThis();
        }
        else if (SelectedItem is CollectionFolderViewModel cfvm)
        {
            cfvm.PasteToThis();
        }
        else if (SelectedItem is EnvironmentViewModel evm)
        {
            ((EnvironmentsGroupViewModel)evm.Parent).PasteToThis();
        }
        else if (SelectedItem is HttpRequestViewModel hrvm)
        {
            if (hrvm.Parent is CollectionViewModel cvm3)
            {
                cvm3.PasteToThis();
            }
            else if (hrvm.Parent is CollectionFolderViewModel cfvm3)
            {
                cfvm3.PasteToThis();
            }
        }
        else if (SelectedItem is WebSocketConnectionViewModel wscvm)
        {
            if (ClipboardArea.Instance.OnlyHasCopiesOfWebSocketClientMessages)
            {
                wscvm.PasteToThis();
            }
            else if (wscvm.Parent is CollectionViewModel cvm2)
            {
                cvm2.PasteToThis();
            }
            else if (wscvm.Parent is CollectionFolderViewModel cfvm2)
            {
                cfvm2.PasteToThis();
            }
        }
        else if (SelectedItem is WebSocketClientMessageViewModel wscmvm)
        {
            ((WebSocketConnectionViewModel)wscmvm.Parent).PasteToThis();
        }
    }

    private void ShowCannotPasteCutItemToItselfDialog()
    {
        Bitmap bitmap = new(AssetLoader.Open(new("avares://Pororoca.Desktop/Assets/Images/pororoca.png")));
        var msgbox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams()
            {
                ContentTitle = Localizer.Instance.CannotPasteItemToItselfDialog.Title,
                ContentMessage = Localizer.Instance.CannotPasteItemToItselfDialog.Content,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = new(bitmap),
                ButtonDefinitions = ButtonEnum.Ok
            });
        Dispatcher.UIThread.Post(async () => await msgbox.ShowAsync());
    }

    #endregion

    #region DELETE

    private void AskUserToConfirmDeleteItems()
    {
        Bitmap bitmap = new(AssetLoader.Open(new("avares://Pororoca.Desktop/Assets/Images/pororoca.png")));

        string dialogMsg;
        
        if (HasMultipleItemsSelected)
        {
            dialogMsg = Localizer.Instance.DeleteItemsDialog.MessageMultipleItems;
        }
        else
        {
            string itemName = SelectedItem?.Name ?? string.Empty;
            dialogMsg = string.Format(Localizer.Instance.DeleteItemsDialog.MessageSingleItem, itemName);
        }

        var msgbox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams()
            {
                ContentTitle = Localizer.Instance.DeleteItemsDialog.Title,
                ContentMessage = dialogMsg,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = new(bitmap),
                ButtonDefinitions = ButtonEnum.OkCancel
            });
        Dispatcher.UIThread.Post(async () =>
        {
            var buttonResult = await msgbox.ShowAsync();
            if (buttonResult == ButtonResult.Ok)
            {
                DeleteMultiple(SelectedItems);
            }
        });
    }

    private void DeleteMultiple(ICollection<CollectionOrganizationItemViewModel> itemsToDelete) =>
        itemsToDelete
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

    #region DUPLICATE

    private void DuplicateSelectedItem()
    {
        if (SelectedItem is CollectionViewModel cvm)
        {
            ((MainWindowViewModel)cvm.Parent).DuplicateCollection(cvm);
        }
    }

    #endregion
    
    #region MOVE UP AND DOWN

    private void MoveSelectedItemUp() => SelectedItem?.MoveThisUp();

    private void MoveSelectedItemDown() => SelectedItem?.MoveThisDown();

    #endregion

    #region SWITCH ITEMS PREVIOUS AND NEXT

    private void SwitchToPreviousItem() => SwitchSelectedItem(false);

    private void SwitchToNextItem() => SwitchSelectedItem(true);

    private void SwitchSelectedItem(bool falseIfPreviousTrueIfNext)
    {
        var linearizedItems = GetItemsTreeLinearized();
        if (SelectedItem is not null)
        {
            int currentIndex = linearizedItems.IndexOf(SelectedItem);
            if (currentIndex != -1)
            {
                int indexToSwitchTo;
                if (falseIfPreviousTrueIfNext)
                {
                    indexToSwitchTo = currentIndex < (linearizedItems.Count - 1) ? (currentIndex + 1) : (linearizedItems.Count - 1);
                }
                else
                {
                    indexToSwitchTo = currentIndex > 0 ? (currentIndex - 1) : 0;
                }
                var itemToSwitchTo = linearizedItems[indexToSwitchTo];
                CollectionsGroupVm.CollectionGroupSelectedItem = itemToSwitchTo;
                ExpandAncestorsOfItem(itemToSwitchTo);
            }
        }
    }

    private void ExpandAncestorsOfItem(CollectionOrganizationItemViewModel itemVm)
    {
        var ancestor = itemVm.Parent;
        do
        {
            if (ancestor is CollectionOrganizationItemParentViewModel<CollectionOrganizationItemViewModel> a)
            {
                a.IsExpanded = true;
            }
            else if (ancestor is EnvironmentsGroupViewModel b)
            {
                b.IsExpanded = true;
            }
            else if (ancestor is WebSocketConnectionViewModel c)
            {
                c.IsExpanded = true;
            }

            if (ancestor is CollectionOrganizationItemViewModel x)
            {
                ancestor = x.Parent;
            }
        } while (ancestor is not MainWindowViewModel);
    }

    #endregion

    #region DELETE

    private void ShowHelpDialog()
    {
        Bitmap bitmap = new(AssetLoader.Open(new("avares://Pororoca.Desktop/Assets/Images/pororoca.png")));

        var msgbox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams()
            {
                ContentTitle = Localizer.Instance.HelpDialog.Title,
                ContentMessage = Localizer.Instance.HelpDialog.Content,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = new(bitmap),
                ButtonDefinitions = ButtonEnum.Ok
            });
        Dispatcher.UIThread.Post(async () => await msgbox.ShowAsync());
    }

    #endregion

    #region RENAME

    private void RenameSelectedItem() => SelectedItem?.RenameThis();

    #endregion

    #region FOCUS ON URL

    internal void FocusOnUrl()
    {
        var mwvm = MainWindowVm;
        TextBox? tbUrl = null;
        if (mwvm.IsHttpRequestViewVisible)
        {
            tbUrl = MainWindow.Instance!.FindControl<HttpRequestView>("httpReqView")?.FindControl<TextBox>("tbUrl");
        }
        else if (mwvm.IsWebSocketConnectionViewVisible)
        {
            tbUrl = MainWindow.Instance!.FindControl<WebSocketConnectionView>("wsConnView")?.FindControl<TextBox>("tbUrl");
        }

        if (tbUrl is not null)
        {
            tbUrl.Focus();
            tbUrl.CaretIndex = tbUrl.Text is null ? 0 : tbUrl.Text.Length;
        }
    }

    #endregion

    #region SEND REQUEST OR CONNECT WEBSOCKET

    internal void SendRequestOrConnectWebSocket()
    {
        var mwvm = MainWindowVm;
        if (mwvm.IsHttpRequestViewVisible && mwvm.HttpRequestViewDataCtx?.IsRequesting == false)
        {
            // This needs to be done via Dispatcher.UIThread.Post() instead of Task.Run
            // because it happens on UI thread. Task.Run also causes a weird bug 
            // that CancellationTokenSource becomes null, even after sending the request.
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRequestViewDataCtx.SendRequestAsync());
        }
        else if (mwvm.IsWebSocketConnectionViewVisible
              && mwvm.WebSocketConnectionViewDataCtx?.IsConnected == false
              && mwvm.WebSocketConnectionViewDataCtx?.IsConnecting == false)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.WebSocketConnectionViewDataCtx.ConnectAsync());
        }
    }

    #endregion

    #region CANCEL REQUEST OR DISCONNECT WEBSOCKET

    internal void CancelRequestOrDisconnectWebSocket()
    {
        var mwvm = MainWindowVm;
        if (mwvm.IsHttpRequestViewVisible && mwvm.HttpRequestViewDataCtx?.IsRequesting == true)
        {
            mwvm.HttpRequestViewDataCtx.CancelRequest();
        }
        else if (mwvm.IsWebSocketConnectionViewVisible)
        {
            if (mwvm.WebSocketConnectionViewDataCtx?.IsConnected == true)
            {
                Dispatcher.UIThread.Post(async () => await mwvm.WebSocketConnectionViewDataCtx.DisconnectAsync());
            }
            else if (mwvm.WebSocketConnectionViewDataCtx?.IsConnecting == true)
            {
                mwvm.WebSocketConnectionViewDataCtx.CancelConnect();
            }
        }
    }

    #endregion

    #region CYCLE ENVIRONMENTS

    internal void CycleActiveEnvironments(bool trueIfNextFalseIfPrevious)
    {
        var x = SelectedItem;
        while (x is not CollectionViewModel)
        {
            if (x?.Parent is CollectionOrganizationItemViewModel coipvm)
            {
                x = coipvm;
            }
            else
            {
                return;
            }
        }
        var cvm = (CollectionViewModel)x;
        var egvm = (EnvironmentsGroupViewModel)cvm.Items.First(y => y is EnvironmentsGroupViewModel);
        egvm.CycleActiveEnvironment(trueIfNextFalseIfPrevious);
    }

    #endregion

    #region SAVE RESPONSE TO FILE

    internal void SaveResponseToFile()
    {
        var mwvm = MainWindowVm;
        if (mwvm.IsHttpRequestViewVisible && mwvm.HttpRequestViewDataCtx?.ResponseDataCtx?.IsSaveResponseBodyToFileVisible == true)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRequestViewDataCtx.ResponseDataCtx.SaveResponseBodyToFileAsync());
        }
        else if (mwvm.IsWebSocketConnectionViewVisible && mwvm.WebSocketConnectionViewDataCtx?.IsSaveSelectedExchangedMessageToFileVisible == true)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.WebSocketConnectionViewDataCtx.SaveSelectedExchangedMessageToFileAsync());
        }
    }

    #endregion
}