using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class SwitchLanguagesUITest : UITest
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

        AssertHasIconVisible(Robot.Options_Language_English);
        AssertHasIconHidden(Robot.Options_Language_Português);
        AssertHasIconHidden(Robot.Options_Language_Russian);

        AssertHasText(Robot.File, "File");
        AssertHasText(Robot.Options, "Options");
        AssertHasText(Robot.Help, "Help");

        // português

        await Robot.Options.ClickOn();
        await Robot.Options_Language.ClickOn();
        await Robot.Options_Language_Português.ClickOn();

        AssertHasIconHidden(Robot.Options_Language_English);
        AssertHasIconVisible(Robot.Options_Language_Português);
        AssertHasIconHidden(Robot.Options_Language_Russian);

        AssertHasText(Robot.File, "Arquivo");
        AssertHasText(Robot.Options, "Opções");
        AssertHasText(Robot.Help, "Ajuda");

        // finishing the test with english language set
        await Robot.Options.ClickOn();
        await Robot.Options_Language.ClickOn();
        await Robot.Options_Language_English.ClickOn();
    }
}