using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class TreeCutAndPasteItemsUITest : PororocaUITest
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

    public TreeCutAndPasteItemsUITest()
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
        // create a collection with many different items
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL1");

        await ColRobot.AddEnvironment.ClickOn();
        await EnvRobot.Name.Edit("ENV1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddFolder.ClickOn();
        await DirRobot.Name.Edit("DIR1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddHttpReq.ClickOn();
        await HttpRobot.Name.Edit("HTTP1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddWebSocket.ClickOn();
        await WsRobot.Name.Edit("WS1");

        await TreeRobot.Select("COL1/WS1");
        await WsRobot.AddClientMessage.ClickOn();
        await WsMsgRobot.Name.Edit("WS1_MSG1");

        await TreeRobot.Select("COL1");
        await ColRobot.AddRepeater.ClickOn();
        await RepeaterRobot.Name.Edit("REP1");

        // copy those items
        await TreeRobot.SelectMultiple("COL1/ENVS/ENV1",
                                       "COL1/DIR1",
                                       "COL1/HTTP1",
                                       "COL1/WS1",
                                       "COL1/WS1/WS1_MSG1",
                                       "COL1/REP1");
        await Wait(1);
        await TreeRobot.Cut();

        // create new collection and paste copied items into it
        await TopMenuRobot.CreateNewCollection();
        await ColRobot.Name.Edit("COL2");
        await TreeRobot.Select("COL2");
        await Wait(1);
        await TreeRobot.Paste();

        // check if items exist only in target collection
        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/HTTP1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/REP1");

        CollectionsGroup.AssertTreeItemExists("COL2");
        CollectionsGroup.AssertTreeItemExists("COL2/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL2/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL2/HTTP1");
        CollectionsGroup.AssertTreeItemExists("COL2/WS1");
        CollectionsGroup.AssertTreeItemExists("COL2/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL2/REP1");

        // let's rename items in col2

        await TreeRobot.Select("COL2/ENVS/ENV1");
        await EnvRobot.Name.Edit("ENV2");

        await TreeRobot.Select("COL2/DIR1");
        await DirRobot.Name.Edit("DIR2");

        await TreeRobot.Select("COL2/HTTP1");
        await HttpRobot.Name.Edit("HTTP2");

        await TreeRobot.Select("COL2/WS1");
        await WsRobot.Name.Edit("WS2");

        await TreeRobot.Select("COL2/WS2/WS1_MSG1");
        await WsMsgRobot.Name.Edit("WS2_MSG1");

        await TreeRobot.Select("COL2/REP1");
        await RepeaterRobot.Name.Edit("REP2");

        // let's cut and paste a single http req into a dir
        await TreeRobot.Select("COL2/HTTP2");
        await TreeRobot.Cut();
        await TreeRobot.Select("COL2/DIR2");
        await TreeRobot.Paste();
        CollectionsGroup.AssertTreeItemNotExists("COL2/HTTP2");
        CollectionsGroup.AssertTreeItemExists("COL2/DIR2/HTTP2");

        // let's cut and paste a single ws into a dir
        await TreeRobot.Select("COL2/WS2");
        await TreeRobot.Cut();
        await TreeRobot.Select("COL2/DIR2");
        await TreeRobot.Paste();
        CollectionsGroup.AssertTreeItemNotExists("COL2/WS2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/WS2/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL2/DIR2/WS2");
        CollectionsGroup.AssertTreeItemExists("COL2/DIR2/WS2/WS2_MSG1");

        // let's cut and paste a single http rep into a dir
        await TreeRobot.Select("COL2/REP2");
        await TreeRobot.Cut();
        await TreeRobot.Select("COL2/DIR2");
        await TreeRobot.Paste();
        CollectionsGroup.AssertTreeItemNotExists("COL2/REP2");
        CollectionsGroup.AssertTreeItemExists("COL2/DIR2/REP2");

        // let's cut and paste a single dir into another dir
        await TreeRobot.Select("COL2");
        await ColRobot.AddFolder.ClickOn();
        await DirRobot.Name.Edit("DIR1");
        await TreeRobot.Select("COL2/DIR1");
        await TreeRobot.Cut();
        await TreeRobot.Select("COL2/DIR2");
        await TreeRobot.Paste();
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL2/DIR2/DIR1");

        // let's cut and paste a dir, should make a full depth cut
        await TreeRobot.Select("COL2/DIR2");
        await TreeRobot.Cut();
        await TreeRobot.Select("COL1");
        await TreeRobot.Paste();
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/HTTP2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/WS2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/WS2/WS2_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/REP2");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/HTTP2");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/WS2");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/WS2/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/REP2");

        // let's cut and paste ws msgs
        await TreeRobot.Select("COL1");
        await ColRobot.AddWebSocket.ClickOn();
        await WsRobot.Name.Edit("WS1");
        await WsRobot.AddClientMessage.ClickOn();
        await WsMsgRobot.Name.Edit("WS1_MSG1");
        await TreeRobot.Select("COL1/WS1/WS1_MSG1");
        await TreeRobot.Cut();
        await TreeRobot.Select("COL1/DIR2/WS2");
        await TreeRobot.Paste();
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/WS2/WS1_MSG1");
    }
}