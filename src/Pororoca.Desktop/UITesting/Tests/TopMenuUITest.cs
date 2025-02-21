using Avalonia.Controls;
using Pororoca.Desktop.UITesting.Robots;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed class TopMenuUITest : PororocaUITest
{
    private TopMenuRobot Robot { get; }

    public TopMenuUITest()
    {
        object? content = MainWindow.Instance!.Content;
        Robot = new((Control)content!);
    }

    public override async Task RunAsync()
    {
        Robot.File.AssertIsVisible();
        Robot.Options.AssertIsVisible();
        Robot.Help.AssertIsVisible();

        await Robot.File.ClickOn();
        // for some reason, this needs to be repeated (???),
        // otherwise the menu item won't expand
        await Robot.File.ClickOn();
        Robot.File_NewCollection.AssertIsVisible();
        Robot.File_ImportCollectionsFromFile.AssertIsVisible();
        Robot.File_Exit.AssertIsVisible();

        await Robot.Options.ClickOn();
        Robot.File_Exit.AssertIsHidden();
        Robot.Options_Theme.AssertIsVisible();
        Robot.Options_Language.AssertIsVisible();
        Robot.Options_EnableTlsVerification.AssertIsVisible();

        await Robot.Options_Theme.ClickOn();
        Robot.Options_Theme_Light.AssertIsVisible();
        Robot.Options_Theme_Light2.AssertIsVisible();
        Robot.Options_Theme_Dark.AssertIsVisible();
        Robot.Options_Theme_Pampa.AssertIsVisible();
        Robot.Options_Theme_AmazonianNight.AssertIsVisible();

        await Robot.Options_Language.ClickOn();
        Robot.Options_Theme_Light.AssertIsHidden();
        Robot.Options_Language_English.AssertIsVisible();
        Robot.Options_Language_Português.AssertIsVisible();
        Robot.Options_Language_Russian.AssertIsVisible();

        //await Robot.Help.ClickOn();
    }
}