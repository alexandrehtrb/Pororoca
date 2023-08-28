using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class EditableTextBlockUITest : UITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }

    public EditableTextBlockUITest()
    {
        RootView = (Control) MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
    }

    public override async Task RunAsync()
    {
        await TopMenuRobot.File.ClickOn();
        // for some reason, this needs to be repeated (???), otherwise the menu item won't expand
        await TopMenuRobot.File.ClickOn();
        await TopMenuRobot.File_NewCollection.ClickOn();
        TopMenuRobot.File.Close();
        
        var collectionView = RootView.FindControl<CollectionView>("collectionView")!;
        var etbView = collectionView.FindControl<EditableTextBlockView>("etbName")!;
        EditableTextBlockRobot robot = new(etbView);

        AssertIsVisible(robot.RootView);
        AssertIsHidden(robot.IconHttp);
        AssertIsHidden(robot.IconDisconnectedWebSocket);
        AssertIsHidden(robot.IconConnectedWebSocket);
        AssertIsVisible(robot.AppliedText);
        AssertHasText(robot.AppliedText, "New collection");
        AssertIsHidden(robot.TextBeingEdited);
        AssertIsVisible(robot.ButtonEditOrApply);
        AssertIsVisible(robot.ButtonIconEdit);
        AssertIsHidden(robot.ButtonIconApply);

        await robot.ButtonEditOrApply.ClickOn();

        AssertIsVisible(robot.RootView);
        AssertIsHidden(robot.IconHttp);
        AssertIsHidden(robot.IconDisconnectedWebSocket);
        AssertIsHidden(robot.IconConnectedWebSocket);
        AssertIsHidden(robot.AppliedText);
        AssertIsVisible(robot.TextBeingEdited);
        AssertHasText(robot.TextBeingEdited, "New collection");
        AssertIsVisible(robot.ButtonEditOrApply);
        AssertIsVisible(robot.ButtonIconApply);
        AssertIsHidden(robot.ButtonIconEdit);

        await robot.TextBeingEdited.ClearText();
        await robot.TextBeingEdited.TypeText("COLLECTION");
        await robot.ButtonEditOrApply.ClickOn();

        AssertIsVisible(robot.RootView);
        AssertIsHidden(robot.IconHttp);
        AssertIsHidden(robot.IconDisconnectedWebSocket);
        AssertIsHidden(robot.IconConnectedWebSocket);
        AssertIsVisible(robot.AppliedText);
        AssertHasText(robot.AppliedText, "COLLECTION");
        AssertIsHidden(robot.TextBeingEdited);
        AssertIsVisible(robot.ButtonEditOrApply);
        AssertIsVisible(robot.ButtonIconEdit);
        AssertIsHidden(robot.ButtonIconApply);
    }
}