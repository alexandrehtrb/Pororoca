using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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
    public ReactiveCommand<Unit, Unit> ExportHttpLogToFileCmd { get; }
    public ReactiveCommand<Unit, Unit> FocusOnUrlCmd { get; }

    #region HELPER PROPERTIES

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
                linearizedItems.AddRange(
                    a.Items
                    .SelectMany(i =>
                    {
                        if (i is HttpRequestViewModel hrvm)
                            return [hrvm];
                        else if (i is HttpRepeaterViewModel hrvm2)
                            return [hrvm2];
                        else if (i is EnvironmentsGroupViewModel egvm)
                            return egvm.Items.Cast<CollectionOrganizationItemViewModel>().ToList();
                        else
                            return GetSubItemsLinearized(i);
                    }));
            }
            else if (parentVm is CollectionFolderViewModel e)
            {
                linearizedItems.Add(e);
                if (e.Items.All(x => x is HttpRequestViewModel || x is HttpRepeaterViewModel))
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
        SendRequestOrConnectWebSocketCmd = ReactiveCommand.Create(SendRequestConnectWebSocketOrStartRepetition);
        CancelRequestOrDisconnectWebSocketCmd = ReactiveCommand.Create(CancelRequestDisconnectWebSocketOrStopRepetition);
        CyclePreviousEnvironmentToActiveCmd = ReactiveCommand.Create(() => CycleActiveEnvironments(false));
        CycleNextEnvironmentToActiveCmd = ReactiveCommand.Create(() => CycleActiveEnvironments(true));
        SaveResponseToFileCmd = ReactiveCommand.Create(SaveResponseToFile);
        ExportHttpLogToFileCmd = ReactiveCommand.Create(ExportHttpLogToFile);
    }

    #region COPY AND CUT

    private bool HasAnyParentAlsoSelected(CollectionOrganizationItemViewModel itemVm)
    {
        if (itemVm is not HttpRequestViewModel
         && itemVm is not HttpRepeaterViewModel
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

    private void ShowCollectionsCannotBeCopiedOrCutDialog() =>
        Dialogs.ShowDialog(
            title: Localizer.Instance.CannotCopyOrCutCollectionDialog.Title,
            message: Localizer.Instance.CannotCopyOrCutCollectionDialog.Content,
            buttons: ButtonEnum.Ok);

    private void PushSelectedItemsToClipboardArea()
    {
        var reqsToCopy = SelectedItems
                         .Where(i => i is HttpRequestViewModel reqVm && !HasAnyParentAlsoSelected(reqVm))
                         .Select(r => (object)((HttpRequestViewModel)r).ToHttpRequest());
        var repsToCopy = SelectedItems
                         .Where(i => i is HttpRepeaterViewModel repVm && !HasAnyParentAlsoSelected(repVm))
                         .Select(r => (object)((HttpRepeaterViewModel)r).ToHttpRepetition());
        var wssToCopy = SelectedItems
                         .Where(i => i is WebSocketConnectionViewModel wsVm && !HasAnyParentAlsoSelected(wsVm))
                         .Select(wsVm => (object)((WebSocketConnectionViewModel)wsVm).ToWebSocketConnection());
        var wsMsgsToCopy = SelectedItems
                           .Where(i => i is WebSocketClientMessageViewModel wsMsgVm && !HasAnyParentAlsoSelected(wsMsgVm))
                           .Select(wsMsgVm => (object)((WebSocketClientMessageViewModel)wsMsgVm).ToWebSocketClientMessage());
        var foldersToCopy = SelectedItems
                            .Where(i => i is CollectionFolderViewModel folderVm && !HasAnyParentAlsoSelected(folderVm))
                            .Select(f => (object)((CollectionFolderViewModel)f).ToCollectionFolder());
        var envsToCopy = SelectedItems
                         .Where(i => i is EnvironmentViewModel)
                         .Select(e => (object)((EnvironmentViewModel)e).ToEnvironment());

        object[] itemsToCopy = reqsToCopy.Concat(repsToCopy).Concat(wssToCopy).Concat(wsMsgsToCopy).Concat(foldersToCopy).Concat(envsToCopy).ToArray();

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
        if (SelectedItem is RequestsAndFoldersParentViewModel rafpvm)
        {
            rafpvm.PasteToThis();
        }
        else if (SelectedItem is EnvironmentsGroupViewModel egvm)
        {
            egvm.PasteToThis();
        }
        else if (SelectedItem is EnvironmentViewModel evm)
        {
            ((EnvironmentsGroupViewModel)evm.Parent).PasteToThis();
        }
        else if (SelectedItem is HttpRequestViewModel hrvm && hrvm.Parent is RequestsAndFoldersParentViewModel rafpvm2)
        {
            rafpvm2.PasteToThis();
        }
        else if (SelectedItem is HttpRepeaterViewModel hrvm2 && hrvm2.Parent is RequestsAndFoldersParentViewModel rafpvm3)
        {
            rafpvm3.PasteToThis();
        }
        else if (SelectedItem is WebSocketConnectionViewModel wscvm)
        {
            if (ClipboardArea.Instance.OnlyHasCopiesOfWebSocketClientMessages)
            {
                wscvm.PasteToThis();
            }
            else if (wscvm.Parent is RequestsAndFoldersParentViewModel rafpvm4)
            {
                rafpvm4.PasteToThis();
            }
        }
        else if (SelectedItem is WebSocketClientMessageViewModel wscmvm)
        {
            ((WebSocketConnectionViewModel)wscmvm.Parent).PasteToThis();
        }
    }

    private void ShowCannotPasteCutItemToItselfDialog() =>
        Dialogs.ShowDialog(
            title: Localizer.Instance.CannotPasteItemToItselfDialog.Title,
            message: Localizer.Instance.CannotPasteItemToItselfDialog.Content,
            buttons: ButtonEnum.Ok);

    #endregion

    #region DELETE

    private void AskUserToConfirmDeleteItems()
    {
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

        Dialogs.ShowDialog(
            title: Localizer.Instance.DeleteItemsDialog.Title,
            message: dialogMsg,
            buttons: ButtonEnum.OkCancel,
            onButtonOkClicked: DeleteSelectedItems);
    }

    internal void DeleteSelectedItems() =>
        DeleteMultiple(SelectedItems);

    private void DeleteMultiple(ICollection<CollectionOrganizationItemViewModel> itemsToDelete) =>
        itemsToDelete
        .Where(i => i is RequestsAndFoldersParentViewModel
                 || i is EnvironmentViewModel
                 || i is HttpRequestViewModel
                 || i is HttpRepeaterViewModel
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
            MainWindowVm.DuplicateCollection(cvm);
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

    private void ShowHelpDialog() =>
        Dialogs.ShowDialog(
            title: Localizer.Instance.HelpDialog.Title,
            message: Localizer.Instance.HelpDialog.Content,
            buttons: ButtonEnum.Ok);

    #endregion

    #region RENAME

    private void RenameSelectedItem() => SelectedItem?.RenameThis();

    #endregion

    #region FOCUS ON URL

    internal void FocusOnUrl()
    {
        var mwvm = MainWindowVm;
        TextBox? tbUrl = null;
        if (mwvm.HttpRequestView.Visible)
        {
            tbUrl = MainWindow.Instance!.FindControl<HttpRequestView>("httpReqView")?.FindControl<TextBox>("tbUrl");
        }
        else if (mwvm.WebSocketConnectionView.Visible)
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

    #region SEND REQUEST, CONNECT WEBSOCKET OR START REPETITION

    internal void SendRequestConnectWebSocketOrStartRepetition()
    {
        var mwvm = MainWindowVm;
        if (mwvm.HttpRequestView.Visible && mwvm.HttpRequestView.VM?.IsRequesting == false)
        {
            // This needs to be done via Dispatcher.UIThread.Post() instead of Task.Run
            // because it happens on UI thread. Task.Run also causes a weird bug
            // that CancellationTokenSource becomes null, even after sending the request.
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRequestView.VM.SendRequestAsync());
        }
        else if (mwvm.HttpRepeaterView.Visible && mwvm.HttpRepeaterView.VM?.IsRepetitionRunning == false)
        {
            // This needs to be done via Dispatcher.UIThread.Post() instead of Task.Run
            // because it happens on UI thread. Task.Run also causes a weird bug
            // that CancellationTokenSource becomes null, even after sending the request.
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRepeaterView.VM.StartRepetitionAsync());
        }
        else if (mwvm.WebSocketConnectionView.Visible
              && mwvm.WebSocketConnectionView.VM?.ConnectionState == PororocaWebSocketConnectorState.Disconnected)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.WebSocketConnectionView.VM.ConnectAsync());
        }
    }

    #endregion

    #region CANCEL REQUEST, DISCONNECT WEBSOCKET OR STOP REPETITION

    internal void CancelRequestDisconnectWebSocketOrStopRepetition()
    {
        var mwvm = MainWindowVm;
        if (mwvm.HttpRequestView.Visible && mwvm.HttpRequestView.VM?.IsRequesting == true)
        {
            mwvm.HttpRequestView.VM.CancelRequest();
        }
        else if (mwvm.HttpRepeaterView.Visible && mwvm.HttpRepeaterView.VM?.IsRepetitionRunning == true)
        {
            mwvm.HttpRepeaterView.VM.StopRepetition();
        }
        else if (mwvm.WebSocketConnectionView.Visible)
        {
            if (mwvm.WebSocketConnectionView.VM?.ConnectionState == PororocaWebSocketConnectorState.Connected)
            {
                Dispatcher.UIThread.Post(async () => await mwvm.WebSocketConnectionView.VM.DisconnectAsync());
            }
            else if (mwvm.WebSocketConnectionView.VM?.ConnectionState == PororocaWebSocketConnectorState.Connecting)
            {
                mwvm.WebSocketConnectionView.VM.CancelConnect();
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
        var egvm = ((CollectionViewModel)x).EnvironmentsGroupVm;
        egvm.CycleActiveEnvironment(trueIfNextFalseIfPrevious);
    }

    #endregion

    #region SAVE RESPONSE TO FILE

    internal void SaveResponseToFile()
    {
        var mwvm = MainWindowVm;
        if (mwvm.HttpRequestView.Visible && mwvm.HttpRequestView.VM?.ResponseDataCtx?.IsSaveResponseBodyToFileVisible == true)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRequestView.VM.ResponseDataCtx.SaveResponseBodyToFileAsync());
        }
        else if (mwvm.HttpRepeaterView.Visible && mwvm.HttpRepeaterView.VM?.ResponseDataCtx?.IsSaveResponseBodyToFileVisible == true)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRepeaterView.VM.ResponseDataCtx.SaveResponseBodyToFileAsync());
        }
        else if (mwvm.WebSocketConnectionView.Visible && mwvm.WebSocketConnectionView.VM?.IsSaveSelectedExchangedMessageToFileVisible == true)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.WebSocketConnectionView.VM.SaveSelectedExchangedMessageToFileAsync());
        }
    }

    internal void ExportHttpLogToFile()
    {
        var mwvm = MainWindowVm;
        if (mwvm.HttpRequestView.Visible && mwvm.HttpRequestView.VM?.ResponseDataCtx?.IsExportLogFileVisible == true)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRequestView.VM.ResponseDataCtx.ExportLogToFileAsync());
        }
        else if (mwvm.HttpRepeaterView.Visible && mwvm.HttpRepeaterView.VM?.ResponseDataCtx?.IsExportLogFileVisible == true)
        {
            Dispatcher.UIThread.Post(async () => await mwvm.HttpRepeaterView.VM.ResponseDataCtx.ExportLogToFileAsync());
        }
    }

    #endregion
}