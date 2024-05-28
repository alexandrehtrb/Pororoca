# Contributing

This is a tutorial for code contributions for the Pororoca project. Welcome!

Remember to fork this repo and develop your code starting from the `develop` branch.

## Machine requirements

* [.NET 8](https://dotnet.microsoft.com)
* [VS Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/pt-br/)

The system requirements are the same as those for .NET 8. Development can be done on Linux, Windows and MacOS.

If you want to run `makereleases.ps1` or `rununittests.ps1` scripts, you will need [PowerShell](https://github.com/PowerShell/PowerShell).

To generate the Windows Installer, you will need [NSIS](https://nsis.sourceforge.io/Main_Page) installed and with `makensis` added to PATH.

## I want to translate Pororoca to my language

1) In the **Pororoca.Desktop.Localization.SourceGeneration** project, add your language to the enum and extensions.

2) In the **Pororoca.Desktop** project, create a new JSON file with your language strings, inside the Localization folder, and reference in AdditionalFiles inside `Pororoca.Desktop.csproj`.

3) Insert a new key in the `i18n_keys.json` file, of your language name, like: `"TopMenuLanguage/YourLanguage",`. The other languages files will need a translation for this key.

4) Edit `MainWindow.xaml` and `MainWindowViewModel.cs` to add your language to the top menu.

5) (OPTIONAL) Add a README for your language.

6) (OPTIONAL) Add your language to the Windows installer: `src/Pororoca.Desktop.WindowsInstaller/Installer.nsi`.

7) If you want to translate the documentation to your language, open an issue on GitHub and I will concede access to the documentation website repo.

6) Done!

## I want to make my own colour theme

1) Create a new theme in the `Styles\Accents` folder. If your theme is dark-based, start your new theme copying from the `AmazonianNight.xaml` theme. If it is light-based, copy from the `Pampa.xaml` theme. Light-based themes require a few more colour definitions than dark-based themes.

2) Add your theme in the `Styles\Themes.xaml` and `PororocaThemeManager.cs` files. The PororocaThemeManager also controls the text editor theme and Pororoca variable highlight colour for your theme.

3) Insert a key and translations for your theme name in the Localization files. The necessary key will be: `"TopMenuTheme/YourThemeName"`.

4) Edit `MainWindow.xaml` and `MainWindowViewModel.cs` to add your theme to the top menu.

5) Done!

## I want a custom keyboard shortcut

1) If the keyboard shortcut is related to the collection tree (left-side panel), then the keybinding needs to be registered in `CollectionsGroupView.xaml`. If it is related to the main window, then the keybinding resides in the `MainWindow.xaml` file.

2) Add a ViewModel command and logic for your keyboard shortcut in the `KeyboardShortcuts.cs` file.

## Is there anything else I can contribute with?

Yes! Check the GitHub issues track.