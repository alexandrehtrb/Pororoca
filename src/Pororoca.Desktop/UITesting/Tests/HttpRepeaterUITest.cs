using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRepeaterUITest : UITest
{
    private static readonly ObservableCollection<VariableViewModel> defaultColVars = GenerateCollectionVariables();
    private static readonly ObservableCollection<VariableViewModel> defaultEnvVars = GenerateEnvironmentVariables();

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionVariablesRobot ColVarsRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }
    private HttpRepeaterRobot RepeaterRobot { get; }

    public HttpRepeaterUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
        RepeaterRobot = new(RootView.FindControl<HttpRepeaterView>("httpRepView")!);
    }

    public override async Task RunAsync()
    {
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.SetVariables(defaultColVars);

        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");
        await EnvRobot.SetVariables(defaultEnvVars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPREQ");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/json");
        await HttpRobot.SetRawBody("application/json", "{\"myValue\":\"{{SpecialValue1}}\"}");

        if (OperatingSystem.IsLinux())
        {
            // we can't trust ASP.NET Core dev certs in Linux,
            // so we disable TLS verification for requests to our TestServer
            await TopMenuRobot.SwitchTlsVerification(false);
        }

        await TreeRobot.Select("COL1");
        await ColRobot.AddRepeater.ClickOn();

        await RepeaterRobot.Name.Edit("REPSIMPLE");
        await RepeaterRobot.BaseHttpRequest.Select("HTTPREQ");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        await RepeaterRobot.RepetitionMode.Select(RepeaterRobot.OptionRepetitionModeSimple);
        AssertIsVisible(RepeaterRobot.NumberOfRepetitions);
        AssertIsVisible(RepeaterRobot.MaxDop);
        AssertIsVisible(RepeaterRobot.DelayInMs);
        await RepeaterRobot.NumberOfRepetitions.SetValue(2);
        await RepeaterRobot.MaxDop.SetValue(1);
        await RepeaterRobot.DelayInMs.SetValue(0);
        await RepeaterRobot.StartOrStopRepetition.RaiseClickEvent();
        await Wait(3);

        AssertIsHidden(RepeaterRobot.ErrorMessage);
        AssertIsVisible(RepeaterRobot.RepetitionStatusMessage);
        AssertContainsText(RepeaterRobot.RepetitionStatusMessage, "Finished 2 repetitions. Elapsed time:");
        await AssertResultAsync(2, 0, "{{SpecialValue1}}");
        await AssertResultAsync(2, 1, "{{SpecialValue1}}");
        AssertIsVisible(RepeaterRobot.RepetitionStatusMessage);
        AssertIsVisible(RepeaterRobot.ExportReport);
        AssertIsVisible(RepeaterRobot.SaveAllResponses);
        AssertIsVisible(RepeaterRobot.ExportAllLogs);

        await RepeaterRobot.Name.Edit("REPSEQUENTIAL");
        await RepeaterRobot.BaseHttpRequest.Select("HTTPREQ");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        await RepeaterRobot.RepetitionMode.Select(RepeaterRobot.OptionRepetitionModeSequential);
        AssertIsHidden(RepeaterRobot.NumberOfRepetitions);
        AssertIsHidden(RepeaterRobot.MaxDop);
        AssertIsVisible(RepeaterRobot.DelayInMs);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        await RepeaterRobot.InputDataType.Select(RepeaterRobot.OptionInputDataTypeRaw);
        await RepeaterRobot.InputDataRawEditor.ClearAndTypeText("[{\"SpecialValue1\":\"Nissan 300ZX\"},{\"SpecialValue1\":\"Nissan 350Z\"}]");
        await RepeaterRobot.StartOrStopRepetition.RaiseClickEvent();
        await Wait(3);

        AssertIsHidden(RepeaterRobot.ErrorMessage);
        AssertIsVisible(RepeaterRobot.RepetitionStatusMessage);
        AssertContainsText(RepeaterRobot.RepetitionStatusMessage, "Finished 2 repetitions. Elapsed time:");
        await AssertResultAsync(2, 0, "Nissan 300ZX");
        await AssertResultAsync(2, 1, "Nissan 350Z");
        AssertIsVisible(RepeaterRobot.RepetitionStatusMessage);
        AssertIsVisible(RepeaterRobot.ExportReport);
        AssertIsVisible(RepeaterRobot.SaveAllResponses);
        AssertIsVisible(RepeaterRobot.ExportAllLogs);

        await RepeaterRobot.Name.Edit("REPRANDOM");
        await RepeaterRobot.BaseHttpRequest.Select("HTTPREQ");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        await RepeaterRobot.RepetitionMode.Select(RepeaterRobot.OptionRepetitionModeRandom);
        AssertIsVisible(RepeaterRobot.NumberOfRepetitions);
        AssertIsVisible(RepeaterRobot.MaxDop);
        AssertIsVisible(RepeaterRobot.DelayInMs);
        await RepeaterRobot.NumberOfRepetitions.SetValue(2);
        await RepeaterRobot.MaxDop.SetValue(1);
        await RepeaterRobot.DelayInMs.SetValue(0);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        await RepeaterRobot.InputDataType.Select(RepeaterRobot.OptionInputDataTypeFile);
        await RepeaterRobot.InputDataFileSrcPath.ClearAndTypeText("{{TestFilesDir}}/InputData.json");
        await RepeaterRobot.StartOrStopRepetition.RaiseClickEvent();
        await Wait(3);

        AssertIsHidden(RepeaterRobot.ErrorMessage);
        AssertIsVisible(RepeaterRobot.RepetitionStatusMessage);
        AssertContainsText(RepeaterRobot.RepetitionStatusMessage, "Finished 2 repetitions. Elapsed time:");
        await AssertResultAsync(2, 0, "Nissan Skyline");
        await AssertResultAsync(2, 1, "Nissan Skyline");
        AssertIsVisible(RepeaterRobot.RepetitionStatusMessage);
        AssertIsVisible(RepeaterRobot.ExportReport);
        AssertIsVisible(RepeaterRobot.SaveAllResponses);
        AssertIsVisible(RepeaterRobot.ExportAllLogs);

        if (OperatingSystem.IsLinux())
        {
            // reenable TLS verification
            await TopMenuRobot.SwitchTlsVerification(true);
        }
    }

    private async Task AssertResultAsync(int expectedCount, int index, string myValue)
    {
        var results = RepeaterRobot.RepetitionResults.ItemsSource!.Cast<HttpRepetitionResultViewModel>();

        Assert(expectedCount == results.Count());

        var result = results.ElementAt(index);
        RepeaterRobot.RepetitionResults.SelectedItem = result;
        await UITestActions.WaitAfterActionAsync();

        AssertContainsText(RepeaterRobot.ResultDetailTitle, "Response: 200 OK");
        await RepeaterRobot.TabControlResultDetail.Select(RepeaterRobot.TabItemResultDetailHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "application/json; charset=utf-8");
        await RepeaterRobot.TabControlResultDetail.Select(RepeaterRobot.TabItemResultDetailBody);
        AssertHasText(RepeaterRobot.ResponseBodyRawEditor, "{" + Environment.NewLine + $"  \"myValue\": \"{myValue}\"" + Environment.NewLine + "}");
        AssertIsVisible(RepeaterRobot.ResultDetailBodySaveToFile);
    }

    private void AssertContainsResponseHeader(string key)
    {
        var vm = ((HttpRepeaterViewModel)RepeaterRobot.RootView!.DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
        Assert(vm.Items.Any(h => h.Key == key));
    }

    private void AssertContainsResponseHeader(string key, string value)
    {
        var vm = ((HttpRepeaterViewModel)RepeaterRobot.RootView!.DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
        Assert(vm.Items.Any(h => h.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && h.Value == value));
    }

    private static ObservableCollection<VariableViewModel> GenerateCollectionVariables()
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "TestFilesDir", Path.Combine(GetTestFilesDirPath()), false)));
        return parent;
    }

    private static ObservableCollection<VariableViewModel> GenerateEnvironmentVariables()
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "BaseUrl", "https://localhost:5001", false)));
        return parent;
    }
}