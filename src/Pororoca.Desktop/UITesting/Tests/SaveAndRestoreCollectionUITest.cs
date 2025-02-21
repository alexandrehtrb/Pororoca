using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.ExportCollection;
using Pororoca.Domain.Features.ImportCollection;

namespace Pororoca.Desktop.UITesting.Tests;

public class SaveAndRestoreCollectionUITest : PororocaUITest
{
    private static readonly ObservableCollection<VariableViewModel> defaultColVars = GenerateCollectionVariables();

    private static readonly ObservableCollection<VariableViewModel> defaultEnvVars = GenerateEnvironmentVariables("env");

    protected static readonly PororocaKeyValueParam[] headers =
    [
        new(false, "Header1", "ValueHeader1"),
        new(true, "Header1", "Header1Value"),
        new(true, "oi_{{SpecialHeaderKey}}", "oi-{{SpecialHeaderValue}}"),
    ];

    private static readonly PororocaKeyValueParam[] urlEncodedParams =
    [
        new(true, "a", "xyz"),
        new(true, "b", "123"),
        new(false, "c", "false"),
        new(true, "c", "true"),
        new(true, "myIdSecret", "{{SpecialValue1}}")
    ];

    private static readonly PororocaHttpRequestFormDataParam[] formDataParams =
    [
        PororocaHttpRequestFormDataParam.MakeTextParam(true, "a", "xyz{{SpecialValue1}}", "text/plain"),
        PororocaHttpRequestFormDataParam.MakeTextParam(true, "b", "[]", "application/json"),
        PororocaHttpRequestFormDataParam.MakeFileParam(true, "arq", "{{TestFilesDir}}/arq.txt", "text/plain")
    ];

    protected Control RootView { get; }
    protected TopMenuRobot TopMenuRobot { get; }
    protected CollectionsGroupView CollectionsGroup { get; }
    protected ItemsTreeRobot TreeRobot { get; }
    protected CollectionRobot ColRobot { get; }
    protected ExportCollectionRobot ExportColRobot { get; }
    protected CollectionVariablesRobot ColVarsRobot { get; }
    protected CollectionScopedAuthRobot ColAuthRobot { get; }
    protected CollectionFolderRobot DirRobot { get; }
    protected EnvironmentRobot EnvRobot { get; }
    protected ExportEnvironmentRobot ExportEnvRobot { get; }
    protected HttpRequestRobot HttpRobot { get; }
    private WebSocketConnectionRobot WsRobot { get; }
    private WebSocketClientMessageRobot WsMsgRobot { get; }
    private HttpRepeaterRobot RepeaterRobot { get; }

    public SaveAndRestoreCollectionUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        CollectionsGroup = RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!;
        TreeRobot = new(CollectionsGroup);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ExportColRobot = new(RootView.FindControl<ExportCollectionView>("exportCollectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        ColAuthRobot = new(RootView.FindControl<CollectionScopedAuthView>("collectionScopedAuthView")!);
        DirRobot = new(RootView.FindControl<CollectionFolderView>("collectionFolderView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        ExportEnvRobot = new(RootView.FindControl<ExportEnvironmentView>("exportEnvironmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
        WsRobot = new(RootView.FindControl<WebSocketConnectionView>("wsConnView")!);
        WsMsgRobot = new(RootView.FindControl<WebSocketClientMessageView>("wsClientMsgView")!);
        RepeaterRobot = new(RootView.FindControl<HttpRepeaterView>("httpRepView")!);
    }

    public override async Task RunAsync()
    {
        await CreateCollectionWithDifferentItems();

        await SaveAndRestoreCollection();

        await AssertCollectionReimportedSuccessfully();
    }

    private async Task CreateCollectionWithDifferentItems()
    {
        await TopMenuRobot.CreateNewCollection();

        await ColRobot.Name.Edit("COL1");
        await TreeRobot.Select("COL1/VARS");
        await ColVarsRobot.SetVariables(defaultColVars);

        await TreeRobot.Select("COL1/AUTH");
        await ColAuthRobot.Auth.SetBearerAuth("token");

        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");
        await EnvRobot.SetVariables(defaultEnvVars);
        await EnvRobot.SetAsCurrentEnvironment.ClickOn();

        await TreeRobot.Select("COL1");
        await ColRobot.AddFolder.ClickOn();
        await DirRobot.Name.Edit("DIR1");

        await TreeRobot.Select("COL1/DIR1");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPNONEBASICAUTH");
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/auth");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetBasicAuth("{{BasicAuthLogin}}", "{{BasicAuthPassword}}");

        await TreeRobot.Select("COL1/DIR1");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPNONEBEARERAUTH");
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/auth");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetBearerAuth("{{BearerAuthToken}}");

        await TreeRobot.Select("COL1/DIR1");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPNONEWINDOWSAUTH");
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/auth");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetWindowsAuthOtherUser("{{WindowsAuthLogin}}", "{{WindowsAuthPassword}}", "{{WindowsAuthDomain}}");

        await TreeRobot.Select("COL1/DIR1");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPNONEPKCS12AUTH");
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BadSslClientCertTestsUrl}}");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetPkcs12CertificateAuth("{{ClientCertificatesDir}}/badssl.com-client.p12", "{{BadSslClientCertFilePassword}}");

        await TreeRobot.Select("COL1/DIR1");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPFORMDATA");
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/multipartformdata");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetFormDataBody(formDataParams);

        await TreeRobot.Select("COL1/DIR1");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPGRAPHQL");
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/graphql");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetGraphQlBody("query", "variables");

        await TreeRobot.Select("COL1/DIR1");
        await DirRobot.AddFolder.ClickOn();
        await DirRobot.Name.Edit("DIR2");

        await TreeRobot.Select("COL1/DIR1/DIR2");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPRAW");
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/json");
        await HttpRobot.SetHttpVersion(2.0m);
        await HttpRobot.SetRawBody("application/json", "{\"myValue\":\"{{SpecialValue1}}\"}");

        await TreeRobot.Select("COL1/DIR1/DIR2");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPFILE");
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/file");
        await HttpRobot.SetHttpVersion(2.0m);
        await HttpRobot.SetFileBody("image/jpeg", "{{TestFilesDir}}/homem_aranha.jpg");

        await TreeRobot.Select("COL1/DIR1/DIR2");
        await DirRobot.AddWebSocket.ClickOn();
        await WsRobot.Name.Edit("WS");
        await WsRobot.Url.ClearAndTypeText("{{BaseUrlWs}}/{{WsHttp2Endpoint}}");
        await WsRobot.SetHttpVersion(2.0m);

        await TreeRobot.Select("COL1/DIR1/DIR2/WS");
        await WsRobot.AddClientMessage.ClickOn();
        await WsMsgRobot.Name.Edit("WSMSGJSON");
        await WsMsgRobot.SetRawJsonContent("{\"elemento\":\"{{SpecialValue1}}\"}");

        await TreeRobot.Select("COL1/DIR1/DIR2/WS");
        await WsRobot.AddClientMessage.ClickOn();
        await WsMsgRobot.Name.Edit("WSMSGFILE");
        await WsMsgRobot.SetFileBinaryContent("{{TestFilesDir}}/homem_aranha.jpg");

        await TreeRobot.Select("COL1/DIR1/DIR2");
        await DirRobot.AddRepeater.ClickOn();
        await RepeaterRobot.Name.Edit("REPSEQUENTIAL");
        await RepeaterRobot.BaseHttpRequest.Select("DIR1/DIR2/HTTPFILE");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        await RepeaterRobot.RepetitionMode.Select(RepeaterRobot.OptionRepetitionModeSequential);
        await RepeaterRobot.DelayInMs.SetValue(0);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        await RepeaterRobot.InputDataType.Select(RepeaterRobot.OptionInputDataTypeRaw);
        await RepeaterRobot.InputDataRawEditor.ClearAndTypeText("[{\"V1\":\"1\",\"VA\":\"A\"}]");

        await TreeRobot.Select("COL1/DIR1/DIR2");
        await DirRobot.AddFolder.ClickOn();
        await DirRobot.Name.Edit("DIR3");

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPURLENCODED");
        await HttpRobot.HttpMethod.Select("POST");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/post/urlencoded");
        await HttpRobot.SetHttpVersion(3.0m);
        await HttpRobot.SetUrlEncodedBody(urlEncodedParams);

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3");
        await DirRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPNONEPEMAUTH");
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BadSslClientCertTestsUrl}}");
        await HttpRobot.SetHttpVersion(1.1m);
        await HttpRobot.SetEmptyBody();
        await HttpRobot.SetPemCertificateAuth("{{ClientCertificatesDir}}/badssl.com-client-certificate-without-private-key.pem",
                                              "{{ClientCertificatesDir}}/badssl.com-client-encrypted-private-key.key",
                                              "{{BadSslClientCertFilePassword}}");

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3");
        await DirRobot.AddRepeater.ClickOn();
        await RepeaterRobot.Name.Edit("REPRANDOM");
        await RepeaterRobot.BaseHttpRequest.Select("DIR1/DIR2/DIR3/HTTPNONEPEMAUTH");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        await RepeaterRobot.RepetitionMode.Select(RepeaterRobot.OptionRepetitionModeRandom);
        await RepeaterRobot.NumberOfRepetitions.SetValue(25);
        await RepeaterRobot.MaxDop.SetValue(3);
        await RepeaterRobot.DelayInMs.SetValue(10);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        await RepeaterRobot.InputDataType.Select(RepeaterRobot.OptionInputDataTypeFile);
        await RepeaterRobot.InputDataFileSrcPath.ClearAndTypeText("inputdata.json");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTPHEADERS");
        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/headers");
        await HttpRobot.SetHttpVersion(1.0m);
        await HttpRobot.SetRequestHeaders(headers);
        await HttpRobot.SetEmptyBody();

