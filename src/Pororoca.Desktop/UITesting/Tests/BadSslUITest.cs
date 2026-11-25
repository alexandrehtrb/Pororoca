using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class BadSslUITest : PororocaUITest
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

    public BadSslUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
    }

    public override async Task RunAsync(CancellationToken cancellationToken)
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
            // reenable TLS verification for BadSSL requests
            await TopMenuRobot.SwitchTlsVerification(true);
        }

        // badssl.com uses only HTTP/1.1
        await HttpRobot.SetHttpVersion(1.1m);
        try
        {
            AppendToLog("Running self-signed and client certificates tests (HTTP/1.1 only).");
            await TestSelfSigned(cancellationToken);
            await TestClientCertificatePkcs12Auth(cancellationToken);
            await TestClientCertificatePemConjoinedUnencryptedAuth(cancellationToken);
            await TestClientCertificatePemConjoinedEncryptedAuth(cancellationToken);
            await TestClientCertificatePemSeparateUnencryptedAuth(cancellationToken);
            await TestClientCertificatePemSeparateEncryptedAuth(cancellationToken);
        }
        catch (Exception ex)
        {
            // badssl.com sometimes is unstable, that is why we are wrapping with a try-catch
            AppendToLog("Bad SSL test failed.");
            AppendToLog(ex.ToString());
        }
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