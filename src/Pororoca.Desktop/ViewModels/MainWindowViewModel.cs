using System.Diagnostics;
using System.Reactive;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;

namespace Pororoca.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, ICollectionOrganizationItemParentViewModel
{
    #region COLLECTIONS ORGANIZATION

    private CollectionsGroupViewModel collectionsGroupViewDataCtxField;
    public CollectionsGroupViewModel CollectionsGroupViewDataCtx
    {
        get => this.collectionsGroupViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionsGroupViewDataCtxField, value);
    }

    public Action<CollectionOrganizationItemViewModel> OnRenameSubItemSelected => onRenameItemSelected;

    Action ICollectionOrganizationItemParentViewModel.OnAfterItemDeleted => onAfterItemDeleted;

    public ReactiveCommand<Unit, Unit> AddNewCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ImportCollectionsCmd { get; }

    #endregion

    #region SCREENS

    private bool isCollectionViewVisibleField = false;
    public bool IsCollectionViewVisible
    {
        get => this.isCollectionViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isCollectionViewVisibleField, value);
    }
    private CollectionViewModel? collectionViewDataCtxField = null;
    public CollectionViewModel? CollectionViewDataCtx
    {
        get => this.collectionViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionViewDataCtxField, value);
    }

    private bool isCollectionVariablesViewVisibleField = false;
    public bool IsCollectionVariablesViewVisible
    {
        get => this.isCollectionVariablesViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isCollectionVariablesViewVisibleField, value);
    }
    private CollectionVariablesViewModel? collectionVariablesViewDataCtxField = null;
    public CollectionVariablesViewModel? CollectionVariablesViewDataCtx
    {
        get => this.collectionVariablesViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionVariablesViewDataCtxField, value);
    }

    private bool isEnvironmentViewVisibleField = false;
    public bool IsEnvironmentViewVisible
    {
        get => this.isEnvironmentViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isEnvironmentViewVisibleField, value);
    }
    private EnvironmentViewModel? environmentViewDataCtxField = null;
    public EnvironmentViewModel? EnvironmentViewDataCtx
    {
        get => this.environmentViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.environmentViewDataCtxField, value);
    }

    private bool isCollectionFolderViewVisibleField = false;
    public bool IsCollectionFolderViewVisible
    {
        get => this.isCollectionFolderViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isCollectionFolderViewVisibleField, value);
    }
    private CollectionFolderViewModel? collectionFolderViewDataCtxField = null;
    public CollectionFolderViewModel? CollectionFolderViewDataCtx
    {
        get => this.collectionFolderViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.collectionFolderViewDataCtxField, value);
    }

    private bool isHttpRequestViewVisibleField = false;
    public bool IsHttpRequestViewVisible
    {
        get => this.isHttpRequestViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isHttpRequestViewVisibleField, value);
    }
    private HttpRequestViewModel? httpRequestViewDataCtxField = null;
    public HttpRequestViewModel? HttpRequestViewDataCtx
    {
        get => this.httpRequestViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.httpRequestViewDataCtxField, value);
    }

    private bool isWebSocketConnectionViewVisibleField = false;
    public bool IsWebSocketConnectionViewVisible
    {
        get => this.isWebSocketConnectionViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isWebSocketConnectionViewVisibleField, value);
    }
    private WebSocketConnectionViewModel? webSocketConnectionViewDataCtxField = null;
    public WebSocketConnectionViewModel? WebSocketConnectionViewDataCtx
    {
        get => this.webSocketConnectionViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.webSocketConnectionViewDataCtxField, value);
    }

    private bool isWebSocketClientMessageViewVisibleField = false;
    public bool IsWebSocketClientMessageViewVisible
    {
        get => this.isWebSocketClientMessageViewVisibleField;
        set => this.RaiseAndSetIfChanged(ref this.isWebSocketClientMessageViewVisibleField, value);
    }
    private WebSocketClientMessageViewModel? webSocketRequestMessageViewDataCtxField = null;
    public WebSocketClientMessageViewModel? WebSocketClientMessageViewDataCtx
    {
        get => this.webSocketRequestMessageViewDataCtxField;
        set => this.RaiseAndSetIfChanged(ref this.webSocketRequestMessageViewDataCtxField, value);
    }

    #endregion

    #region LANGUAGE

    private bool isLanguagePortugueseField = false;
    public bool IsLanguagePortuguese
    {
        get => this.isLanguagePortugueseField;
        set => this.RaiseAndSetIfChanged(ref this.isLanguagePortugueseField, value);
    }

    private bool isLanguageEnglishField = false;
    public bool IsLanguageEnglish
    {
        get => this.isLanguageEnglishField;
        set => this.RaiseAndSetIfChanged(ref this.isLanguageEnglishField, value);
    }

    public ReactiveCommand<Unit, Unit> SelectLanguagePortuguesCmd { get; }
    public ReactiveCommand<Unit, Unit> SelectLanguageEnglishCmd { get; }

    #endregion

    #region GLOBAL OPTIONS

    private bool isSslVerificationDisabledField = false;
    public bool IsSslVerificationDisabled
    {
        get => this.isSslVerificationDisabledField;
        set => this.RaiseAndSetIfChanged(ref this.isSslVerificationDisabledField, value);
    }

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
        this.collectionsGroupViewDataCtxField = new(this, OnCollectionsGroupItemSelected);
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