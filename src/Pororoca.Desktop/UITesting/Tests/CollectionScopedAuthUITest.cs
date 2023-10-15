using System.Collections.ObjectModel;
using System.Drawing.Text;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.UserData;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class CollectionScopedAuthUITest : UITest
{
    private static readonly ObservableCollection<VariableViewModel> defaultColVars = GenerateCollectionVariables();
    private static readonly ObservableCollection<VariableViewModel> defaultEnvVars = GenerateEnvironmentVariables();

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionVariablesRobot ColVarsRobot { get; }
    private CollectionScopedAuthRobot ColAuthRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }

    public CollectionScopedAuthUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        ColAuthRobot = new(RootView.FindControl<CollectionScopedAuthView>("collectionScopedAuthView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
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

        if (OperatingSystem.IsLinux())
        {
            // we can't trust ASP.NET Core dev certs in Linux,
            // so we disable TLS verification for requests to our TestServer
            await TopMenuRobot.SwitchTlsVerification(false);
        }

        await HttpRobot.SetHttpVersion(1.1m);
        await TestBearerAuthInherited();

        if (OperatingSystem.IsLinux())
        {
            // reenable TLS verification for BadSSL requests
            await TopMenuRobot.SwitchTlsVerification(true);
        }

        // badssl.com uses only HTTP/1.1
        await HttpRobot.SetHttpVersion(1.1m);
        try
        {
            await TestClientCertificatePkcs12AuthInherited();
        }
        catch (Exception ex)
        {
            // badssl.com sometimes is unstable, that is why we are wrapping with a try-catch
            AppendToLog("Bad SSL test failed.");
            AppendToLog(ex.ToString());
        }
    }

    private async Task TestBearerAuthInherited()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/auth");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetInheritFromCollectionAuth();

        await TreeRobot.Select("COL1/AUTH");
        await ColAuthRobot.Auth.SetBearerAuth("{{BearerAuthToken}}");
        
        await TreeRobot.Select("COL1/HTTPREQ");
        await HttpRobot.ClickOnSendAndWaitForResponse();

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertHasText(HttpRobot.ResBodyRawContent, "Bearer token_local");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }

    private async Task TestClientCertificatePkcs12AuthInherited()
    {
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BadSslClientCertTestsUrl}}");
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetInheritFromCollectionAuth();

        await TreeRobot.Select("COL1/AUTH");
        await ColAuthRobot.Auth.SetPkcs12CertificateAuth("{{ClientCertificatesDir}}/badssl.com-client.p12", "{{BadSslClientCertFilePassword}}");

        await TreeRobot.Select("COL1/HTTPREQ");
        await HttpRobot.ClickOnSendAndWaitForResponse();
        await Wait(3);

        AssertContainsText(HttpRobot.ResTitle, "Response: 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        //AssertContainsResponseHeader("Content-Type", "text/plain; charset=utf-8");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);
        AssertContainsText(HttpRobot.ResBodyRawContent, "<html>");
        AssertIsVisible(HttpRobot.ResBodySaveToFile);
    }

    private void AssertContainsResponseHeader(string key)
    {
        var vm = ((HttpRequestViewModel)HttpRobot.RootView!.DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
        Assert(vm.Items.Any(h => h.Key == key));
    }

    private void AssertContainsResponseHeader(string key, string value)
    {
        var vm = ((HttpRequestViewModel)HttpRobot.RootView!.DataContext!).ResponseDataCtx.ResponseHeadersAndTrailersTableVm;
        Assert(vm.Items.Any(h => h.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && h.Value == value));
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

    private static string GetTestFilesDirPath()
    {
        var userDataDir = UserDataManager.GetUserDataFolder();
        return Path.Combine(userDataDir.FullName, "PororocaUserData", "TestFiles");
    }
}