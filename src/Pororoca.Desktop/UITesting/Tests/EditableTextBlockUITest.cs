using Avalonia.Controls;
using Pororoca.Desktop.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class EditableTextBlockUITest : PororocaUITest
{
    private Control RootView { get; }
    private TopMenuRobot TopMenuRobot { get; }

    public EditableTextBlockUITest()
    {
        RootView = (Control)MainWindow.Instance!.Content!;
        TopMenuRobot = new(RootView);
    }

    public override async Task RunAsync()
    {
        await TopMenuRobot.CreateNewCollection();

        var collectionView = RootView.FindControl<CollectionView>("collectionView")!;
        var etbView = collectionView.FindControl<EditableTextBlock>("etbName")!;
        EditableTextBlockRobot robot = new(etbView);

        robot.RootView.AssertIsVisible();
        AssertCondition(robot.Icon == null);
        robot.AppliedText.AssertIsVisible();
        robot.AppliedText.AssertHasText("New collection");
        robot.TextBeingEdited.AssertIsHidden();
        robot.ButtonEditOrApply.AssertIsVisible();
        robot.ButtonIconEdit.AssertIsVisible();
        robot.ButtonIconApply.AssertIsHidden();

        await robot.ButtonEditOrApply.ClickOn();

        robot.RootView.AssertIsVisible();
        AssertCondition(robot.Icon == null);
        robot.AppliedText.AssertIsHidden();
        robot.TextBeingEdited.AssertIsVisible();
        robot.TextBeingEdited.AssertHasText("New collection");
        robot.ButtonEditOrApply.AssertIsVisible();
        robot.ButtonIconApply.AssertIsVisible();
        robot.ButtonIconEdit.AssertIsHidden();

        await robot.TextBeingEdited.ClearText();
        await robot.TextBeingEdited.TypeText("COLLECTION");
        await robot.ButtonEditOrApply.ClickOn();

        robot.RootView.AssertIsVisible();
        AssertCondition(robot.Icon == null);
        robot.AppliedText.AssertIsVisible();
        robot.AppliedText.AssertHasText("COLLECTION");
        robot.TextBeingEdited.AssertIsHidden();
        robot.ButtonEditOrApply.AssertIsVisible();
        robot.ButtonIconEdit.AssertIsVisible();
        robot.ButtonIconApply.AssertIsHidden();
    }
}