using System.Text;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class SwitchLanguagesUITest : UITest
{
    private TopMenuRobot Robot { get; }

    public SwitchLanguagesUITest()
    {
        var content = MainWindow.Instance!.Content;
        Robot = new(this, (Control)content!);
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

        Robot.Options_Language_English.AssertHasIconVisible(this);
        Robot.Options_Language_Português.AssertHasIconHidden(this);
        Robot.Options_Language_Russian.AssertHasIconHidden(this);

        Robot.File.AssertHasText(this, "File");
        Robot.Options.AssertHasText(this, "Options");
        Robot.Help.AssertHasText(this, "Help");

        // português

        await Robot.Options.ClickOn();
        await Robot.Options_Language.ClickOn();
        await Robot.Options_Language_Português.ClickOn();

        Robot.Options_Language_English.AssertHasIconHidden(this);
        Robot.Options_Language_Português.AssertHasIconVisible(this);
        Robot.Options_Language_Russian.AssertHasIconHidden(this);

        Robot.File.AssertHasText(this, "Arquivo");
        Robot.Options.AssertHasText(this, "Opções");
        Robot.Help.AssertHasText(this, "Ajuda");
    }
}