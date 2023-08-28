using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class CollectionUITest : UITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionFolderRobot DirRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }
    private WebSocketConnectionRobot WsRobot { get; }
    private WebSocketClientMessageRobot WsMsgRobot { get; }

    public CollectionUITest()
    {
        RootView = (Control) MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        TreeRobot = new(RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        DirRobot = new(RootView.FindControl<CollectionFolderView>("collectionFolderView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
        WsRobot = new(RootView.FindControl<WebSocketConnectionView>("wsConnView")!);
        WsMsgRobot = new(RootView.FindControl<WebSocketClientMessageView>("wsClientMsgView")!);
    }

    public override async Task RunAsync()
    {
        await TopMenuRobot.CreateNewCollection();

        AssertIsVisible(ColRobot.RootView);
        await ColRobot.Name.Edit("COLLECTION");
        
        await TreeRobot.Select("COLLECTION");
        await ColRobot.AddEnvironment.ClickOn();
        AssertIsVisible(EnvRobot.RootView);
        await EnvRobot.Name.Edit("ENV1");

        await TreeRobot.Select("COLLECTION");
        await ColRobot.AddFolder.ClickOn();
        AssertIsVisible(DirRobot.RootView);
        await DirRobot.Name.Edit("DIR1");

        await TreeRobot.Select("COLLECTION");
        await ColRobot.AddHttpReq.ClickOn();
        AssertIsVisible(HttpRobot.RootView);
        await HttpRobot.Name.Edit("HTTP1");

        await TreeRobot.Select("COLLECTION");
        await ColRobot.AddWebSocket.ClickOn();
        AssertIsVisible(WsRobot.RootView);
        await WsRobot.Name.Edit("WS1");

        await TreeRobot.Select("COLLECTION/WS1");
        await WsRobot.AddWsClientMsg.ClickOn();
        AssertIsVisible(WsMsgRobot.RootView);
        await WsMsgRobot.Name.Edit("WS_MSG1");
    }
}