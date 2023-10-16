using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class WebSocketsValidationsUITest : UITest
{
    private static readonly ObservableCollection<VariableViewModel> defaultColVars = GenerateCollectionVariables();
    private static readonly ObservableCollection<VariableViewModel> defaultEnvVars = GenerateEnvironmentVariables();

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionVariablesRobot ColVarsRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private WebSocketConnectionRobot WsRobot { get; }

    public WebSocketsValidationsUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        WsRobot = new(RootView.FindControl<WebSocketConnectionView>("wsConnView")!);
    }

    public override async Task RunAsync()
    {
        //validate http version

        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.SetVariables(defaultColVars);

        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");
        await EnvRobot.SetVariables(defaultEnvVars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();

        await TreeRobot.Select("COL1");
        await ColRobot.AddWebSocket.ClickOn();
        await WsRobot.Name.Edit("WS");

        await TestUrlValidation();
        await TestHttpVersionValidation();
        await TestSelfSigned();
    }

    private async Task TestUrlValidation()
    {
        async Task TestBadUrl(string url)
        {
            await WsRobot.Url.ClearAndTypeText(url);
            AssertIsHidden(WsRobot.ErrorMsg);
            AssertDoesntHaveStyleClass(WsRobot.Url, "HasValidationProblem");
            await WsRobot.Connect.ClickOn();
            AssertIsVisible(WsRobot.ErrorMsg);
            AssertHasText(WsRobot.ErrorMsg, "Invalid URL. Please, check it and try again.");
            AssertHasStyleClass(WsRobot.Url, "HasValidationProblem");
        }

        // bad url, non-parameterized
        await TestBadUrl("dasdasdasd");
        await TestBadUrl("ftp://192.168.0.2");
        // bad url, with variable
        // variable resolving will be checked in other tests
        await TestBadUrl("{{MyDomain}}");

        await WsRobot.Url.ClearAndTypeText("{{BaseUrlWs}}/{{WsHttp1Endpoint}}");
        AssertIsHidden(WsRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(WsRobot.Url, "HasValidationProblem");
    }

    private async Task TestHttpVersionValidation()
    {
        if (!AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS(2.0m, out _))
        {
            await WsRobot.SetHttpVersion(2.0m);
            AssertIsHidden(WsRobot.ErrorMsg);
            AssertDoesntHaveStyleClass(WsRobot.HttpVersion, "HasValidationProblem");
            await WsRobot.Url.ClearAndTypeText("{{BaseUrlWs}}/{{WsHttp2Endpoint}}");
            await WsRobot.Connect.ClickOn();
            AssertIsVisible(WsRobot.ErrorMsg);
            AssertHasText(WsRobot.ErrorMsg, "On Windows, support for HTTP/2 requires Windows 10 or greater.");
            AssertHasStyleClass(WsRobot.HttpVersion, "HasValidationProblem");
        }

        await WsRobot.SetHttpVersion(1.1m);
        AssertIsHidden(WsRobot.ErrorMsg);
        AssertDoesntHaveStyleClass(WsRobot.HttpVersion, "HasValidationProblem");
    }

    private async Task TestSelfSigned()
    {
        await TopMenuRobot.SwitchTlsVerification(true);
        await AssertTopMenuTlsVerification(true);

        await WsRobot.Url.ClearAndTypeText("wss://self-signed.badssl.com");
        await WsRobot.ClickOnConnectAndWaitForConnection();
        await Wait(5);

        AssertIsVisible(WsRobot.DisableTlsVerification);
        AssertIsVisible(WsRobot.ConnectionRequestException);
        AssertContainsText(WsRobot.ConnectionRequestException, "The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot");

        await WsRobot.DisableTlsVerification.ClickOn();
        AssertIsHidden(WsRobot.DisableTlsVerification);
        await AssertTopMenuTlsVerification(false);
        
        await WsRobot.ClickOnConnectAndWaitForConnection();
        AssertIsHidden(WsRobot.DisableTlsVerification);
        AssertIsVisible(WsRobot.ConnectionRequestException);
        AssertContainsText(WsRobot.ConnectionRequestException, "'101' was expected");
        
        await TopMenuRobot.SwitchTlsVerification(true);
    }

    private async Task AssertTopMenuTlsVerification(bool shouldBeEnabled)
    {
        TopMenuRobot.Options.Open();
        await UITestActions.WaitAfterActionAsync();
        if (shouldBeEnabled)
        {
            AssertHasIconVisible(TopMenuRobot.Options_EnableTlsVerification);
        }
        else
        {
            AssertHasIconHidden(TopMenuRobot.Options_EnableTlsVerification);
        }
        TopMenuRobot.Options.Close();
        await UITestActions.WaitAfterActionAsync();
    }

    private static ObservableCollection<VariableViewModel> GenerateCollectionVariables()
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "TestFilesDir", Path.Combine(GetTestFilesDirPath()), false)));
        parent.Add(new(parent, new(true, "SpecialValue1", "Plutônio", false)));
        parent.Add(new(parent, new(true, "WsHttp1Endpoint", "test/http1websocket", false)));
        parent.Add(new(parent, new(true, "WsHttp2Endpoint", "test/http2websocket", false)));
        return parent;
    }

    private static ObservableCollection<VariableViewModel> GenerateEnvironmentVariables()
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "BaseUrlWs", "wss://localhost:5001", false)));
        return parent;
    }

    private static string GetTestFilesDirPath()
    {
        var userDataDir = UserDataManager.GetUserDataFolder();
        return Path.Combine(userDataDir.FullName, "PororocaUserData", "TestFiles");
    }
}