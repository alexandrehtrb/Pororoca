using System.Collections.ObjectModel;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Desktop.Views;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class WebSocketsUITest : UITest
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
    private WebSocketClientMessageRobot WsMsgRobot { get; }
    private readonly List<decimal> httpVersionsToTest;

    public WebSocketsUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        ColVarsRobot = new(RootView.FindControl<CollectionVariablesView>("collectionVariablesView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        WsRobot = new(RootView.FindControl<WebSocketConnectionView>("wsConnView")!);
        WsMsgRobot = new(RootView.FindControl<WebSocketClientMessageView>("wsClientMsgView")!);

        this.httpVersionsToTest = new() { 1.1m };
        if (AvailablePororocaRequestSelectionOptions.IsWebSocketHttpVersionAvailableInOS(2.0m, out _))
        {
            this.httpVersionsToTest.Add(2.0m);
        }
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
        await ColRobot.AddWebSocket.ClickOn();
        await WsRobot.Name.Edit("WS");
        await WsRobot.AddClientMessage.ClickOn();
        await WsMsgRobot.Name.Edit("JSON");
        await WsMsgRobot.SetRawJsonContent("{\"elemento\":\"{{SpecialValue1}}\"}");
        await TreeRobot.Select("COL1/WS");
        await WsRobot.AddClientMessage.ClickOn();
        await WsMsgRobot.Name.Edit("HOMEM_ARANHA");
        await WsMsgRobot.SetFileBinaryContent("{{TestFilesDir}}/homem_aranha.jpg");
        await TreeRobot.Select("COL1/WS");

        if (OperatingSystem.IsLinux())
        {
            // we can't trust ASP.NET Core dev certs in Linux,
            // so we disable TLS verification for requests to our TestServer
            await TopMenuRobot.SwitchTlsVerification(false);
        }

        foreach (decimal version in this.httpVersionsToTest)
        {
            AppendToLog($"Selecting HTTP version {version}.");
            await WsRobot.SetHttpVersion(version);
            if (version == 1.1m)
            {
                await WsRobot.Url.ClearAndTypeText("{{BaseUrlWs}}/{{WsHttp1Endpoint}}");
            }
            else if (version == 2.0m)
            {
                await WsRobot.Url.ClearAndTypeText("{{BaseUrlWs}}/{{WsHttp2Endpoint}}");
            }

            AssertIsHidden(WsRobot.IsWsConnected);
            AssertIsHidden(WsRobot.Name.IconConnectedWebSocket);
            AssertIsVisible(WsRobot.Name.IconDisconnectedWebSocket);
            await WsRobot.ClickOnConnectAndWaitForConnection();
            AssertIsVisible(WsRobot.IsWsConnected);
            AssertIsVisible(WsRobot.Name.IconConnectedWebSocket);
            AssertIsHidden(WsRobot.Name.IconDisconnectedWebSocket);

            AssertIsHidden(WsRobot.ConnectionRequestException);
            AssertIsVisible(WsRobot.ConnectionResponseStatusCode);
            AssertIsVisible(WsRobot.ConnectionResponseHeaders);

            if (version == 1.1m)
            {
                AssertContainsText(WsRobot.ConnectionResponseStatusCode, "101 SwitchingProtocols");
                AssertContainsResponseHeader("Connection", "Upgrade");
                AssertContainsResponseHeader("Server", "Kestrel");
                AssertContainsResponseHeader("Upgrade", "websocket");
                AssertContainsResponseHeader("Sec-WebSocket-Accept");
                AssertContainsResponseHeader("Date");
            }
            else if (version == 2.0m)
            {
                AssertContainsText(WsRobot.ConnectionResponseStatusCode, "200 OK");
                AssertContainsResponseHeader("Server", "Kestrel");
                AssertContainsResponseHeader("Date");
            }

            await WsRobot.MessageToSend.Select("JSON");
            await WsRobot.SendMessage.ClickOn();
            await Wait(1);

            await AssertExchangedMessage(0, "client -> server", "text, 24 bytes", "Text", "{\"elemento\":\"Plutônio\"}");
            await AssertExchangedMessage(1, "server -> client", "text, 50 bytes", "Text", "received text (24 bytes): {\"elemento\":\"Plutônio\"}");

            await WsRobot.MessageToSend.Select("HOMEM_ARANHA");
            await WsRobot.SendMessage.ClickOn();
            await Wait(1);

            await AssertExchangedMessage(2, "client -> server", "binary, 9784 bytes", "Binary", "binary, 9784 bytes");
            await AssertExchangedMessage(3, "server -> client", "text, 26 bytes", "Text", "received binary 9784 bytes");

            await WsRobot.ClickOnDisconnectAndWaitForDisconnection();
            await Wait(3);
            AssertIsHidden(WsRobot.IsWsConnected);
            AssertIsHidden(WsRobot.Name.IconConnectedWebSocket);
            AssertIsVisible(WsRobot.Name.IconDisconnectedWebSocket);

            await Wait(2);
            await AssertExchangedMessage(4, "server -> client", "closing, 7 bytes", "Closing message", "ok, bye");
        }

        if (OperatingSystem.IsLinux())
        {
            // reenable TLS verification for BadSSL requests
            await TopMenuRobot.SwitchTlsVerification(true);
        }
    }

    private async Task AssertExchangedMessage(int index, string originDescription, string sizeDescription, string typeDescription, string contentDescription)
    {
        var exchangedMessages = WsRobot.ExchangedMessages.ItemsSource!.Cast<WebSocketExchangedMessageViewModel>().Reverse().ToArray();

        var msg = exchangedMessages[index];
        WsRobot.ExchangedMessages.SelectedItem = msg;
        await UITestActions.WaitAfterActionAsync();

        Assert(msg.OriginDescription == originDescription);
        Assert(msg.MessageSizeDescription == sizeDescription);
        AssertHasText(WsRobot.MessageDetailType, typeDescription);
        AssertHasText(WsRobot.MessageDetailContent, contentDescription);
    }

    private void AssertContainsResponseHeader(string key)
    {
        var vm = ((WebSocketConnectionViewModel)WsRobot.RootView!.DataContext!).ConnectionResponseHeadersTableVm;
        Assert(vm.Items.Any(h => h.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
    }

    private void AssertContainsResponseHeader(string key, string value)
    {
        var vm = ((WebSocketConnectionViewModel)WsRobot.RootView!.DataContext!).ConnectionResponseHeadersTableVm;
        Assert(vm.Items.Any(h => h.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && h.Value == value));
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
}