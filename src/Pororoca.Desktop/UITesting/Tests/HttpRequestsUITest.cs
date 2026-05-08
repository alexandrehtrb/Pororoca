using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private static readonly ObservableCollection<VariableViewModel> defaultColVars = GenerateCollectionVariables();
    private static readonly ObservableCollection<VariableViewModel> defaultEnvVars = GenerateEnvironmentVariables();

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionScopedRequestHeadersRobot ColScopedReqHeadersRobot { get; }
    private CollectionVariablesRobot ColVarsRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }
    private readonly List<decimal> httpVersionsToTest;

    public HttpRequestsUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColScopedReqHeadersRobot = new(RootView.FindControl<CollectionScopedRequestHeadersView>("collectionScopedReqHeadersView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);

        this.httpVersionsToTest = new() { 1.1m };
        if (AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS(2.0m, out _))
        {
            this.httpVersionsToTest.Add(2.0m);
        }
        if (AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS(3.0m, out _))
        {
            this.httpVersionsToTest.Add(3.0m);
        }
    }

    public override async Task RunAsync(CancellationToken cancellationToken)
    {
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await ColRobot.SetCollectionScopedReqHeaders.ClickOn();
        await ColScopedReqHeadersRobot.SetRequestHeaders([
            new(true, "ColScopedHeaderName", "ColScopedHeaderValue")]);

        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.SetVariables(defaultColVars);

        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");
        await EnvRobot.SetVariables(defaultEnvVars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPREQ");

        if (OperatingSystem.IsLinux())
        {
            // we can't trust ASP.NET Core dev certs in Linux,
            // so we disable TLS verification for requests to our TestServer
            await TopMenuRobot.SwitchTlsVerification(false);
        }

        foreach (decimal version in this.httpVersionsToTest)
        {
            AppendToLog($"Selecting HTTP version {version}.");
            await HttpRobot.SetHttpVersion(version);

            await TestGetTextResponse(cancellationToken);
            await TestGetJsonResponse(cancellationToken);
            await TestGetBinaryResponse(cancellationToken);
            await TestHeaders(cancellationToken);
            await TestTrailers(cancellationToken);
            await TestPostEmptyBody(cancellationToken);
            await TestPostRawJsonBody(cancellationToken);
            await TestPostRawTextBody(cancellationToken);
            await TestPostFileBody(cancellationToken);
            await TestPostUrlEncodedBody(cancellationToken);
            await TestPostFormDataBody(cancellationToken);
            await TestBasicAuth(cancellationToken);
            await TestBearerAuth(cancellationToken);
        }
    }

    private void AssertContainsResponseHeader(string key)
    {
        var vm = ((HttpRequestViewModel)HttpRobot.RootView!.DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
        AssertCondition(vm.Items.Any(h => h.Key == key));
    }

    private void AssertContainsResponseHeader(string key, string value)
    {
        var vm = ((HttpRequestViewModel)HttpRobot.RootView!.DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
        AssertCondition(vm.Items.Any(h => h.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && h.Value == value));
    }

    private static ObservableCollection<VariableViewModel> GenerateCollectionVariables()
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "ClientCertificatesDir", Path.Combine(GetTestFilesDirPath(), "ClientCertificates"), false)));
        parent.Add(new(parent, new(true, "TestFilesDir", Path.Combine(GetTestFilesDirPath()), false)));
        parent.Add(new(parent, new(true, "SpecialHeaderKey", "Header2", false)));
        parent.Add(new(parent, new(true, "SpecialHeaderValue", "ciao", false)));
        parent.Add(new(parent, new(true, "SpecialValue1", "Tailândia", false)));
        return parent;
    }

    private static ObservableCollection<VariableViewModel> GenerateEnvironmentVariables()
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "BaseUrl", "https://localhost:5001", false)));
        parent.Add(new(parent, new(true, "BaseUrlWs", "wss://localhost:5001", false)));
        parent.Add(new(parent, new(true, "BadSslSelfSignedTestsUrl", "https://self-signed.badssl.com/", false)));
        parent.Add(new(parent, new(true, "BadSslClientCertTestsUrl", "https://client.badssl.com", false)));
        parent.Add(new(parent, new(true, "BadSslClientCertFilePassword", "badssl.com", false)));
        parent.Add(new(parent, new(true, "BasicAuthLogin", "usr", false)));
        parent.Add(new(parent, new(true, "BasicAuthPassword", "pwd", false)));
        parent.Add(new(parent, new(true, "BearerAuthToken", "token_local", false)));
        return parent;
    }
}