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
        AssertIsVisible(Robot.File);
        AssertIsVisible(Robot.Options);
        AssertIsVisible(Robot.Help);

        await Robot.File.ClickOn();
        // for some reason, this needs to be repeated (???),
        // otherwise the menu item won't expand
        await Robot.File.ClickOn();
        AssertIsVisible(Robot.File_NewCollection);
        AssertIsVisible(Robot.File_ImportCollection);
        AssertIsVisible(Robot.File_Exit);

        await Robot.Options.ClickOn();
        AssertIsHidden(Robot.File_Exit);
        AssertIsVisible(Robot.Options_Theme);
        AssertIsVisible(Robot.Options_Language);
        AssertIsVisible(Robot.Options_DisableTlsVerification);

        await Robot.Options_Theme.ClickOn();
        AssertIsVisible(Robot.Options_Theme_Light);
        AssertIsVisible(Robot.Options_Theme_Dark);
        AssertIsVisible(Robot.Options_Theme_Pampa);
        AssertIsVisible(Robot.Options_Theme_AmazonianNight);

        await Robot.Options_Language.ClickOn();
        AssertIsHidden(Robot.Options_Theme_Light);
        AssertIsVisible(Robot.Options_Language_English);
        AssertIsVisible(Robot.Options_Language_Português);
        AssertIsVisible(Robot.Options_Language_Russian);

        //await Robot.Help.ClickOn();
    }
}