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

public sealed class ExportAndImportUITest : UITest
{
    private static readonly ObservableCollection<VariableViewModel> defaultColVars = GenerateCollectionVariables();

    private static readonly ObservableCollection<VariableViewModel> defaultEnvVars = GenerateEnvironmentVariables();

    private static readonly PororocaKeyValueParam[] headers =
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

    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private CollectionsGroupView CollectionsGroup { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionVariablesRobot ColVarsRobot { get; }
    private CollectionScopedAuthRobot ColAuthRobot { get; }
    private CollectionFolderRobot DirRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }
    private WebSocketConnectionRobot WsRobot { get; }
    private WebSocketClientMessageRobot WsMsgRobot { get; }
    private HttpRepeaterRobot RepeaterRobot { get; }

    public ExportAndImportUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        CollectionsGroup = RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!;
        TreeRobot = new(CollectionsGroup);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        ColAuthRobot = new(RootView.FindControl<CollectionScopedAuthView>("collectionScopedAuthView")!);
        DirRobot = new(RootView.FindControl<CollectionFolderView>("collectionFolderView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
        WsRobot = new(RootView.FindControl<WebSocketConnectionView>("wsConnView")!);
        WsMsgRobot = new(RootView.FindControl<WebSocketClientMessageView>("wsClientMsgView")!);
        RepeaterRobot = new(RootView.FindControl<HttpRepeaterView>("httpRepView")!);
    }

    public override async Task RunAsync()
    {
        await CreateCollectionWithDifferentItems();

        await ExportAndReimportCollection();

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

        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/AUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEBASICAUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEBEARERAUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEWINDOWSAUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEPKCS12AUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPFORMDATA");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPGRAPHQL");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/HTTPRAW");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/HTTPFILE");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/WS");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/WS/WSMSGJSON");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/WS/WSMSGFILE");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/REPSEQUENTIAL");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3/HTTPNONEPEMAUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3/REPRANDOM");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTPHEADERS");
        AssertTreeItemExists(CollectionsGroup, "COL1/REPSIMPLE");
    }

    private async Task ExportAndReimportCollection()
    {
        await TreeRobot.Select("COL1");
        var collection = ((CollectionViewModel)ColRobot.RootView!.DataContext!).ToCollection();
        string json = PororocaCollectionExporter.ExportAsPororocaCollection(collection, shouldHideSecrets: false);
        var mwvm = ((MainWindowViewModel)MainWindow.Instance!.DataContext!);
        mwvm.CollectionsGroupViewDataCtx.CollectionGroupSelectedItem = null;
        mwvm.CollectionsGroupViewDataCtx.Items.Clear();

        AssertTreeItemNotExists(CollectionsGroup, "COL1");

        Assert(PororocaCollectionImporter.TryImportPororocaCollection(json, preserveId: true, out var reimportedCollection));
        mwvm.AddCollection(reimportedCollection!, showItemInScreen: true);
    }

    private async Task AssertCollectionReimportedSuccessfully()
    {
        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/AUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEBASICAUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEBEARERAUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEWINDOWSAUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPNONEPKCS12AUTH");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPFORMDATA");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/HTTPGRAPHQL");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/HTTPRAW");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/HTTPFILE");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/WS");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/WS/WSMSGJSON");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/WS/WSMSGFILE");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/REPSEQUENTIAL");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR2/DIR3/REPRANDOM");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTPHEADERS");
        AssertTreeItemExists(CollectionsGroup, "COL1/REPSIMPLE");

        await TreeRobot.Select("COL1");
        AssertIsVisible(ColRobot.RootView);

        await TreeRobot.Select("COL1/AUTH");
        AssertIsVisible(ColAuthRobot.RootView);
        AssertSelection(ColAuthRobot.Auth.AuthType, ColAuthRobot.Auth.AuthTypeOptionBearer);
        AssertIsVisible(ColAuthRobot.Auth.BearerAuthToken);
        AssertHasText(ColAuthRobot.Auth.BearerAuthToken, "token");
        AssertIsHidden(ColAuthRobot.Auth.BasicAuthLogin);
        AssertIsHidden(ColAuthRobot.Auth.ClientCertificateType);

        await TreeRobot.Select("COL1/VARS");
        AssertIsVisible(ColVarsRobot.RootView);
        var expectedColVars = defaultColVars.Select(x => x.ToVariable()).ToList();
        var colVars = ColVarsRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToList();
        Assert(Enumerable.SequenceEqual(expectedColVars, colVars));

        await TreeRobot.Select("COL1/ENVS/ENV1");
        AssertIsVisible(EnvRobot.RootView);
        var expectedEnvVars = defaultEnvVars.Select(x => x.ToVariable()).ToList();
        var envVars = EnvRobot.VariablesVm.Items.Select(x => x.ToVariable()).ToList();
        Assert(Enumerable.SequenceEqual(expectedEnvVars, envVars));

        await TreeRobot.Select("COL1/DIR1");
        AssertIsVisible(DirRobot.RootView);

        await TreeRobot.Select("COL1/DIR1/HTTPNONEBASICAUTH");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "GET");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/auth");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionBasic);
        AssertIsVisible(HttpRobot.Auth.BasicAuthLogin);
        AssertIsVisible(HttpRobot.Auth.BasicAuthPassword);
        AssertHasText(HttpRobot.Auth.BasicAuthLogin, "{{BasicAuthLogin}}");
        AssertHasText(HttpRobot.Auth.BasicAuthPassword, "{{BasicAuthPassword}}");
        AssertIsHidden(HttpRobot.Auth.BearerAuthToken);
        AssertIsHidden(HttpRobot.Auth.ClientCertificateType);

        await TreeRobot.Select("COL1/DIR1/HTTPNONEBEARERAUTH");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "GET");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/auth");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionBearer);
        AssertIsVisible(HttpRobot.Auth.BearerAuthToken);
        AssertHasText(HttpRobot.Auth.BearerAuthToken, "{{BearerAuthToken}}");
        AssertIsHidden(HttpRobot.Auth.BasicAuthLogin);
        AssertIsHidden(HttpRobot.Auth.ClientCertificateType);

        await TreeRobot.Select("COL1/DIR1/HTTPNONEWINDOWSAUTH");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "GET");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/auth");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionWindows);
        AssertIsHidden(HttpRobot.Auth.BasicAuthLogin);
        AssertIsHidden(HttpRobot.Auth.BearerAuthToken);
        AssertIsNotChecked(HttpRobot.Auth.WindowsAuthUseCurrentUser);
        AssertHasText(HttpRobot.Auth.WindowsAuthLogin, "{{WindowsAuthLogin}}");
        AssertHasText(HttpRobot.Auth.WindowsAuthPassword, "{{WindowsAuthPassword}}");
        AssertHasText(HttpRobot.Auth.WindowsAuthDomain, "{{WindowsAuthDomain}}");

        await TreeRobot.Select("COL1/DIR1/HTTPNONEPKCS12AUTH");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "GET");
        AssertHasText(HttpRobot.Url, "{{BadSslClientCertTestsUrl}}");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionClientCertificate);
        AssertSelection(HttpRobot.Auth.ClientCertificateType, HttpRobot.Auth.ClientCertificateTypeOptionPkcs12);
        AssertIsVisible(HttpRobot.Auth.ClientCertificatePkcs12FilePath);
        AssertIsVisible(HttpRobot.Auth.ClientCertificatePkcs12FilePassword);
        AssertHasText(HttpRobot.Auth.ClientCertificatePkcs12FilePath, "{{ClientCertificatesDir}}/badssl.com-client.p12");
        AssertHasText(HttpRobot.Auth.ClientCertificatePkcs12FilePassword, "{{BadSslClientCertFilePassword}}");
        AssertIsHidden(HttpRobot.Auth.BasicAuthLogin);
        AssertIsHidden(HttpRobot.Auth.BearerAuthToken);
        AssertIsHidden(HttpRobot.Auth.WindowsAuthUseCurrentUser);

        await TreeRobot.Select("COL1/DIR1/HTTPFORMDATA");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "POST");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/post/multipartformdata");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionFormData);
        AssertIsVisible(HttpRobot.ReqBodyFormDataParams);
        var actualFormDataParams = HttpRobot.FormDataParamsVm.Items.Select(x => x.ToFormDataParam()).ToArray();
        Assert(Enumerable.SequenceEqual(formDataParams, actualFormDataParams));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/HTTPGRAPHQL");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "POST");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/post/graphql");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionGraphQl);
        AssertIsVisible(HttpRobot.ReqBodyGraphQlQuery);
        AssertIsVisible(HttpRobot.ReqBodyGraphQlVariables);
        AssertHasText(HttpRobot.ReqBodyGraphQlQuery, "query");
        AssertHasText(HttpRobot.ReqBodyGraphQlVariables, "variables");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2");
        AssertIsVisible(DirRobot.RootView);

        await TreeRobot.Select("COL1/DIR1/DIR2/HTTPRAW");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "POST");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/post/json");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/2");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionRaw);
        AssertIsVisible(HttpRobot.ReqBodyRawContentType);
        AssertIsVisible(HttpRobot.ReqBodyRawContent);
        AssertHasText(HttpRobot.ReqBodyRawContentType, "application/json");
        AssertHasText(HttpRobot.ReqBodyRawContent, "{\"myValue\":\"{{SpecialValue1}}\"}");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2/HTTPFILE");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "POST");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/post/file");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/2");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionFile);
        AssertIsVisible(HttpRobot.ReqBodyFileContentType);
        AssertIsVisible(HttpRobot.ReqBodyFileSrcPath);
        AssertHasText(HttpRobot.ReqBodyFileContentType, "image/jpeg");
        AssertHasText(HttpRobot.ReqBodyFileSrcPath, "{{TestFilesDir}}/homem_aranha.jpg");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2/WS");
        AssertIsVisible(WsRobot.RootView);
        AssertHasText(WsRobot.Url, "{{BaseUrlWs}}/{{WsHttp2Endpoint}}");
        AssertHasText(WsRobot.HttpVersion, "HTTP/2");

        await TreeRobot.Select("COL1/DIR1/DIR2/WS/WSMSGJSON");
        AssertIsVisible(WsMsgRobot.RootView);
        AssertSelection(WsMsgRobot.MessageType, WsMsgRobot.MessageTypeOptionText);
        AssertSelection(WsMsgRobot.ContentMode, WsMsgRobot.ContentModeOptionRaw);
        AssertSelection(WsMsgRobot.ContentRawSyntax, WsMsgRobot.ContentRawSyntaxOptionJson);
        AssertHasText(WsMsgRobot.ContentRaw, "{\"elemento\":\"{{SpecialValue1}}\"}");
        AssertIsHidden(WsMsgRobot.ContentFileSrcPath);
        AssertIsVisible(WsMsgRobot.ContentRaw);

        await TreeRobot.Select("COL1/DIR1/DIR2/WS/WSMSGFILE");
        AssertIsVisible(WsMsgRobot.RootView);
        AssertSelection(WsMsgRobot.MessageType, WsMsgRobot.MessageTypeOptionBinary);
        AssertSelection(WsMsgRobot.ContentMode, WsMsgRobot.ContentModeOptionFile);
        AssertHasText(WsMsgRobot.ContentFileSrcPath, "{{TestFilesDir}}/homem_aranha.jpg");
        AssertIsVisible(WsMsgRobot.ContentFileSrcPath);
        AssertIsHidden(WsMsgRobot.ContentRaw);

        await TreeRobot.Select("COL1/DIR1/DIR2/REPSEQUENTIAL");
        AssertIsVisible(RepeaterRobot.RootView);
        AssertSelection(RepeaterRobot.BaseHttpRequest, "DIR1/DIR2/HTTPFILE");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        AssertValue(RepeaterRobot.DelayInMs, 0);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        AssertSelection(RepeaterRobot.InputDataType, RepeaterRobot.OptionInputDataTypeRaw);
        AssertHasText(RepeaterRobot.InputDataRawEditor, "[{\"V1\":\"1\",\"VA\":\"A\"}]");

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3");
        AssertIsVisible(DirRobot.RootView);

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3/HTTPURLENCODED");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "POST");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/post/urlencoded");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/3");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionUrlEncoded);
        AssertIsVisible(HttpRobot.ReqBodyUrlEncodedParams);
        var actualUrlEncodedParams = HttpRobot.UrlEncodedParamsVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(Enumerable.SequenceEqual(urlEncodedParams, actualUrlEncodedParams));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3/HTTPNONEPEMAUTH");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "GET");
        AssertHasText(HttpRobot.Url, "{{BadSslClientCertTestsUrl}}");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.1");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionNone);
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionClientCertificate);
        AssertSelection(HttpRobot.Auth.ClientCertificateType, HttpRobot.Auth.ClientCertificateTypeOptionPem);
        AssertIsVisible(HttpRobot.Auth.ClientCertificatePemCertificateFilePath);
        AssertIsVisible(HttpRobot.Auth.ClientCertificatePemPrivateKeyFilePath);
        AssertIsVisible(HttpRobot.Auth.ClientCertificatePemPrivateKeyPassword);
        AssertHasText(HttpRobot.Auth.ClientCertificatePemCertificateFilePath, "{{ClientCertificatesDir}}/badssl.com-client-certificate-without-private-key.pem");
        AssertHasText(HttpRobot.Auth.ClientCertificatePemPrivateKeyFilePath, "{{ClientCertificatesDir}}/badssl.com-client-encrypted-private-key.key");
        AssertHasText(HttpRobot.Auth.ClientCertificatePemPrivateKeyPassword, "{{BadSslClientCertFilePassword}}");

        await TreeRobot.Select("COL1/DIR1/DIR2/DIR3/REPRANDOM");
        AssertIsVisible(RepeaterRobot.RootView);
        AssertSelection(RepeaterRobot.BaseHttpRequest, "DIR1/DIR2/DIR3/HTTPNONEPEMAUTH");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        AssertValue(RepeaterRobot.NumberOfRepetitions, 25);
        AssertValue(RepeaterRobot.MaxDop, 3);
        AssertValue(RepeaterRobot.DelayInMs, 10);
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionInputData);
        AssertSelection(RepeaterRobot.InputDataType, RepeaterRobot.OptionInputDataTypeFile);
        AssertHasText(RepeaterRobot.InputDataFileSrcPath, "inputdata.json");

        await TreeRobot.Select("COL1/HTTPHEADERS");
        AssertIsVisible(HttpRobot.RootView);
        AssertHasText(HttpRobot.HttpMethod, "GET");
        AssertHasText(HttpRobot.Url, "{{BaseUrl}}/test/get/headers");
        AssertHasText(HttpRobot.HttpVersion, "HTTP/1.0");
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqBody);
        AssertSelection(HttpRobot.ReqBodyMode, HttpRobot.ReqBodyModeOptionNone);
        var actualHeaders = HttpRobot.ReqHeadersVm.Items.Select(x => x.ToKeyValueParam()).ToArray();
        Assert(Enumerable.SequenceEqual(headers, actualHeaders));
        await HttpRobot.TabControlReq.Select(HttpRobot.TabReqAuth);
        AssertSelection(HttpRobot.Auth.AuthType, HttpRobot.Auth.AuthTypeOptionNone);

        await TreeRobot.Select("COL1/REPSIMPLE");
        AssertIsVisible(RepeaterRobot.RootView);
        AssertSelection(RepeaterRobot.BaseHttpRequest, "HTTPHEADERS");
        await RepeaterRobot.TabControlRepetition.Select(RepeaterRobot.TabItemRepetitionMode);
        AssertValue(RepeaterRobot.NumberOfRepetitions, 200);
        AssertValue(RepeaterRobot.MaxDop, 10);
        AssertValue(RepeaterRobot.DelayInMs, 12);
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