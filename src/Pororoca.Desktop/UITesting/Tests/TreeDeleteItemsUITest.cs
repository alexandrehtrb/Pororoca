using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class TreeDeleteItemsUITest : PororocaUITest
{
    private Control RootView { get; }
    private CollectionsGroupView CollectionsGroup { get; }
    private ItemsTreeRobot TreeRobot { get; }

    public TreeDeleteItemsUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        CollectionsGroup = RootView.FindControl<CollectionsGroupView>("mainWindowCollectionsGroup")!;
        TreeRobot = new(CollectionsGroup);
    }

    public override async Task RunAsync()
    {
        await new TreeCopyAndPasteItemsUITest().RunAsync();
        await Wait(2);

        // when deleting a collection, all its sub-items should be deleted
        await TreeRobot.Select("COL2");
        await TreeRobot.Delete();

        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/HTTP1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/WS1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/REP1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP0");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/REP0");
        CollectionsGroup.AssertTreeItemExists("COL1/REP1");

        CollectionsGroup.AssertTreeItemNotExists("COL2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/ENVS/ENV2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/HTTP1");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL2/DIR2/REP1");
        CollectionsGroup.AssertTreeItemNotExists("COL2/HTTP2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/WS2");
        CollectionsGroup.AssertTreeItemNotExists("COL2/WS2/WS2_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL2/REP2");

        // when deleting a single item, only it should be deleted
        await TreeRobot.Select("COL1/WS0/WS1_MSG1");
        await TreeRobot.Delete();

        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/HTTP1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/WS1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR2/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP0");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS0/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/REP0");
        CollectionsGroup.AssertTreeItemExists("COL1/REP1");

        // when deleting a parent, recursively remove all sub-items
        await TreeRobot.Select("COL1/DIR2");
        await TreeRobot.Delete();

        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/HTTP1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/WS1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/REP1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP0");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS0/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS1/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/REP0");
        CollectionsGroup.AssertTreeItemExists("COL1/REP1");

        // when deleting a parent and a child selected, siblings should be removed too
        await TreeRobot.SelectMultiple("COL1/WS1", "COL1/WS1/WS1_MSG1");
        await TreeRobot.Delete();

        CollectionsGroup.AssertTreeItemExists("COL1");
        CollectionsGroup.AssertTreeItemExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/HTTP1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/WS1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/REP1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP0");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS0/WS1_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/WS0/WS2_MSG1");
        CollectionsGroup.AssertTreeItemExists("COL1/HTTP1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1/WS2_MSG1");

        // delete everything else with a multiple selection
        await TreeRobot.SelectMultiple("COL1", "COL1/ENVS/ENVS1", "COL1/DIR1", "COL1/HTTP0", "COL1/WS0", "COL1/WS0/WS2_MSG1", "COL1/HTTP1", "COL1/REP1");
        await TreeRobot.Delete();

        CollectionsGroup.AssertTreeItemNotExists("COL1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/ENVS/ENV1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/DIR1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/HTTP1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/WS1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/DIR2/REP1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/HTTP0");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS0");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS0/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS0/WS2_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/HTTP1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1/WS1_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/WS1/WS2_MSG1");
        CollectionsGroup.AssertTreeItemNotExists("COL1/REP0");
        CollectionsGroup.AssertTreeItemNotExists("COL1/REP1");
    }
}