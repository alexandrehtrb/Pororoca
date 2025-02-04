using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class CollectionAndCollectionFolderUITest : PororocaUITest
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
    private HttpRepeaterRobot RepeaterRobot { get; }

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
        RepeaterRobot = new(RootView.FindControl<HttpRepeaterView>("httpRepView")!);
    }

    public override async Task RunAsync()
    {
        // collection

        await TopMenuRobot.CreateNewCollection();

        ColRobot.RootView.AssertIsVisible();
        await ColRobot.Name.Edit("COL1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddEnvironment.ClickOn();
        EnvRobot.RootView.AssertIsVisible();
        await EnvRobot.Name.Edit("ENV1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddFolder.ClickOn();
        DirRobot.RootView.AssertIsVisible();
        await DirRobot.Name.Edit("DIR1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        HttpRobot.RootView.AssertIsVisible();
        await HttpRobot.Name.Edit("HTTP1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddWebSocket.ClickOn();
        WsRobot.RootView.AssertIsVisible();
        await WsRobot.Name.Edit("WS1");

        await TreeRobot.Select("COL1/WS1");
        await WsRobot.AddClientMessage.ClickOn();
        WsMsgRobot.RootView.AssertIsVisible();
        await WsMsgRobot.Name.Edit("WS_MSG1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddRepeater.ClickOn();
        RepeaterRobot.RootView.AssertIsVisible();
        await RepeaterRobot.Name.Edit("REP1");

        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1/WS_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/REP1");

        // folder

        await TreeRobot.Select("COL1/DIR1");
        DirRobot.RootView.AssertIsVisible();
        await DirRobot.AddFolder.ClickOn();
        await DirRobot.Name.Edit("DIR1_1");

        await TreeRobot.Select("COL1/DIR1/DIR1_1");
        DirRobot.RootView.AssertIsVisible();
        await DirRobot.AddHttpReq.ClickOn();
        HttpRobot.RootView.AssertIsVisible();
        await HttpRobot.Name.Edit("HTTP_1");

        await TreeRobot.Select("COL1/DIR1/DIR1_1");
        DirRobot.RootView.AssertIsVisible();
        await DirRobot.AddWebSocket.ClickOn();
        WsRobot.RootView.AssertIsVisible();
        await WsRobot.Name.Edit("WS_1");

        await TreeRobot.Select("COL1/DIR1/DIR1_1/WS_1");
        await WsRobot.AddClientMessage.ClickOn();
        WsMsgRobot.RootView.AssertIsVisible();
        await WsMsgRobot.Name.Edit("WS_1_MSG1");

        await TreeRobot.Select("COL1/DIR1/DIR1_1");
        DirRobot.RootView.AssertIsVisible();
        await DirRobot.AddRepeater.ClickOn();
        RepeaterRobot.RootView.AssertIsVisible();
        await RepeaterRobot.Name.Edit("REP_1");

        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR1_1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR1_1/HTTP_1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR1_1/WS_1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR1_1/WS_1/WS_1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1/DIR1_1/REP_1");
    }
}