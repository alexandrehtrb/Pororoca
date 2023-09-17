using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class TreeDeleteItemsUITest : UITest
{
    private Control RootView { get; }
    private CollectionsGroupView CollectionsGroup { get; }
    private ItemsTreeRobot TreeRobot { get; }

    public TreeDeleteItemsUITest()
    {
        RootView = (Control) MainWindow.Instance!.Content!;
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

        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/DIR1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/HTTP1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/WS1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/WS1/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP0");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0/WS2_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1/WS2_MSG1");
        
        AssertTreeItemNotExists(CollectionsGroup, "COL2");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/ENVS/ENV2");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/DIR2");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/DIR2/DIR1");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/DIR2/HTTP1");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/DIR2/WS1/WS1_MSG1");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/HTTP2");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/WS2");
        AssertTreeItemNotExists(CollectionsGroup, "COL2/WS2/WS2_MSG1");

        // when deleting a single item, only it should be deleted
        await TreeRobot.Select("COL1/WS0/WS1_MSG1");
        await TreeRobot.Delete();

        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/DIR1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/HTTP1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/WS1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR2/WS1/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP0");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS0/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0/WS2_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1/WS2_MSG1");

        // when deleting a parent, recursively remove all sub-items
        await TreeRobot.Select("COL1/DIR2");
        await TreeRobot.Delete();

        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/DIR1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/HTTP1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/WS1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/WS1/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP0");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS0/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0/WS2_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS1/WS2_MSG1");        
        
        // when deleting a parent and a child selected, siblings should be removed too
        await TreeRobot.SelectMultiple("COL1/WS1", "COL1/WS1/WS1_MSG1");
        await TreeRobot.Delete();

        AssertTreeItemExists(CollectionsGroup, "COL1");
        AssertTreeItemExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/DIR1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/HTTP1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/WS1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/WS1/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP0");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS0/WS1_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/WS0/WS2_MSG1");
        AssertTreeItemExists(CollectionsGroup, "COL1/HTTP1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS1/WS1_MSG1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS1/WS2_MSG1");  

        // delete everything else with a multiple selection
        await TreeRobot.SelectMultiple("COL1", "COL1/ENVS/ENVS1", "COL1/DIR1", "COL1/HTTP0", "COL1/WS0", "COL1/WS0/WS2_MSG1", "COL1/HTTP1");
        await TreeRobot.Delete();

        AssertTreeItemNotExists(CollectionsGroup, "COL1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/ENVS/ENV1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/DIR1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/HTTP1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/WS1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/DIR2/WS1/WS1_MSG1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/HTTP0");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS0");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS0/WS1_MSG1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS0/WS2_MSG1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/HTTP1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS1/WS1_MSG1");
        AssertTreeItemNotExists(CollectionsGroup, "COL1/WS1/WS2_MSG1");  
    }
}