using System.Diagnostics;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, ICollectionOrganizationItemParentViewModel
{
    #region COLLECTIONS ORGANIZATION

    [Reactive]
    public CollectionsGroupViewModel CollectionsGroupViewDataCtx { get; set; }

    public Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => onRenameItemSelected;

    Action ICollectionOrganizationItemParentViewModel.OnAfterItemDeleted => onAfterItemDeleted;

    public ReactiveCommand<Unit, Unit> AddNewCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ImportCollectionsCmd { get; }

    #endregion

    #region SCREENS

    [Reactive]
    public bool IsCollectionViewVisible { get; set; }

    [Reactive]
    public CollectionViewModel? CollectionViewDataCtx { get; set; }

    [Reactive]
    public bool IsCollectionVariablesViewVisible { get; set; }

    [Reactive]
    public CollectionVariablesViewModel? CollectionVariablesViewDataCtx { get; set; }

    [Reactive]
    public bool IsEnvironmentViewVisible { get; set; }

    [Reactive]
    public EnvironmentViewModel? EnvironmentViewDataCtx { get; set; }

    [Reactive]
    public bool IsCollectionFolderViewVisible { get; set; }

    [Reactive]
    public CollectionFolderViewModel? CollectionFolderViewDataCtx { get; set; }

    [Reactive]
    public bool IsHttpRequestViewVisible { get; set; }

    [Reactive]
    public HttpRequestViewModel? HttpRequestViewDataCtx { get; set; }

    [Reactive]
    public bool IsWebSocketConnectionViewVisible { get; set; }

    [Reactive]
    public WebSocketConnectionViewModel? WebSocketConnectionViewDataCtx { get; set; }

    [Reactive]
    public bool IsWebSocketClientMessageViewVisible { get; set; }

    [Reactive]
    public WebSocketClientMessageViewModel? WebSocketClientMessageViewDataCtx { get; set; }

    #endregion

    #region LANGUAGE

    [Reactive]
    public bool IsLanguagePortuguese { get; set; }

    [Reactive]
    public bool IsLanguageEnglish { get; set; }

    public ReactiveCommand<Unit, Unit> SelectLanguagePortuguesCmd { get; }
    public ReactiveCommand<Unit, Unit> SelectLanguageEnglishCmd { get; }

    #endregion

    #region GLOBAL OPTIONS

    [Reactive]
    public bool IsSslVerificationDisabled { get; set; }

    public ReactiveCommand<Unit, Unit> ToggleSSLVerificationCmd { get; }

    #endregion

    #region OTHERS

    private readonly bool isOperatingSystemMacOsx;

    #endregion

    #region USER PREFERENCES

    private UserPreferences? UserPrefs { get; set; }

    #endregion

    public MainWindowViewModel(Func<bool>? isOperatingSystemMacOsx = null)
    {
        #region OTHERS
        this.isOperatingSystemMacOsx = (isOperatingSystemMacOsx ?? OperatingSystem.IsMacOS)();
        #endregion

        #region COLLECTIONS ORGANIZATION
        CollectionsGroupViewDataCtx = new(this, OnCollectionsGroupItemSelected);
        ImportCollectionsCmd = ReactiveCommand.CreateFromTask(ImportCollectionsAsync);
        AddNewCollectionCmd = ReactiveCommand.Create(AddNewCollection);
        #endregion

        #region LANGUAGE
        SelectLanguagePortuguesCmd = ReactiveCommand.Create(() => SelectLanguage(Language.PtBr));
        SelectLanguageEnglishCmd = ReactiveCommand.Create(() => SelectLanguage(Language.EnGb));
        #endregion

        #region GLOBAL OPTIONS
        ToggleSSLVerificationCmd = ReactiveCommand.Create(ToggleSslVerification);
        #endregion

        #region USER DATA
        LoadUserData();
        #endregion
    }

    #region SCREENS

    private void OnCollectionsGroupItemSelected(ViewModelBase? selectedItem)
    {
        if (selectedItem is CollectionViewModel colVm)
        {
            CollectionViewDataCtx = colVm;
            if (!IsCollectionViewVisible)
            {
                // TODO: Use a tuple with (enum, ref bool variable) instead of many bools
                IsCollectionViewVisible = true;
                IsCollectionVariablesViewVisible = false;
                IsEnvironmentViewVisible = false;
                IsCollectionFolderViewVisible = false;
                IsHttpRequestViewVisible = false;
                IsWebSocketConnectionViewVisible = false;
                IsWebSocketClientMessageViewVisible = false;
            }
        }
        else if (selectedItem is CollectionVariablesViewModel colVarsVm)
        {
            CollectionVariablesViewDataCtx = colVarsVm;
            if (!IsCollectionVariablesViewVisible)
            {
                IsCollectionViewVisible = false;
                IsCollectionVariablesViewVisible = true;
                IsEnvironmentViewVisible = false;
                IsCollectionFolderViewVisible = false;
                IsHttpRequestViewVisible = false;
                IsWebSocketConnectionViewVisible = false;
                IsWebSocketClientMessageViewVisible = false;
            }
        }
        else if (selectedItem is EnvironmentViewModel envVm)
        {
            EnvironmentViewDataCtx = envVm;
            if (!IsEnvironmentViewVisible)
            {
                IsCollectionViewVisible = false;
                IsCollectionVariablesViewVisible = false;
                IsEnvironmentViewVisible = true;
                IsCollectionFolderViewVisible = false;
                IsHttpRequestViewVisible = false;
                IsWebSocketConnectionViewVisible = false;
                IsWebSocketClientMessageViewVisible = false;
            }
        }
        else if (selectedItem is CollectionFolderViewModel folderVm)
        {
            CollectionFolderViewDataCtx = folderVm;
            if (!IsCollectionFolderViewVisible)
            {
                IsCollectionViewVisible = false;
                IsCollectionVariablesViewVisible = false;
                IsEnvironmentViewVisible = false;
                IsCollectionFolderViewVisible = true;
                IsHttpRequestViewVisible = false;
                IsWebSocketConnectionViewVisible = false;
                IsWebSocketClientMessageViewVisible = false;
            }
        }
        else if (selectedItem is HttpRequestViewModel reqVm)
        {
            HttpRequestViewDataCtx = reqVm;
            if (!IsHttpRequestViewVisible)
            {
                IsCollectionViewVisible = false;
                IsCollectionVariablesViewVisible = false;
                IsEnvironmentViewVisible = false;
                IsCollectionFolderViewVisible = false;
                IsHttpRequestViewVisible = true;
                IsWebSocketConnectionViewVisible = false;
                IsWebSocketClientMessageViewVisible = false;
            }
        }
        else if (selectedItem is WebSocketConnectionViewModel wsVm)
        {
            WebSocketConnectionViewDataCtx = wsVm;
            if (!IsWebSocketConnectionViewVisible)
            {
                IsCollectionViewVisible = false;
                IsCollectionVariablesViewVisible = false;
                IsEnvironmentViewVisible = false;
                IsCollectionFolderViewVisible = false;
                IsHttpRequestViewVisible = false;
                IsWebSocketConnectionViewVisible = true;
                IsWebSocketClientMessageViewVisible = false;
            }
        }
        else if (selectedItem is WebSocketClientMessageViewModel wsReqVm)
        {
            WebSocketClientMessageViewDataCtx = wsReqVm;
            if (!IsWebSocketClientMessageViewVisible)
            {
                IsCollectionViewVisible = false;
                IsCollectionVariablesViewVisible = false;
                IsEnvironmentViewVisible = false;
                IsCollectionFolderViewVisible = false;
                IsHttpRequestViewVisible = false;
                IsWebSocketConnectionViewVisible = false;
                IsWebSocketClientMessageViewVisible = true;
            }
        }
    }

    #endregion

    #region COLLECTIONS ORGANIZATION

    public void MoveSubItem(ICollectionOrganizationItemViewModel colItemVm, MoveableItemMovementDirection direction) =>
        CollectionsGroupViewDataCtx.MoveSubItem(colItemVm, direction);

    private void AddNewCollection()
    {
        PororocaCollection newCol = new(Localizer.Instance["Collection/NewCollection"]);
        AddCollection(newCol, showItemInScreen: true);
    }

    internal void AddCollection(PororocaCollection col, bool showItemInScreen = false)
    {
        CollectionViewModel colVm = new(this, col, DuplicateCollection);
        CollectionsGroupViewDataCtx.Items.Add(colVm);
        CollectionsGroupViewDataCtx.RefreshSubItemsAvailableMovements();
        if (showItemInScreen)
        {
            CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = colVm;
        }
    }

    private void DuplicateCollection(CollectionViewModel colVm)
    {
        var collectionCopy = (PororocaCollection)colVm.ToCollection().Clone();
        AddCollection(collectionCopy);
    }

    private void onRenameItemSelected(ViewModelBase vm) =>
        OnCollectionsGroupItemSelected(vm);

    public void DeleteSubItem(ICollectionOrganizationItemViewModel colVm)
    {
        CollectionsGroupViewDataCtx.Items.Remove((CollectionViewModel)colVm);
        CollectionsGroupViewDataCtx.RefreshSubItemsAvailableMovements();
        onAfterItemDeleted();
    }

    private void onAfterItemDeleted()
    {
        IsCollectionViewVisible = false;
        IsCollectionVariablesViewVisible = false;
        IsEnvironmentViewVisible = false;
        IsCollectionFolderViewVisible = false;
        IsHttpRequestViewVisible = false;
        IsWebSocketConnectionViewVisible = false;
        IsWebSocketClientMessageViewVisible = false;
    }

    private Task ImportCollectionsAsync() =>
        FileExporterImporter.ImportCollectionsAsync(this);

    #endregion

    #region LANGUAGE

    private void SelectLanguage(Language lang)
    {
        Localizer.Instance.LoadLanguage(lang);
        switch (lang)
        {
            case Language.PtBr:
                IsLanguagePortuguese = true;
                IsLanguageEnglish = false;
                break;
            default:
            case Language.EnGb:
                IsLanguagePortuguese = false;
                IsLanguageEnglish = true;
                break;
        }
    }

    #endregion

    #region GLOBAL OPTIONS

    private void ToggleSslVerification() =>
        IsSslVerificationDisabled = !IsSslVerificationDisabled;

    #endregion

    #region USER DATA

    private static UserPreferences GetDefaultUserPrefs() =>
        new(Language.EnGb, DateTime.Now);

    private void LoadUserData()
    {
        UserPrefs = UserDataManager.LoadUserPreferences() ?? GetDefaultUserPrefs();
        var cols = UserDataManager.LoadUserCollections();

        SelectLanguage(UserPrefs.GetLanguage());
        if (UserPrefs.NeedsToShowUpdateReminder())
        {
            ShowUpdateReminder();
            UserPrefs.SetUpdateReminderLastShownDateAsToday();
        }
        else if (UserPrefs.HasUpdateReminderLastShownDate() == false)
        {
            UserPrefs.SetUpdateReminderLastShownDateAsToday();
        }

        foreach (var col in cols)
        {
            AddCollection(col);
        }
    }

    public void SaveUserData()
    {
        UserPrefs!.SetLanguage(Localizer.Instance.Language);

        var cols = CollectionsGroupViewDataCtx
                                    .Items
                                    .Select(cvm => cvm.ToCollection())
                                    .DistinctBy(c => c.Id)
                                    .ToArray();

        UserDataManager.SaveUserData(UserPrefs, cols).GetAwaiter().GetResult();
    }

    private void ShowUpdateReminder()
    {
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        Bitmap bitmap = new(assets!.Open(new("avares://Pororoca.Desktop/Assets/Images/pororoca.png")));

        var msgbox = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams()
            {
                ContentTitle = Localizer.Instance["UpdateReminder/DialogTitle"],
                ContentMessage = Localizer.Instance["UpdateReminder/DialogMessage"],
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = new(bitmap),
                ButtonDefinitions = ButtonEnum.OkCancel
            });
        Dispatcher.UIThread.Post(async () =>
        {
            var buttonResult = await msgbox.ShowDialog(MainWindow.Instance!);
            if (buttonResult == ButtonResult.Ok)
            {
                OpenPororocaSiteInWebBrowser();
            }
        });
    }

    private static void OpenPororocaSiteInWebBrowser()
    {
        const string url = "https://github.com/alexandrehtrb/Pororoca";
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", url);
            }
        }
        catch (Exception)
        {
            // Process.Start can throw several errors (not all of them documented),
            // just ignore all of them.
        }
    }

    #endregion
}