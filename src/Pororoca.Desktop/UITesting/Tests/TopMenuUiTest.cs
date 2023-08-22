using System.Text;
using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class TopMenuUITest : UITest
{
    private TopMenuRobot Robot { get; }

    public TopMenuUITest()
    {
        var content = MainWindow.Instance!.Content;
        Robot = new(this, (Control)content!);
    }

    public override async Task RunAsync()
    {
        Robot.File.AssertIsVisible(this);
        Robot.Options.AssertIsVisible(this);
        Robot.Help.AssertIsVisible(this);

        await Robot.File.ClickOn();
        // for some reason, this needs to be repeated (???),
        // otherwise the menu item won't expand
        await Robot.File.ClickOn();
        Robot.File_NewCollection.AssertIsVisible(this);
        Robot.File_ImportCollection.AssertIsVisible(this);
        Robot.File_Exit.AssertIsVisible(this);

        await Robot.Options.ClickOn();
        Robot.File_Exit.AssertIsHidden(this);
        Robot.Options_Theme.AssertIsVisible(this);
        Robot.Options_Language.AssertIsVisible(this);
        Robot.Options_DisableTlsVerification.AssertIsVisible(this);

        await Robot.Options_Theme.ClickOn();
        Robot.Options_Theme_Light.AssertIsVisible(this);
        Robot.Options_Theme_Dark.AssertIsVisible(this);
        Robot.Options_Theme_Pampa.AssertIsVisible(this);
        Robot.Options_Theme_AmazonianNight.AssertIsVisible(this);

        await Robot.Options_Language.ClickOn();
        Robot.Options_Theme_Light.AssertIsHidden(this);
        Robot.Options_Language_English.AssertIsVisible(this);
        Robot.Options_Language_Português.AssertIsVisible(this);
        Robot.Options_Language_Russian.AssertIsVisible(this);

        //await Robot.Help.ClickOn();
    }
}