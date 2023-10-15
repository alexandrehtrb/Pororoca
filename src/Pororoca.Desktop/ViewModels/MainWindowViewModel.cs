using System.Diagnostics;
using System.Reactive;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.UserData;
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

    [Reactive]
    public bool IsSavedLabelVisible { get; set; }

    public ReactiveCommand<Unit, Unit> SaveAllCmd { get; }

    #endregion

    #region SCREENS

    public PageHolder<CollectionViewModel> CollectionView { get; }

    public PageHolder<CollectionVariablesViewModel> CollectionVariablesView { get; }

    public PageHolder<CollectionScopedAuthViewModel> CollectionScopedAuthView { get; }

    public PageHolder<EnvironmentViewModel> EnvironmentView { get; }

    public PageHolder<CollectionFolderViewModel> CollectionFolderView { get; }

    public PageHolder<HttpRequestViewModel> HttpRequestView { get; }

    public PageHolder<WebSocketConnectionViewModel> WebSocketConnectionView { get; }

    public PageHolder<WebSocketClientMessageViewModel> WebSocketClientMessageView { get; }

    private readonly List<PageHolder> pages;

    #endregion

    #region LANGUAGE

    [Reactive]
    public bool IsLanguagePortuguese { get; set; }
    public ReactiveCommand<Unit, Unit> SelectLanguagePortuguesCmd { get; }

    [Reactive]
    public bool IsLanguageEnglish { get; set; }
    public ReactiveCommand<Unit, Unit> SelectLanguageEnglishCmd { get; }

    [Reactive]
    public bool IsLanguageRussian { get; set; }
    public ReactiveCommand<Unit, Unit> SelectLanguageRussianCmd { get; }

    #endregion

    #region THEMES

    [Reactive]
    public bool IsThemeLight { get; set; }
    public ReactiveCommand<Unit, Unit> SwitchToLightThemeCmd { get; }

    [Reactive]
    public bool IsThemeDark { get; set; }
    public ReactiveCommand<Unit, Unit> SwitchToDarkThemeCmd { get; }

    [Reactive]
    public bool IsThemePampa { get; set; }
    public ReactiveCommand<Unit, Unit> SwitchToPampaThemeCmd { get; }

    [Reactive]
    public bool IsThemeAmazonianNight { get; set; }
    public ReactiveCommand<Unit, Unit> SwitchToAmazonianNightThemeCmd { get; }

    #endregion

    #region GLOBAL OPTIONS

    [Reactive]
    public bool IsSslVerificationDisabled { get; set; }

    public ReactiveCommand<Unit, Unit> ToggleSSLVerificationCmd { get; }

    #endregion

    #region USER PREFERENCES

    private UserPreferences? UserPrefs { get; set; }

    #endregion

    #region UI TESTS

    [Reactive]
    public bool IsRunUITestsVisible { get; set; } =
#if DEBUG || UI_TESTS_ENABLED
        true;
#else
        false;
