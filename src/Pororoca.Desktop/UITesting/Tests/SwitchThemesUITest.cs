using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class SwitchThemesUITest : UITest
{
    private TopMenuRobot Robot { get; }
    private Panel MainWindowPanel { get; }

    public SwitchThemesUITest()
    {
        var content = (Control) MainWindow.Instance!.Content!;
        Robot = new(content);
        MainWindowPanel = content.FindControl<Panel>("mainWindowPanel")!;
    }

    public override async Task RunAsync()
    {
        // light

        await Robot.Options.ClickOn();
        // for some reason, this needs to be repeated (???),
        // otherwise the menu item won't expand
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Light.ClickOn();

        AssertHasIconVisible(Robot.Options_Theme_Light);
        AssertHasIconHidden(Robot.Options_Theme_Dark);
        AssertHasIconHidden(Robot.Options_Theme_Pampa);
        AssertHasIconHidden(Robot.Options_Theme_AmazonianNight);

        AssertBackgroundColor(MainWindowPanel, "#DCDCDC");

        // dark
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Dark.ClickOn();

        AssertHasIconHidden(Robot.Options_Theme_Light);
        AssertHasIconVisible(Robot.Options_Theme_Dark);
        AssertHasIconHidden(Robot.Options_Theme_Pampa);
        AssertHasIconHidden(Robot.Options_Theme_AmazonianNight);

        AssertBackgroundColor(MainWindowPanel, "#080808");

        // pampa
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Pampa.ClickOn();

        AssertHasIconHidden(Robot.Options_Theme_Light);
        AssertHasIconHidden(Robot.Options_Theme_Dark);
        AssertHasIconVisible(Robot.Options_Theme_Pampa);
        AssertHasIconHidden(Robot.Options_Theme_AmazonianNight);

        AssertBackgroundColor(MainWindowPanel, "#EEE8AA");

        // amazonian night
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_AmazonianNight.ClickOn();

        AssertHasIconHidden(Robot.Options_Theme_Light);
        AssertHasIconHidden(Robot.Options_Theme_Dark);
        AssertHasIconHidden(Robot.Options_Theme_Pampa);
        AssertHasIconVisible(Robot.Options_Theme_AmazonianNight);

        AssertBackgroundColor(MainWindowPanel, "#0F263F");
    }
}