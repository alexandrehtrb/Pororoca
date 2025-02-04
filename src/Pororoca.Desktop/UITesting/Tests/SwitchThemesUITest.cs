using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class SwitchThemesUITest : PororocaUITest
{
    private TopMenuRobot Robot { get; }
    private Panel MainWindowPanel { get; }

    public SwitchThemesUITest()
    {
        var content = (Control)MainWindow.Instance!.Content!;
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

        Robot.Options_Theme_Light.AssertHasIconVisible();
        Robot.Options_Theme_Light2.AssertHasIconHidden();
        Robot.Options_Theme_Dark.AssertHasIconHidden();
        Robot.Options_Theme_Pampa.AssertHasIconHidden();
        Robot.Options_Theme_AmazonianNight.AssertHasIconHidden();

        MainWindowPanel.AssertBackgroundColor("#DCDCDC");

        // light 2

        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Light2.ClickOn();

        Robot.Options_Theme_Light.AssertHasIconHidden();
        Robot.Options_Theme_Light2.AssertHasIconVisible();
        Robot.Options_Theme_Dark.AssertHasIconHidden();
        Robot.Options_Theme_Pampa.AssertHasIconHidden();
        Robot.Options_Theme_AmazonianNight.AssertHasIconHidden();

        MainWindowPanel.AssertBackgroundColor("#FAFAFA");

        // dark
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Dark.ClickOn();

        Robot.Options_Theme_Light.AssertHasIconHidden();
        Robot.Options_Theme_Light2.AssertHasIconHidden();
        Robot.Options_Theme_Dark.AssertHasIconVisible();
        Robot.Options_Theme_Pampa.AssertHasIconHidden();
        Robot.Options_Theme_AmazonianNight.AssertHasIconHidden();

        MainWindowPanel.AssertBackgroundColor("#080808");

        // pampa
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_Pampa.ClickOn();

        Robot.Options_Theme_Light.AssertHasIconHidden();
        Robot.Options_Theme_Light2.AssertHasIconHidden();
        Robot.Options_Theme_Dark.AssertHasIconHidden();
        Robot.Options_Theme_Pampa.AssertHasIconVisible();
        Robot.Options_Theme_AmazonianNight.AssertHasIconHidden();

        MainWindowPanel.AssertBackgroundColor("#EEE8AA");

        // amazonian night
        await Robot.Options.ClickOn();
        await Robot.Options_Theme.ClickOn();
        await Robot.Options_Theme_AmazonianNight.ClickOn();

        Robot.Options_Theme_Light.AssertHasIconHidden();
        Robot.Options_Theme_Light2.AssertHasIconHidden();
        Robot.Options_Theme_Dark.AssertHasIconHidden();
        Robot.Options_Theme_Pampa.AssertHasIconHidden();
        Robot.Options_Theme_AmazonianNight.AssertHasIconVisible();

        MainWindowPanel.AssertBackgroundColor("#04538B");
    }
}