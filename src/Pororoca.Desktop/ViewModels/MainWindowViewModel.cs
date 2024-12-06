using System.Diagnostics;
using System.Reactive;
using System.Reflection;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using Pororoca.Desktop.ExportImport;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using Pororoca.Desktop.UserData;
using Pororoca.Domain.Features.Entities.GitHub;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.UpdateChecker;
using Pororoca.Infrastructure.Features.Requester;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, ICollectionOrganizationItemParentViewModel
{
    #region COLLECTIONS ORGANIZATION

    private static readonly SemaphoreSlim loadSaveUserDataSemaphore = new(1, 1);

    [Reactive]
    public CollectionsGroupViewModel CollectionsGroupViewDataCtx { get; set; }

    public ReactiveCommand<Unit, Unit> AddNewCollectionCmd { get; }
    public ReactiveCommand<Unit, Unit> ImportCollectionsFromFileCmd { get; }

    [Reactive]
    public bool IsSavedLabelVisible { get; set; }

    public ReactiveCommand<Unit, Unit> SaveAllCmd { get; }

    #endregion

    #region SCREENS

    public PageHolder<WelcomeViewModel> WelcomeView { get; }

    public PageHolder<CollectionViewModel> CollectionView { get; }

    public PageHolder<ExportCollectionViewModel> ExportCollectionView { get; }

    public PageHolder<CollectionVariablesViewModel> CollectionVariablesView { get; }

    public PageHolder<CollectionScopedAuthViewModel> CollectionScopedAuthView { get; }

    public PageHolder<CollectionScopedRequestHeadersViewModel> CollectionScopedRequestHeadersView { get; }

    public PageHolder<EnvironmentViewModel> EnvironmentView { get; }

    public PageHolder<ExportEnvironmentViewModel> ExportEnvironmentView { get; }

    public PageHolder<CollectionFolderViewModel> CollectionFolderView { get; }

    public PageHolder<HttpRequestViewModel> HttpRequestView { get; }

    public PageHolder<WebSocketConnectionViewModel> WebSocketConnectionView { get; }

    public PageHolder<WebSocketClientMessageViewModel> WebSocketClientMessageView { get; }

    public PageHolder<HttpRepeaterViewModel> HttpRepeaterView { get; }

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

    [Reactive]
    public bool IsLanguageItalian { get; set; }
    public ReactiveCommand<Unit, Unit> SelectLanguageItalianCmd { get; }

    [Reactive]
    public bool IsLanguageSimplifiedChinese { get; set; }
    public ReactiveCommand<Unit, Unit> SelectLanguageSimplifiedChineseCmd { get; }

    #endregion

    #region THEMES

    [Reactive]
    public bool IsThemeLight { get; set; }
    public ReactiveCommand<Unit, Unit> SwitchToLightThemeCmd { get; }

    [Reactive]
    public bool IsThemeLight2 { get; set; }
    public ReactiveCommand<Unit, Unit> SwitchToLight2ThemeCmd { get; }

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

    private bool isSslVerificationDisabledField;
    public bool IsSslVerificationDisabled
    {
        get => this.isSslVerificationDisabledField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.isSslVerificationDisabledField, value);
            PororocaRequester.Singleton.DisableSslVerification = value;
        }
    }

    public ReactiveCommand<Unit, Unit> ToggleSSLVerificationCmd { get; }

    #endregion

    #region USER PREFERENCES

    private UserPreferences? UserPrefs { get; set; }


    private bool autoCheckForUpdatesEnabledField;
    public bool AutoCheckForUpdatesEnabled
    {
        get => this.autoCheckForUpdatesEnabledField;
        set
        {
            this.RaiseAndSetIfChanged(ref this.autoCheckForUpdatesEnabledField, value);
            if (UserPrefs != null)
            {
                UserPrefs.AutoCheckForUpdates = value;
            }
        }
    }

    public ReactiveCommand<Unit, Unit> ToggleAutoCheckForUpdatesCmd { get; }

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

    #region WEBSITES

    private const string GitHubRepoUrl = "https://github.com/alexandrehtrb/Pororoca";
    private const string DocsWebSiteUrl = "https://pororoca.io/docs";

    public ReactiveCommand<Unit, Unit> OpenDocsInWebBrowserCmd { get; }

    public ReactiveCommand<Unit, Unit> OpenGitHubRepoInWebBrowserCmd { get; }

    #endregion

    public MainWindowViewModel()
    {
        #region COLLECTIONS ORGANIZATION
        CollectionsGroupViewDataCtx = new(this, SwitchVisiblePage);
        ImportCollectionsFromFileCmd = ReactiveCommand.CreateFromTask(ImportCollectionsAsync);
        AddNewCollectionCmd = ReactiveCommand.Create(AddNewCollection);
        IsSavedLabelVisible = false;
        SaveAllCmd = ReactiveCommand.CreateFromTask(SaveAllAsync);
        #endregion

        #region SCREENS
        this.pages = new();
        this.pages.Add(WelcomeView = new());
        this.pages.Add(CollectionView = new());
        this.pages.Add(ExportCollectionView = new());
        this.pages.Add(CollectionVariablesView = new());
        this.pages.Add(CollectionScopedAuthView = new());
        this.pages.Add(CollectionScopedRequestHeadersView = new());
        this.pages.Add(EnvironmentView = new());
        this.pages.Add(ExportEnvironmentView = new());
        this.pages.Add(CollectionFolderView = new());
        this.pages.Add(HttpRequestView = new());
        this.pages.Add(WebSocketConnectionView = new());
        this.pages.Add(WebSocketClientMessageView = new());
        this.pages.Add(HttpRepeaterView = new());
        #endregion

        #region LANGUAGE
        SelectLanguagePortuguesCmd = ReactiveCommand.Create(() => SelectLanguage(Language.Portuguese));
        SelectLanguageEnglishCmd = ReactiveCommand.Create(() => SelectLanguage(Language.English));
        SelectLanguageRussianCmd = ReactiveCommand.Create(() => SelectLanguage(Language.Russian));
        SelectLanguageItalianCmd = ReactiveCommand.Create(() => SelectLanguage(Language.Italian));
        SelectLanguageSimplifiedChineseCmd = ReactiveCommand.Create(() => SelectLanguage(Language.SimplifiedChinese));
        #endregion

        #region THEMES
        SwitchToLightThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.Light));
        SwitchToLight2ThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.Light2));
        SwitchToPampaThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.Pampa));
        SwitchToDarkThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.Dark));
        SwitchToAmazonianNightThemeCmd = ReactiveCommand.Create(() => SwitchToTheme(PororocaTheme.AmazonianNight));
        UpdateMenuSelectedTheme();
        #endregion

        #region GLOBAL OPTIONS
        ToggleSSLVerificationCmd = ReactiveCommand.Create(ToggleSslVerification);
        #endregion

        #region USER DATA
        ToggleAutoCheckForUpdatesCmd = ReactiveCommand.Create(ToggleAutoCheckForUpdates);
        LoadUserData();
        ShowWelcomeIfNoCollectionsExist();
        #endregion

        #region UI TESTS
        RunUITestsCmd = ReactiveCommand.CreateFromTask(RunUITestsAsync);
        #endregion

        #region VERSION NAME
        VersionName = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        #endregion

        #region WEBSITES
        OpenDocsInWebBrowserCmd = ReactiveCommand.Create(() => OpenWebBrowser(DocsWebSiteUrl));
        OpenGitHubRepoInWebBrowserCmd = ReactiveCommand.Create(() => OpenWebBrowser(GitHubRepoUrl));
        #endregion
    }

    #region SCREENS

    public void SwitchVisiblePage(ViewModelBase? selectedItem)
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

    private void ShowWelcomeIfNoCollectionsExist()
    {
        if (CollectionsGroupViewDataCtx.Items.Count == 0)
        {
            if (WelcomeView.VM is null)
            {
                WelcomeView.SetVM(WelcomeViewModel.Instance);
            }
            WelcomeView.Visible = true;
        }
    }

    #endregion

    #region COLLECTIONS ORGANIZATION

    public void MoveSubItemUp(CollectionOrganizationItemViewModel colItemVm) =>
        CollectionsGroupViewDataCtx.MoveSubItemUp(colItemVm);

    public void MoveSubItemDown(CollectionOrganizationItemViewModel colItemVm) =>
        CollectionsGroupViewDataCtx.MoveSubItemDown(colItemVm);

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

    public void DeleteSubItem(CollectionOrganizationItemViewModel item)
    {
        CollectionsGroupViewDataCtx.Items.Remove((CollectionViewModel)item);
        CollectionsGroupViewDataCtx.RefreshSubItemsAvailableMovements();
        HideAllPages();
        ShowWelcomeIfNoCollectionsExist();
    }

    public void HideAllPages() =>
        this.pages.ForEach(p => p.Visible = false);

    public Task ImportCollectionsAsync() =>
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

    internal void SelectLanguage(Language lang)
    {
        Localizer.Instance.CurrentLanguage = lang;
        IsLanguagePortuguese = lang == Language.Portuguese;
        IsLanguageEnglish = lang == Language.English;
        IsLanguageRussian = lang == Language.Russian;
        IsLanguageItalian = lang == Language.Italian;
        IsLanguageSimplifiedChinese = lang == Language.SimplifiedChinese;
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
        IsThemeLight = PororocaThemeManager.CurrentTheme == PororocaTheme.Light;
        IsThemeLight2 = PororocaThemeManager.CurrentTheme == PororocaTheme.Light2;
        IsThemeDark = PororocaThemeManager.CurrentTheme == PororocaTheme.Dark;
        IsThemePampa = PororocaThemeManager.CurrentTheme == PororocaTheme.Pampa;
        IsThemeAmazonianNight = PororocaThemeManager.CurrentTheme == PororocaTheme.AmazonianNight;
    }

    #endregion

    #region GLOBAL OPTIONS

    private void ToggleSslVerification() =>
        IsSslVerificationDisabled = !IsSslVerificationDisabled;

    #endregion

    #region AUTO-CHECK FOR UPDATES

    private void ToggleAutoCheckForUpdates() =>
        AutoCheckForUpdatesEnabled = !AutoCheckForUpdatesEnabled;

    private void CheckForUpdates() =>
        Task.Run(() => UpdateAvailableChecker.CheckForUpdatesAsync(
            PororocaRequester.Singleton,
            Assembly.GetExecutingAssembly().GetName().Version!,
            (latestReleaseInfo) =>
            {
                Dispatcher.UIThread.Post(() => ShowUpdateReminder(latestReleaseInfo));
            }));

    #endregion

    #region USER DATA

    private static UserPreferences GetDefaultUserPrefs() =>
        new(Language.English, PororocaThemeManager.DefaultTheme, true, DateTime.Now);

    private void LoadUserData()
    {
        LoadUserPreferences();
        LoadUserCollections();
    }

    private void LoadUserPreferences()
    {
        // this needs to be before the migration dialog,
        // because here we load the localization strings
        UserPrefs = UserDataManager.LoadUserPreferences() ?? GetDefaultUserPrefs();
        SelectLanguage(UserPrefs.GetLanguage());

        // setting for userPrefs that don't have this flag
        AutoCheckForUpdatesEnabled = UserPrefs.AutoCheckForUpdates ?? true;

        if (UserPrefs.NeedsToCheckForUpdates())
        {
            UserPrefs!.SetLastUpdateCheckDateAsToday();
            CheckForUpdates();
        }
        else if (UserPrefs.HasLastUpdateCheckDate() == false)
        {
            UserPrefs.SetLastUpdateCheckDateAsToday();
        }

        if (UserDataManager.NeedsMacOSXUserDataFolderMigrationToV3())
        {
            // this is a silent migration.
            // users can manually delete the PororocaUserData_old folder afterwards.
            UserDataManager.ExecuteMacOSXUserDataFolderMigrationToV3();
            ShowMacOSXUserDataFolderMigratedV3Dialog();
        }
    }

    private void LoadUserCollections() =>
        Task.Run(async () =>
        {
            // taken from:
            // https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/

            await loadSaveUserDataSemaphore.WaitAsync();
            try
            {
                await foreach (var col in UserDataManager.LoadUserCollectionsAsync())
                {
                    Dispatcher.UIThread.Post(() => AddCollection(col));
                }
            }
            finally
            {
                loadSaveUserDataSemaphore.Release();
            }
        });

    public void SaveUserData()
    {
        // This means that the user data is still being loaded.
        if (loadSaveUserDataSemaphore.CurrentCount == 0)
        {
            // IMPORTANT!
            // This protects against deleting all collections
            // if they were not loaded yet, in case the
            // startup takes a longer time.
            return;
        }

        UserPrefs!.SetLanguage(Localizer.Instance.CurrentLanguage);

        var cols = CollectionsGroupViewDataCtx
                                    .Items
                                    .Select(cvm => cvm.ToCollection())
                                    .DistinctBy(c => c.Id)
                                    .ToArray();

        UserPrefs!.Theme = PororocaThemeManager.CurrentTheme;

        UserDataManager.SaveUserData(UserPrefs, cols);
    }

    private void ShowUpdateReminder(GitHubGetReleaseResponse latestVersionInfo) =>
        Dialogs.ShowDialog(
            title: Localizer.Instance.UpdateReminder.DialogTitle,
            message: string.Format(Localizer.Instance.UpdateReminder.DialogMessage, latestVersionInfo.Description),
            buttons: ButtonEnum.OkCancel,
            onButtonOkClicked: () => OpenWebBrowser(latestVersionInfo.HtmlUrl));

    private void ShowMacOSXUserDataFolderMigratedV3Dialog()
    {
        string newUserDataDirPath = UserDataManager.GetUserDataFolder().FullName;
        string message = string.Format(Localizer.Instance.MacOSXUserDataFolderMigratedV3Dialog.Message, newUserDataDirPath);

        Dialogs.ShowDialog(
            title: Localizer.Instance.MacOSXUserDataFolderMigratedV3Dialog.Title,
            message: message,
            buttons: ButtonEnum.Ok);
    }

    private static void OpenWebBrowser(string url)
    {
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
    private Task RunUITestsAsync()
    {
        Pororoca.Desktop.Views.UITestsPrepareWindow uiTestsPrepareWindow = new();
        uiTestsPrepareWindow.Show(Pororoca.Desktop.Views.MainWindow.Instance!);
        return Task.CompletedTask;
    }
#else
    private Task RunUITestsAsync() => Task.CompletedTask;
#endif

    #endregion
}