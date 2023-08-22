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
        Robot = new(this, content);
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

        Robot.Options_Theme_Light.AssertHasIconVisible(this);
        Robot.Options_Theme_Dark.AssertHasIconHidden(this);
        Robot.Options_Theme_Pampa.AssertHasIconHidden(this);
        Robot.Options_Theme_AmazonianNight.AssertHasIconHidden(this);

        MainWindowPanel.AssertBackgroundColor(this, "#DCDCDC");

        // dark
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Dark.ClickOn();

        Robot.Options_Theme_Light.AssertHasIconHidden(this);
        Robot.Options_Theme_Dark.AssertHasIconVisible(this);
        Robot.Options_Theme_Pampa.AssertHasIconHidden(this);
        Robot.Options_Theme_AmazonianNight.AssertHasIconHidden(this);

        MainWindowPanel.AssertBackgroundColor(this, "#080808");

        // pampa
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Pampa.ClickOn();

        Robot.Options_Theme_Light.AssertHasIconHidden(this);
        Robot.Options_Theme_Dark.AssertHasIconHidden(this);
        Robot.Options_Theme_Pampa.AssertHasIconVisible(this);
        Robot.Options_Theme_AmazonianNight.AssertHasIconHidden(this);

        MainWindowPanel.AssertBackgroundColor(this, "#EEE8AA");

        // amazonian night
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_AmazonianNight.ClickOn();

        Robot.Options_Theme_Light.AssertHasIconHidden(this);
        Robot.Options_Theme_Dark.AssertHasIconHidden(this);
        Robot.Options_Theme_Pampa.AssertHasIconHidden(this);
        Robot.Options_Theme_AmazonianNight.AssertHasIconVisible(this);

        MainWindowPanel.AssertBackgroundColor(this, "#0F263F");

    }
}