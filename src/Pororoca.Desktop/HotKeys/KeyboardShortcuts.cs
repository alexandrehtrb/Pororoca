using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Threading;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;
using ReactiveUI;
using Avalonia.Platform;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Pororoca.Desktop.Localization;
using Avalonia.Media.Imaging;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.HotKeys;

public sealed class KeyboardShortcuts : ViewModelBase
{
    internal static readonly KeyboardShortcuts Instance = new();

    [Reactive]
    public bool HasMultipleItemsSelected { get; set; }

    public ReactiveCommand<Unit, Unit> CopyCmd { get; }
    public ReactiveCommand<Unit, Unit> PasteCmd { get; }
    public ReactiveCommand<Unit, Unit> DeleteCmd { get; }
    public ReactiveCommand<Unit, Unit> DuplicateCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveUpCmd { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCmd { get; }
    public ReactiveCommand<Unit, Unit> RenameCmd { get; }
    public ReactiveCommand<Unit, Unit> SendRequestOrConnectWebSocketCmd { get; }
    public ReactiveCommand<Unit, Unit> CancelRequestOrDisconnectWebSocketCmd { get; }
    public ReactiveCommand<Unit, Unit> CycleNextEnvironmentToActiveCmd { get; }
    public ReactiveCommand<Unit, Unit> SaveResponseToFileCmd { get; }
    public ReactiveCommand<Unit, Unit> FocusOnUrlCmd { get; }

    #region HELPER PROPERTIES

    private MainWindowViewModel MainWindowVm => 
        ((MainWindowViewModel)MainWindow.Instance!.DataContext!);

    private CollectionsGroupViewModel CollectionsGroupVm => 
        MainWindowVm.CollectionsGroupViewDataCtx;

    private ViewModelBase? SelectedItem =>
        CollectionsGroupVm.CollectionGroupSelectedItem;

    private ObservableCollection<ViewModelBase> SelectedItems =>
        CollectionsGroupVm.CollectionGroupSelectedItems;

    #endregion

    private KeyboardShortcuts()
    {
        CopyCmd = ReactiveCommand.Create(CopySelectedItems);
        PasteCmd = ReactiveCommand.Create(PasteCopiedItems);
        DeleteCmd = ReactiveCommand.Create(AskUserToConfirmDeleteItems);
        DuplicateCmd = ReactiveCommand.Create(DuplicateSelectedItem);
        // Moving items up and down is not great yet
        MoveUpCmd = ReactiveCommand.Create(MoveSelectedItemUp);
        MoveDownCmd = ReactiveCommand.Create(MoveSelectedItemDown);
        RenameCmd = ReactiveCommand.Create(RenameSelectedItem);
        FocusOnUrlCmd = ReactiveCommand.Create(FocusOnUrl);
        SendRequestOrConnectWebSocketCmd = ReactiveCommand.Create(SendRequestOrConnectWebSocket);
        CancelRequestOrDisconnectWebSocketCmd = ReactiveCommand.Create(CancelRequestOrDisconnectWebSocket);
        CycleNextEnvironmentToActiveCmd = ReactiveCommand.Create(CycleNextEnvironmentToActive);
        SaveResponseToFileCmd = ReactiveCommand.Create(SaveResponseToFile);
    }

    #region COPY

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

    private void CopySelectedItems()
    {
        var reqsToCopy = SelectedItems
                         .Where(i => i is HttpRequestViewModel reqVm && !HasAnyParentAlsoSelected(reqVm))
                         .Select(r => (ICloneable)((HttpRequestViewModel)r).ToHttpRequest());
        var wssToCopy = SelectedItems
                         .Where(i => i is WebSocketConnectionViewModel wsVm && !HasAnyParentAlsoSelected(wsVm))
                         .Select(wsVm => (ICloneable)((WebSocketConnectionViewModel)wsVm).ToWebSocketConnection());
        var foldersToCopy = SelectedItems
                            .Where(i => i is CollectionFolderViewModel folderVm && !HasAnyParentAlsoSelected(folderVm))
                            .Select(f => (ICloneable)((CollectionFolderViewModel)f).ToCollectionFolder());
        var envsToCopy = SelectedItems
                         .Where(i => i is EnvironmentViewModel)
                         .Select(e => (ICloneable)((EnvironmentViewModel)e).ToEnvironment());

        var itemsToCopy = reqsToCopy.Concat(wssToCopy).Concat(foldersToCopy).Concat(envsToCopy).ToArray();

        ClipboardArea.Instance.PushToCopy(itemsToCopy);
    }

    #endregion

    #region PASTE

    private void PasteCopiedItems()
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
            string itemName;
            if (SelectedItem is CollectionOrganizationItemViewModel coivm)
            {
                itemName = coivm.Name;
            }
            else
            {
                itemName = string.Empty;
            }
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
                DeleteMultiple();
            }
        });
    }

    private void DeleteMultiple() =>
        SelectedItems
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

    private void MoveSelectedItemUp()
    {
        if (SelectedItem is CollectionOrganizationItemViewModel coivm)
            coivm.MoveThisUp();
    }

    private void MoveSelectedItemDown()
    {
        if (SelectedItem is CollectionOrganizationItemViewModel coivm)
            coivm.MoveThisDown();
    }

    #endregion

    #region RENAME

    private void RenameSelectedItem()
    {
        if (SelectedItem is CollectionOrganizationItemViewModel coivm)
            coivm.RenameThis();
    }

    #endregion

    #region FOCUS ON URL

    internal void FocusOnUrl()
    {
        var mwvm = MainWindowVm;
        if (mwvm.IsHttpRequestViewVisible)
        {
            var tbUrl = MainWindow.Instance!.FindControl<HttpRequestView>("httpReqView")?.FindControl<TextBox>("tbUrl");
            tbUrl?.Focus();
        }
        else if (mwvm.IsWebSocketConnectionViewVisible)
        {
            var tbUrl = MainWindow.Instance!.FindControl<HttpRequestView>("wsConnView")?.FindControl<TextBox>("tbUrl");
            tbUrl?.Focus();
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

    internal void CycleNextEnvironmentToActive()
    {
        if (SelectedItem is CollectionOrganizationItemViewModel coivm)
        {
            var x = coivm;
            while (coivm is not CollectionViewModel)
            {
                if (coivm.Parent is CollectionOrganizationItemViewModel coipvm)
                {
                    coivm = coipvm;
                }
                else
                {
                    return;
                }
            }
            var cvm = (CollectionViewModel)coivm;
            var egvm = (EnvironmentsGroupViewModel)cvm.Items.First(y => y is EnvironmentsGroupViewModel);
            egvm.SetNextEnvironmentAsActive();
        }
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