#endif

    public ReactiveCommand<Unit, Unit> RunUITestsCmd { get; }

    #endregion

    #region VERSION NAME

    [Reactive]
    public string VersionName { get; set; }

    #endregion

    public MainWindowViewModel()
    {
        #region COLLECTIONS ORGANIZATION
        CollectionsGroupViewDataCtx = new(this, OnCollectionsGroupItemSelected);
        ImportCollectionsCmd = ReactiveCommand.CreateFromTask(ImportCollectionsAsync);
        AddNewCollectionCmd = ReactiveCommand.Create(AddNewCollection);
        IsSavedLabelVisible = false;
        SaveAllCmd = ReactiveCommand.CreateFromTask(SaveAllAsync);
        #endregion

        #region SCREENS
        this.pages = new();
        this.pages.Add(CollectionView = new());
        this.pages.Add(CollectionVariablesView = new());
        this.pages.Add(CollectionScopedAuthView = new());
        this.pages.Add(EnvironmentView = new());
        this.pages.Add(CollectionFolderView = new());
        this.pages.Add(HttpRequestView = new());
        this.pages.Add(WebSocketConnectionView = new());
        this.pages.Add(WebSocketClientMessageView = new());
        #endregion

        #region LANGUAGE
        SelectLanguagePortuguesCmd = ReactiveCommand.Create(() => SelectLanguage(Language.Portuguese));
        SelectLanguageEnglishCmd = ReactiveCommand.Create(() => SelectLanguage(Language.English));
        SelectLanguageRussianCmd = ReactiveCommand.Create(() => SelectLanguage(Language.Russian));
        #endregion

        #region THEMES
        SwitchToLightThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.Light));
        SwitchToPampaThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.Pampa));
        SwitchToDarkThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.Dark));
        SwitchToAmazonianNightThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.AmazonianNight));
        UpdateMenuSelectedTheme();
        #endregion

        #region GLOBAL OPTIONS
        ToggleSSLVerificationCmd = ReactiveCommand.Create(ToggleSslVerification);
        #endregion

        #region USER DATA
        LoadUserData();
        #endregion

        #region UI TESTS
        RunUITestsCmd = ReactiveCommand.CreateFromTask(RunUITestsAsync);
        #endregion

        #region VERSION NAME
        VersionName = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        #endregion
    }

    #region SCREENS

    private void OnCollectionsGroupItemSelected(ViewModelBase? selectedItem)
    {
        if (selectedItem is EnvironmentsGroupViewModel)
        {
            return; // do nothing
        }

        var currentPage = this.pages.FirstOrDefault(p => p.Visible);
        var nextPage = this.pages.FirstOrDefault(p => p.PageType == selectedItem?.GetType());
        if (nextPage is not null)
        {
            nextPage.SetVM(selectedItem);
            if (currentPage is not null)
            {
                currentPage.Visible = false;
            }
            nextPage.Visible = true;
        }
        else
        {
            this.pages.ForEach(p => p.Visible = false);
        }
    }

    #endregion

    #region COLLECTIONS ORGANIZATION

    public void MoveSubItem(ICollectionOrganizationItemViewModel colItemVm, MoveableItemMovementDirection direction) =>
        CollectionsGroupViewDataCtx.MoveSubItem(colItemVm, direction);

    public void AddNewCollection()
    {
        PororocaCollection newCol = new(Localizer.Instance.Collection.NewCollection);
        AddCollection(newCol, showItemInScreen: true);
    }

    internal void AddCollection(PororocaCollection col, bool showItemInScreen = false)
    {
        CollectionViewModel colVm = new(this, col);
        CollectionsGroupViewDataCtx.Items.Add(colVm);
        CollectionsGroupViewDataCtx.RefreshSubItemsAvailableMovements();
        if (showItemInScreen)
        {
            CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = colVm;
        }
    }

    public void DuplicateCollection(CollectionViewModel colVm)
    {
        var collectionCopy = colVm.ToCollection().Copy(preserveIds: false);
        AddCollection(collectionCopy);
    }

    private void onRenameItemSelected(ViewModelBase vm) =>
        OnCollectionsGroupItemSelected(vm);

    public void DeleteSubItem(ICollectionOrganizationItemViewModel item)
    {
        CollectionsGroupViewDataCtx.Items.Remove((CollectionViewModel)item);
        CollectionsGroupViewDataCtx.RefreshSubItemsAvailableMovements();
        onAfterItemDeleted();
    }

    private void onAfterItemDeleted() =>
        this.pages.ForEach(p => p.Visible = false);

    private Task ImportCollectionsAsync() =>
        FileExporterImporter.ImportCollectionsAsync(this);

    private async Task SaveAllAsync()
    {
        SaveUserData();
        IsSavedLabelVisible = true;
        await Task.Delay(3000);
        IsSavedLabelVisible = false;
    }

    #endregion

    #region LANGUAGE

    private void SelectLanguage(Language lang)
    {
        Localizer.Instance.CurrentLanguage = lang;
        switch (lang)
        {
            case Language.Portuguese:
                IsLanguagePortuguese = true;
                IsLanguageEnglish = false;
                IsLanguageRussian = false;
                break;
            default:
            case Language.English:
                IsLanguagePortuguese = false;
                IsLanguageEnglish = true;
                IsLanguageRussian = false;
                break;
            case Language.Russian:
                IsLanguagePortuguese = false;
                IsLanguageEnglish = false;
                IsLanguageRussian = true;
                break;
        }
    }

    #endregion

    #region THEMES

    private void SwitchToTheme(PororocaTheme theme)
    {
        PororocaThemeManager.CurrentTheme = theme;
        UpdateMenuSelectedTheme();
    }

    private void UpdateMenuSelectedTheme()
    {
        IsThemeLight = IsThemeDark = IsThemePampa = IsThemeAmazonianNight = false;
        switch (PororocaThemeManager.CurrentTheme)
        {
            case PororocaTheme.Light:
                IsThemeLight = true;
                break;
            case PororocaTheme.Dark:
                IsThemeDark = true;
                break;
            case PororocaTheme.Pampa:
                IsThemePampa = true;
                break;
            case PororocaTheme.AmazonianNight:
                IsThemeAmazonianNight = true;
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
        new(Language.English, DateTime.Now, PororocaThemeManager.DefaultTheme);

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
        UserPrefs!.SetLanguage(Localizer.Instance.CurrentLanguage);

        var cols = CollectionsGroupViewDataCtx
                                    .Items
                                    .Select(cvm => cvm.ToCollection())
                                    .DistinctBy(c => c.Id)
                                    .ToArray();

        UserPrefs!.Theme = PororocaThemeManager.CurrentTheme;

        UserDataManager.SaveUserData(UserPrefs, cols);
    }

    private void ShowUpdateReminder()
    {
        Bitmap bitmap = new(AssetLoader.Open(new("avares://Pororoca.Desktop/Assets/Images/pororoca.png")));

        var msgbox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams()
            {
                ContentTitle = Localizer.Instance.UpdateReminder.DialogTitle,
                ContentMessage = Localizer.Instance.UpdateReminder.DialogMessage,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = new(bitmap),
                ButtonDefinitions = ButtonEnum.OkCancel
            });
        Dispatcher.UIThread.Post(async () =>
        {
            var buttonResult = await msgbox.ShowAsync();
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

    #region UI TESTS

#if DEBUG || UI_TESTS_ENABLED
    private async Task RunUITestsAsync()
    {
        /*
        IMPORTANT:
        To run the UI tests, run Pororoca.TestServer in localhost and 
        have the TestFiles directory inside the PororocaUserData folder.        
        */

        // making a backup of the items' tree and clearing it before the tests
        var bkupedLang = Localizer.Instance.CurrentLanguage;
        var bkupedItems = CollectionsGroupViewDataCtx.Items.ToList();
        CollectionsGroupViewDataCtx.Items.Clear();
        SelectLanguage(Language.English);

        string resultsLog = await Pororoca.Desktop.UITesting.UITestsRunner.RunAllTestsAsync();

        // restoring the items' tree after the tests
        foreach (var item in bkupedItems) { CollectionsGroupViewDataCtx.Items.Add(item); }
        CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        SelectLanguage(bkupedLang);

        Bitmap bitmap = new(AssetLoader.Open(new("avares://Pororoca.Desktop/Assets/Images/pororoca.png")));

        var msgbox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams()
            {
                ContentTitle = "UI tests results",
                ContentMessage = resultsLog,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = new(bitmap),
                ButtonDefinitions = ButtonEnum.Ok
            });
        Dispatcher.UIThread.Post(async () => await msgbox.ShowAsync());
    }
#else
    private Task RunUITestsAsync() => Task.CompletedTask;
#endif

    #endregion
}