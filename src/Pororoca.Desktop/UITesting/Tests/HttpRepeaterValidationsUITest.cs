using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRepeaterValidationsUITest : UITest
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

    public HttpRepeaterValidationsUITest()
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

        await TreeRobot.Select("COL1");
        await ColRobot.AddRepeater.ClickOn();

        await RepeaterRobot.Name.Edit("REP");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        await RepeaterRobot.RepetitionMode.Select(RepeaterRobot.OptionRepetitionModeSequential);

        await TestBaseRequestValidation();
        await TestInvalidInputData();
    }

    private async Task TestBaseRequestValidation()
    {
        AssertIsHidden(RepeaterRobot.ErrorMessage);
        AssertDoesntHaveStyleClass(RepeaterRobot.BaseHttpRequest, "HasValidationProblem");
        await RepeaterRobot.StartRepetition.ClickOn();
        AssertIsVisible(RepeaterRobot.ErrorMessage);
        AssertHasText(RepeaterRobot.ErrorMessage, "Select the base HTTP request to repeat.");
        AssertHasStyleClass(RepeaterRobot.BaseHttpRequest, "HasValidationProblem");
        await RepeaterRobot.BaseHttpRequest.Select("HTTPREQ");
        AssertIsHidden(RepeaterRobot.ErrorMessage);
        AssertDoesntHaveStyleClass(RepeaterRobot.BaseHttpRequest, "HasValidationProblem");
    }

    private async Task TestInvalidInputData()
    {
        AssertIsHidden(RepeaterRobot.ErrorMessage);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        // raw
        await RepeaterRobot.InputDataType.Select(RepeaterRobot.OptionInputDataTypeRaw);
        await RepeaterRobot.InputDataRawEditor.ClearAndTypeText("[");
        await RepeaterRobot.StartRepetition.ClickOn();
        AssertIsVisible(RepeaterRobot.ErrorMessage);
        AssertHasText(RepeaterRobot.ErrorMessage, "Invalid input data JSON array.");
        // error message will remain visible, because the text only changes inside the TextDocument object
        // file
        await RepeaterRobot.InputDataType.Select(RepeaterRobot.OptionInputDataTypeFile);
        AssertDoesntHaveStyleClass(RepeaterRobot.InputDataFileSrcPath, "HasValidationProblem");
        await RepeaterRobot.InputDataFileSrcPath.ClearAndTypeText("{{TestFilesDir}}/InputData.jso");
        await RepeaterRobot.StartRepetition.ClickOn();
        AssertIsVisible(RepeaterRobot.ErrorMessage);
        AssertHasText(RepeaterRobot.ErrorMessage, "Input data file not found.");
        AssertHasStyleClass(RepeaterRobot.InputDataFileSrcPath, "HasValidationProblem");
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