        await TreeRobot.Select("COL1");
        await ColRobot.AddRepeater.ClickOn();
        await RepeaterRobot.Name.Edit("REPSIMPLE");
        await RepeaterRobot.BaseHttpRequest.Select("HTTPHEADERS");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        await RepeaterRobot.RepetitionMode.Select(RepeaterRobot.OptionRepetitionModeSimple);
        await RepeaterRobot.NumberOfRepetitions.SetValue(200);
        await RepeaterRobot.MaxDop.SetValue(10);
        await RepeaterRobot.DelayInMs.SetValue(12);

        /* collection:
        COL1
            VARS
            ENVS
                ENV1
            DIR1
                DIR2
                    DIR3
                        HTTPURLENCODED
                        HTTPNONEPEMAUTH
                        REPRANDOM
                    HTTPRAW
                    HTTPFILE
                    WS
                        WSMSGJSON
                        WSMSGFILE
                    REPSEQUENTIAL
                HTTPNONEBASICAUTH
                HTTPNONEBEARERAUTH
                HTTPNONEWINDOWSAUTH
                HTTPNONEPKCS12AUTH
                HTTPFORMDATA
                HTTPGRAPHQL
            HTTPHEADERS
            REPSIMPLE
        */

        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/AUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEBASICAUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEBEARERAUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEWINDOWSAUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEPKCS12AUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPFORMDATA");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPGRAPHQL");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/HTTPRAW");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/HTTPFILE");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/WS");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/WS/WSMSGJSON");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/WS/WSMSGFILE");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/REPSEQUENTIAL");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3/HTTPNONEPEMAUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3/REPRANDOM");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTPHEADERS");
        CollectionsGroup.AssertTreeItemExists("COL1/REPSIMPLE");
    }

    private async Task SaveAndRestoreCollection()
    {
        await TreeRobot.Select("COL1");
        var collection = ((CollectionViewModel)ColRobot.RootView!.DataContext!).ToCollection(forExporting: false);
        MemoryStream colMs = new(16384);
        PororocaCollectionExporter.ExportAsPororocaCollection(colMs, collection);
        colMs.Seek(0, SeekOrigin.Begin);
        MainWindowVm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        MainWindowVm.CollectionsGroupViewDataCtx.Items.Clear();

        CollectionsGroup.AssertTreeItemNotExists("COL1");

        var reimportedCollection = await PororocaCollectionImporter.ImportPororocaCollectionAsync(colMs, preserveId: true);
        AssertCondition(reimportedCollection is not null);
        MainWindowVm.AddCollection(reimportedCollection!, showItemInScreen: true);
    }

    private async Task AssertCollectionReimportedSuccessfully()
    {
        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/AUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEBASICAUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEBEARERAUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEWINDOWSAUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPNONEPKCS12AUTH");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPFORMDATA");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/HTTPGRAPHQL");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/HTTPRAW");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/HTTPFILE");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/WS");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/WS/WSMSGJSON");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/WS/WSMSGFILE");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/REPSEQUENTIAL");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR2/DIR3/REPRANDOM");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTPHEADERS");
        CollectionsGroup.AssertTreeItemExists("COL1/REPSIMPLE");

        await TreeRobot.Select("COL1");
        ColRobot.RootView.AssertIsVisible();

        await TreeRobot.Select("COL1/AUTH");
        ColAuthRobot.RootView.AssertIsVisible();
        ColAuthRobot.Auth.AuthType.AssertSelection(ColAuthRobot.Auth.AuthTypeOptionBearer);
        ColAuthRobot.Auth.BearerAuthToken.AssertIsVisible();
        ColAuthRobot.Auth.BearerAuthToken.AssertHasText("token");
        ColAuthRobot.Auth.BasicAuthLogin.AssertIsHidden();
        ColAuthRobot.Auth.ClientCertificateType.AssertIsHidden();

        await TreeRobot.Select("COL1/VARS");
        ColVarsRobot.RootView.AssertIsVisible();
        var expectedColVars = defaultColVars.Select(x => x.ToVariable()).ToList();
        var colVars = ColVarsRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToList();
        AssertCondition(Enumerable.SequenceEqual(expectedColVars, colVars));

        await TreeRobot.Select("COL1/ENVS/ENV1");
        EnvRobot.RootView.AssertIsVisible();
        var expectedEnvVars = defaultEnvVars.Select(x => x.ToVariable()).ToList();
        var envVars = EnvRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToList();
        AssertCondition(Enumerable.SequenceEqual(expectedEnvVars, envVars));

        await TreeRobot.Select("COL1/DIR1");
        DirRobot.RootView.AssertIsVisible();

        await TreeRobot.Select("COL1/DIR1/HTTPNONEBASICAUTH");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("GET");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/auth");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionBasic);
        HttpRobot.Auth.BasicAuthLogin.AssertIsVisible();
        HttpRobot.Auth.BasicAuthPassword.AssertIsVisible();
        HttpRobot.Auth.BasicAuthLogin.AssertHasText("{{BasicAuthLogin}}");
        HttpRobot.Auth.BasicAuthPassword.AssertHasText("{{BasicAuthPassword}}");
        HttpRobot.Auth.BearerAuthToken.AssertIsHidden();
        HttpRobot.Auth.ClientCertificateType.AssertIsHidden();

        await TreeRobot.Select("COL1/DIR1/HTTPNONEBEARERAUTH");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("GET");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/auth");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionBearer);
        HttpRobot.Auth.BearerAuthToken.AssertIsVisible();
        HttpRobot.Auth.BearerAuthToken.AssertHasText("{{BearerAuthToken}}");
        HttpRobot.Auth.BasicAuthLogin.AssertIsHidden();
        HttpRobot.Auth.ClientCertificateType.AssertIsHidden();

        await TreeRobot.Select("COL1/DIR1/HTTPNONEWINDOWSAUTH");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("GET");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/auth");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionWindows);
        HttpRobot.Auth.BasicAuthLogin.AssertIsHidden();
        HttpRobot.Auth.BearerAuthToken.AssertIsHidden();
        HttpRobot.Auth.WindowsAuthUseCurrentUser.AssertIsNotChecked();
        HttpRobot.Auth.WindowsAuthLogin.AssertHasText("{{WindowsAuthLogin}}");
        HttpRobot.Auth.WindowsAuthPassword.AssertHasText("{{WindowsAuthPassword}}");
        HttpRobot.Auth.WindowsAuthDomain.AssertHasText("{{WindowsAuthDomain}}");

        await TreeRobot.Select("COL1/DIR1/HTTPNONEPKCS12AUTH");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("GET");
        HttpRobot.Url.AssertHasText("{{BadSslClientCertTestsUrl}}");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionClientCertificate);
        HttpRobot.Auth.ClientCertificateType.AssertSelection(HttpRobot.Auth.ClientCertificateTypeOptionPkcs12);
        HttpRobot.Auth.ClientCertificatePkcs12FilePath.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePkcs12FilePassword.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePkcs12FilePath.AssertHasText("{{ClientCertificatesDir}}/badssl.com-client.p12");
        HttpRobot.Auth.ClientCertificatePkcs12FilePassword.AssertHasText("{{BadSslClientCertFilePassword}}");
        HttpRobot.Auth.BasicAuthLogin.AssertIsHidden();
        HttpRobot.Auth.BearerAuthToken.AssertIsHidden();
        HttpRobot.Auth.WindowsAuthUseCurrentUser.AssertIsHidden();

        await TreeRobot.Select("COL1/DIR1/HTTPFORMDATA");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("POST");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/post/multipartformdata");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionFormData);
        HttpRobot.ReqBodyFormDataParams.AssertIsVisible();
        var actualFormDataParams = HttpRobot.FormDataParamsVm.Items.Select(x => x.ToFormDataParam()).ToArray();
        AssertCondition(Enumerable.SequenceEqual(formDataParams, actualFormDataParams));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/HTTPGRAPHQL");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("POST");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/post/graphql");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionGraphQl);
        HttpRobot.ReqBodyGraphQlQuery.AssertIsVisible();
        HttpRobot.ReqBodyGraphQlVariables.AssertIsVisible();
        HttpRobot.ReqBodyGraphQlQuery.AssertHasText("query");
        HttpRobot.ReqBodyGraphQlVariables.AssertHasText("variables");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2");
        DirRobot.RootView.AssertIsVisible();

        await TreeRobot.Select("COL1/DIR1/DIR2/HTTPRAW");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("POST");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/post/json");
        HttpRobot.HttpVersion.AssertHasText("HTTP/2");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionRaw);
        HttpRobot.ReqBodyRawContentType.AssertIsVisible();
        HttpRobot.ReqBodyRawContent.AssertIsVisible();
        HttpRobot.ReqBodyRawContentType.AssertHasText("application/json");
        HttpRobot.ReqBodyRawContent.AssertHasText("{\"myValue\":\"{{SpecialValue1}}\"}");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2/HTTPFILE");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("POST");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/post/file");
        HttpRobot.HttpVersion.AssertHasText("HTTP/2");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionFile);
        HttpRobot.ReqBodyFileContentType.AssertIsVisible();
        HttpRobot.ReqBodyFileSrcPath.AssertIsVisible();
        HttpRobot.ReqBodyFileContentType.AssertHasText("image/jpeg");
        HttpRobot.ReqBodyFileSrcPath.AssertHasText("{{TestFilesDir}}/homem_aranha.jpg");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2/WS");
        WsRobot.RootView.AssertIsVisible();
        WsRobot.Url.AssertHasText("{{BaseUrlWs}}/{{WsHttp2Endpoint}}");
        WsRobot.HttpVersion.AssertHasText("HTTP/2");

        await TreeRobot.Select("COL1/DIR1/DIR2/WS/WSMSGJSON");
        WsMsgRobot.RootView.AssertIsVisible();
        WsMsgRobot.MessageType.AssertSelection(WsMsgRobot.MessageTypeOptionText);
        WsMsgRobot.ContentMode.AssertSelection(WsMsgRobot.ContentModeOptionRaw);
        WsMsgRobot.ContentRawSyntax.AssertSelection(WsMsgRobot.ContentRawSyntaxOptionJson);
        WsMsgRobot.ContentRaw.AssertHasText("{\"elemento\":\"{{SpecialValue1}}\"}");
        WsMsgRobot.ContentFileSrcPath.AssertIsHidden();
        WsMsgRobot.ContentRaw.AssertIsVisible();

        await TreeRobot.Select("COL1/DIR1/DIR2/WS/WSMSGFILE");
        WsMsgRobot.RootView.AssertIsVisible();
        WsMsgRobot.MessageType.AssertSelection(WsMsgRobot.MessageTypeOptionBinary);
        WsMsgRobot.ContentMode.AssertSelection(WsMsgRobot.ContentModeOptionFile);
        WsMsgRobot.ContentFileSrcPath.AssertHasText("{{TestFilesDir}}/homem_aranha.jpg");
        WsMsgRobot.ContentFileSrcPath.AssertIsVisible();
        WsMsgRobot.ContentRaw.AssertIsHidden();

        await TreeRobot.Select("COL1/DIR1/DIR2/REPSEQUENTIAL");
        RepeaterRobot.RootView.AssertIsVisible();
        RepeaterRobot.BaseHttpRequest.AssertSelection("DIR1/DIR2/HTTPFILE");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        RepeaterRobot.DelayInMs.AssertNumericValue(0);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        RepeaterRobot.InputDataType.AssertSelection(RepeaterRobot.OptionInputDataTypeRaw);
        RepeaterRobot.InputDataRawEditor.AssertHasText("[{\"V1\":\"1\",\"VA\":\"A\"}]");

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3");
        DirRobot.RootView.AssertIsVisible();

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("POST");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/post/urlencoded");
        HttpRobot.HttpVersion.AssertHasText("HTTP/3");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionUrlEncoded);
        HttpRobot.ReqBodyUrlEncodedParams.AssertIsVisible();
        var actualUrlEncodedParams = HttpRobot.UrlEncodedParamsVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        AssertCondition(Enumerable.SequenceEqual(urlEncodedParams, actualUrlEncodedParams));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3/HTTPNONEPEMAUTH");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("GET");
        HttpRobot.Url.AssertHasText("{{BadSslClientCertTestsUrl}}");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionClientCertificate);
        HttpRobot.Auth.ClientCertificateType.AssertSelection(HttpRobot.Auth.ClientCertificateTypeOptionPem);
        HttpRobot.Auth.ClientCertificatePemCertificateFilePath.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePemPrivateKeyFilePath.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePemPrivateKeyPassword.AssertIsVisible();
        HttpRobot.Auth.ClientCertificatePemCertificateFilePath.AssertHasText("{{ClientCertificatesDir}}/badssl.com-client-certificate-without-private-key.pem");
        HttpRobot.Auth.ClientCertificatePemPrivateKeyFilePath.AssertHasText("{{ClientCertificatesDir}}/badssl.com-client-encrypted-private-key.key");
        HttpRobot.Auth.ClientCertificatePemPrivateKeyPassword.AssertHasText("{{BadSslClientCertFilePassword}}");

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3/REPRANDOM");
        RepeaterRobot.RootView.AssertIsVisible();
        RepeaterRobot.BaseHttpRequest.AssertSelection("DIR1/DIR2/DIR3/HTTPNONEPEMAUTH");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        RepeaterRobot.NumberOfRepetitions.AssertNumericValue(25);
        RepeaterRobot.MaxDop.AssertNumericValue(3);
        RepeaterRobot.DelayInMs.AssertNumericValue(10);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        RepeaterRobot.InputDataType.AssertSelection(RepeaterRobot.OptionInputDataTypeFile);
        RepeaterRobot.InputDataFileSrcPath.AssertHasText("inputdata.json");

        await TreeRobot.Select("COL1/HTTPHEADERS");
        HttpRobot.RootView.AssertIsVisible();
        HttpRobot.HttpMethod.AssertHasText("GET");
        HttpRobot.Url.AssertHasText("{{BaseUrl}}/test/get/headers");
        HttpRobot.HttpVersion.AssertHasText("HTTP/1.0");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        HttpRobot.ReqBodyMode.AssertSelection(HttpRobot.ReqBodyModeOptionNone);
        var actualHeaders = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        AssertCondition(Enumerable.SequenceEqual(headers, actualHeaders));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        HttpRobot.Auth.AuthType.AssertSelection(HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/REPSIMPLE");
        RepeaterRobot.RootView.AssertIsVisible();
        RepeaterRobot.BaseHttpRequest.AssertSelection("HTTPHEADERS");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        RepeaterRobot.NumberOfRepetitions.AssertNumericValue(200);
        RepeaterRobot.MaxDop.AssertNumericValue(10);
        RepeaterRobot.DelayInMs.AssertNumericValue(12);
    }

    protected static ObservableCollection<VariableViewModel> GenerateCollectionVariables(bool secretsCleared = false)
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "colVarNotSecret", "valueNotSecret", false)));
        parent.Add(new(parent, new(true, "colVarSecret", secretsCleared ? string.Empty : "valueSecret", true)));
        parent.Add(new(parent, new(false, "colVarNotSecret", "valueNotSecret", false)));
        parent.Add(new(parent, new(false, "colVarSecret", secretsCleared ? string.Empty : "valueSecret", true)));
        return parent;
    }

    protected static ObservableCollection<VariableViewModel> GenerateEnvironmentVariables(string envName, bool secretsCleared = false)
    {
        ObservableCollection<VariableViewModel> parent = new();
        parent.Add(new(parent, new(true, "envVarNotSecret", envName + "-notSecret", false)));
        parent.Add(new(parent, new(true, "envVarSecret", secretsCleared ? string.Empty : (envName + "-secret"), true)));
        parent.Add(new(parent, new(false, "envVarNotSecret", envName + "-notSecret", false)));
        parent.Add(new(parent, new(false, "envVarSecret", secretsCleared ? string.Empty : (envName + "-secret"), true)));
        return parent;
    }
}