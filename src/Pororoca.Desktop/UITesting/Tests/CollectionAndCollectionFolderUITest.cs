using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class CollectionAndCollectionFolderUITest : UITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }
    private CollectionsGroupView CollectionsGroup { get; }
    private ItemsTreeRobot TreeRobot { get; }
    private CollectionRobot ColRobot { get; }
    private CollectionFolderRobot DirRobot { get; }
    private EnvironmentRobot EnvRobot { get; }
    private HttpRequestRobot HttpRobot { get; }
    private WebSocketConnectionRobot WsRobot { get; }
    private WebSocketClientMessageRobot WsMsgRobot { get; }

    public CollectionAndCollectionFolderUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
        CollectionsGroup = RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!;
        TreeRobot = new(CollectionsGroup);
        ColRobot = new(RootView.FindControl<CollectionView>("collectionView")!);
        DirRobot = new(RootView.FindControl<CollectionFolderView>("collectionFolderView")!);
        EnvRobot = new(RootView.FindControl<EnvironmentView>("environmentView")!);
        HttpRobot = new(RootView.FindControl<HttpRequestView>("httpReqView")!);
        WsRobot = new(RootView.FindControl<WebSocketConnectionView>("wsConnView")!);
        WsMsgRobot = new(RootView.FindControl<WebSocketClientMessageView>("wsClientMsgView")!);
    }

    public override async Task RunAsync()
    {
        // collection

        await TopMenuRobot.CreateNewCollection();

        AssertIsVisible(ColRobot.RootView);
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        AssertIsVisible(EnvRobot.RootView);
        await EnvRobot.Name.Edit("ENV1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddFolder.ClickOn();
        AssertIsVisible(DirRobot.RootView);
        await DirRobot.Name.Edit("DIR1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        AssertIsVisible(HttpRobot.RootView);
        await HttpRobot.Name.Edit("HTTP1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddWebSocket.ClickOn();
        AssertIsVisible(WsRobot.RootView);
        await WsRobot.Name.Edit("WS1");

        await TreeRobot.Select("COL1/WS1");
        await WsRobot.AddClientMessage.ClickOn();
        AssertIsVisible(WsMsgRobot.RootView);
        await WsMsgRobot.Name.Edit("WS_MSG1");

        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1/WS_MSG1");

        // folder

        await TreeRobot.Select("COL1/DIR1");
        AssertIsVisible(DirRobot.RootView);
        await DirRobot.AddFolder.ClickOn();
        await DirRobot.Name.Edit("DIR1_1");

        await TreeRobot.Select("COL1/DIR1/DIR1_1");
        AssertIsVisible(DirRobot.RootView);
        await DirRobot.AddHttpReq.ClickOn();
        AssertIsVisible(HttpRobot.RootView);
        await HttpRobot.Name.Edit("HTTP_1");

        await TreeRobot.Select("COL1/DIR1/DIR1_1");
        AssertIsVisible(DirRobot.RootView);
        await DirRobot.AddWebSocket.ClickOn();
        AssertIsVisible(WsRobot.RootView);
        await WsRobot.Name.Edit("WS_1");

        await TreeRobot.Select("COL1/DIR1/DIR1_1/WS_1");
        await WsRobot.AddClientMessage.ClickOn();
        AssertIsVisible(WsMsgRobot.RootView);
        await WsMsgRobot.Name.Edit("WS_1_MSG1");

        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR1_1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR1_1/HTTP_1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR1_1/WS_1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1/DIR1_1/WS_1/WS_1_MSG1");
    }
}