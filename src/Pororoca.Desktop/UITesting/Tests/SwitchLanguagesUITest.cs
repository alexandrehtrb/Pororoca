using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class SwitchLanguagesUITest : PororocaUITest
{
    private TopMenuRobot Robot { get; }

    public SwitchLanguagesUITest()
    {
        var content = (Control)MainWindow.Instance!.Content!;
        Robot = new(content);
    }

    public override async Task RunAsync()
    {
        // english

        await Robot.Options.ClickOn();
        // for some reason, this needs to be repeated (???),
        // otherwise the menu item won't expand
        await Robot.Options.ClickOn();
        await Robot.Options_Language.ClickOn();
        await Robot.Options_Language_English.ClickOn();

        Robot.Options_Language_English.AssertHasIconVisible();
        Robot.Options_Language_Português.AssertHasIconHidden();
        Robot.Options_Language_Russian.AssertHasIconHidden();

        Robot.File.AssertHasText("File");
        Robot.Options.AssertHasText("Options");
        Robot.Help.AssertHasText("Help");

        // português

        await Robot.Options.ClickOn();
        await Robot.Options_Language.ClickOn();
        await Robot.Options_Language_Português.ClickOn();

        Robot.Options_Language_English.AssertHasIconHidden();
        Robot.Options_Language_Português.AssertHasIconVisible();
        Robot.Options_Language_Russian.AssertHasIconHidden();

        Robot.File.AssertHasText("Arquivo");
        Robot.Options.AssertHasText("Opções");
        Robot.Help.AssertHasText("Ajuda");

        // finishing the test with english language set
        await Robot.Options.ClickOn();
        await Robot.Options_Language.ClickOn();
        await Robot.Options_Language_English.ClickOn();
    }
}