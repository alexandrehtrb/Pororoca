using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.Localization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
#if DEBUG || UI_TESTS_ENABLED
using Pororoca.Desktop.UITesting;
using Pororoca.Desktop.UITesting.Tests;
#endif

namespace Pororoca.Desktop.ViewModels;

public sealed class UITestViewModel : ViewModelBase
{
    [Reactive]
    public bool Include { get; set; }

    [Reactive]
    public string Name { get; set; }

#if DEBUG || UI_TESTS_ENABLED
    public UITest Test { get; }

    public UITestViewModel(string name, UITest test)
    {
        Include = true;
        Name = name;
        Test = test;
    }
#endif
}

public sealed class UITestsPrepareWindowViewModel : ViewModelBase
{
    public static readonly UITestsPrepareWindowViewModel Instance = new();

    [Reactive]
    public int ActionsWaitingTimeInMs { get; set; }

    [Reactive]
    public bool TestFilesFolderFound { get; set; }

    public ObservableCollection<UITestViewModel> Tests { get; }

    public ReactiveCommand<Unit, Unit> SelectAllTestsCmd { get; }

    public ReactiveCommand<Unit, Unit> DeselectAllTestsCmd { get; }

    private UITestsPrepareWindowViewModel()
    {
        ActionsWaitingTimeInMs = 20;
        Tests =
        [
#if DEBUG || UI_TESTS_ENABLED
            new(nameof(TopMenuUITest), new TopMenuUITest()),
            new(nameof(SwitchLanguagesUITest), new SwitchLanguagesUITest()),
            new(nameof(SwitchThemesUITest), new SwitchThemesUITest()),
            new(nameof(EditableTextBlockUITest), new EditableTextBlockUITest()),
            new(nameof(CollectionAndCollectionFolderUITest), new CollectionAndCollectionFolderUITest()),
            new(nameof(TreeCopyAndPasteItemsUITest), new TreeCopyAndPasteItemsUITest()),
            new(nameof(TreeCutAndPasteItemsUITest), new TreeCutAndPasteItemsUITest()),
            new(nameof(TreeDeleteItemsUITest), new TreeDeleteItemsUITest()),
            new(nameof(HttpRequestValidationsUITest), new HttpRequestValidationsUITest()),
            new(nameof(HttpRequestsUITest), new HttpRequestsUITest()),
            new(nameof(VariablesCutCopyPasteDeleteUITest), new VariablesCutCopyPasteDeleteUITest()),
            new(nameof(HeadersCutCopyPasteDeleteUITest), new HeadersCutCopyPasteDeleteUITest()),
            new(nameof(UrlEncodedParamsCutCopyPasteDeleteUITest), new UrlEncodedParamsCutCopyPasteDeleteUITest()),
            new(nameof(FormDataParamsCutCopyPasteDeleteUITest), new FormDataParamsCutCopyPasteDeleteUITest()),
            new(nameof(WebSocketsValidationsUITest), new WebSocketsValidationsUITest()),
            new(nameof(WebSocketsUITest), new WebSocketsUITest()),
            new(nameof(SaveAndRestoreCollectionUITest), new SaveAndRestoreCollectionUITest()),
            new(nameof(ExportCollectionsUITest), new ExportCollectionsUITest()),
            new(nameof(ExportEnvironmentsUITest), new ExportEnvironmentsUITest()),
            new(nameof(CollectionScopedAuthUITest), new CollectionScopedAuthUITest()),
            new(nameof(HttpRepeaterUITest), new HttpRepeaterUITest()),
            new(nameof(HttpRepeaterValidationsUITest), new HttpRepeaterValidationsUITest()),
            new(nameof(ResponseCapturesUITest), new ResponseCapturesUITest()),
            // Out of scope of automated UI tests:
            // some keybindings
            // all dialogs
            // context menu actions
            // form data add text and file params buttons
            // cut collection, cut and paste to itself
            // save responses to file
            // welcome screen
            // http headers names and values autocomplete
#endif
        ];
        SelectAllTestsCmd = ReactiveCommand.Create(SelectAllTests);
        DeselectAllTestsCmd = ReactiveCommand.Create(DeselectAllTests);
    }

    public void CheckIfTestFilesFolderExist() =>
#if DEBUG || UI_TESTS_ENABLED
        TestFilesFolderFound = Directory.Exists(UITest.GetTestFilesDirPath());
#else
        TestFilesFolderFound = false;
#endif

    private void SelectAllTests()
    {
        foreach (var test in Tests)
        {
            test.Include = true;
        }
    }

    private void DeselectAllTests()
    {
        foreach (var test in Tests)
        {
            test.Include = false;
        }
    }

    internal void RunTests() => Dispatcher.UIThread.Post(async () => await RunTestsAsync());

#if DEBUG || UI_TESTS_ENABLED
    private async Task RunTestsAsync()
    {
        /*
        IMPORTANT:
        To run the UI tests, run Pororoca.TestServer in localhost and
        have the TestFiles directory inside the PororocaUserData folder.
        */

        // making a backup of the items' tree and clearing it before the tests
        var bkupedLang = Localizer.Instance.CurrentLanguage;
        var bkupedItems = MainWindowVm.CollectionsGroupViewDataCtx.Items.ToList();
        MainWindowVm.CollectionsGroupViewDataCtx.Items.Clear();
        MainWindowVm.SelectLanguage(Language.English);
        if (Pororoca.Desktop.Views.MainWindow.Instance!.Clipboard is Avalonia.Input.Platform.IClipboard systemClipboard)
        {
            await systemClipboard.ClearAsync();
        }

        var waitingTimeBetweenActions = TimeSpan.FromMilliseconds(ActionsWaitingTimeInMs);
        var tests = Tests.Where(t => t.Include).Select(t => t.Test).ToArray();

        string resultsLog = await UITestsRunner.RunTestsAsync(waitingTimeBetweenActions, tests);

        // restoring the items' tree after the tests
        foreach (var item in bkupedItems)
        {
            MainWindowVm.CollectionsGroupViewDataCtx.Items.Add(item);
        }
        MainWindowVm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        MainWindowVm.SelectLanguage(bkupedLang);

        Dialogs.ShowDialog(
            title: "UI tests results",
            message: resultsLog,
            buttons: ButtonEnum.Ok);
    }
#else
    private Task RunTestsAsync() => Task.CompletedTask;
#endif
}