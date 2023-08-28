using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class TopMenuRobot : BaseRobot
{
    public TopMenuRobot(Control rootView) : base(rootView) { }
    
    internal MenuItem File => GetChildView<MenuItem>("topMenuItemFile")!;
    internal MenuItem File_NewCollection => GetChildView<MenuItem>("topMenuItemFileNewCollection")!;
    internal MenuItem File_ImportCollection => GetChildView<MenuItem>("topMenuItemFileImportCollection")!;
    internal MenuItem File_Exit => GetChildView<MenuItem>("topMenuItemFileExit")!;
    internal MenuItem Options => GetChildView<MenuItem>("topMenuItemOptions")!;
    internal MenuItem Options_Theme => GetChildView<MenuItem>("topMenuItemOptionsTheme")!;
    internal MenuItem Options_Theme_Light => GetChildView<MenuItem>("topMenuItemOptionsThemeLight")!;
    internal MenuItem Options_Theme_Dark => GetChildView<MenuItem>("topMenuItemOptionsThemeDark")!;
    internal MenuItem Options_Theme_Pampa => GetChildView<MenuItem>("topMenuItemOptionsThemePampa")!;
    internal MenuItem Options_Theme_AmazonianNight => GetChildView<MenuItem>("topMenuItemOptionsThemeAmazonianNight")!;
    internal MenuItem Options_Language => GetChildView<MenuItem>("topMenuItemOptionsLanguage")!;
    internal MenuItem Options_Language_Português => GetChildView<MenuItem>("topMenuItemOptionsLanguagePortuguês")!;
    internal MenuItem Options_Language_English => GetChildView<MenuItem>("topMenuItemOptionsLanguageEnglish")!;
    internal MenuItem Options_Language_Russian => GetChildView<MenuItem>("topMenuItemOptionsLanguageRussian")!;
    internal MenuItem Options_DisableTlsVerification => GetChildView<MenuItem>("topMenuItemOptionsDisableTlsVerification")!;
    internal MenuItem Help => GetChildView<MenuItem>("topMenuItemHelp")!;

    internal async Task CreateNewCollection()
    {
        await File.ClickOn();
        // for some reason, this needs to be repeated (???), otherwise the menu item won't expand
        await File.ClickOn();
        await File_NewCollection.ClickOn();
        File.Close();
    }
}