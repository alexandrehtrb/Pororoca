using Avalonia.Controls;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class TopMenuRobot : BaseRobot
{
    public TopMenuRobot(Control rootView) : base(rootView) { }

    internal MenuItem File => GetChildView<MenuItem>("topMenuItemFile")!;
    internal MenuItem File_NewCollection => GetChildView<MenuItem>("topMenuItemFileNewCollection")!;
    internal MenuItem File_ImportCollectionsFromFile => GetChildView<MenuItem>("topMenuItemFileImportCollectionsFromFile")!;
    internal MenuItem File_Exit => GetChildView<MenuItem>("topMenuItemFileExit")!;
    internal MenuItem Options => GetChildView<MenuItem>("topMenuItemOptions")!;
    internal MenuItem Options_Theme => GetChildView<MenuItem>("topMenuItemOptionsTheme")!;
    internal MenuItem Options_Theme_Light => GetChildView<MenuItem>("topMenuItemOptionsThemeLight")!;
    internal MenuItem Options_Theme_Light2 => GetChildView<MenuItem>("topMenuItemOptionsThemeLight2")!;
    internal MenuItem Options_Theme_Dark => GetChildView<MenuItem>("topMenuItemOptionsThemeDark")!;
    internal MenuItem Options_Theme_Pampa => GetChildView<MenuItem>("topMenuItemOptionsThemePampa")!;
    internal MenuItem Options_Theme_AmazonianNight => GetChildView<MenuItem>("topMenuItemOptionsThemeAmazonianNight")!;
    internal MenuItem Options_Language => GetChildView<MenuItem>("topMenuItemOptionsLanguage")!;
    internal MenuItem Options_Language_Português => GetChildView<MenuItem>("topMenuItemOptionsLanguagePortuguês")!;
    internal MenuItem Options_Language_English => GetChildView<MenuItem>("topMenuItemOptionsLanguageEnglish")!;
    internal MenuItem Options_Language_Russian => GetChildView<MenuItem>("topMenuItemOptionsLanguageRussian")!;
    internal MenuItem Options_EnableTlsVerification => GetChildView<MenuItem>("topMenuItemOptionsEnableTlsVerification")!;
    internal MenuItem Help => GetChildView<MenuItem>("topMenuItemHelp")!;

    internal async Task CreateNewCollection()
    {
        await File.ClickOn();
        // for some reason, this needs to be repeated (???), otherwise the menu item won't expand
        await File.ClickOn();
        await File_NewCollection.ClickOn();
        File.Close();
    }

    internal async Task SwitchTlsVerification(bool enable)
    {
        bool isTlsVerificationEnabled = ((Image)Options_EnableTlsVerification.Icon!).IsVisible;
        Options.Open();
        await UITestActions.WaitAfterActionAsync();
        if (enable ^ isTlsVerificationEnabled)
        {
            await Options_EnableTlsVerification.ClickOn();
        }
        Options.Close();
    }
}