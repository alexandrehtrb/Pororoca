# Contributing

This is a tutorial for code contributions for the Pororoca project. Welcome!

Remember to fork this repo and develop your code starting from the `develop` branch.

* [Bug and crash reports](#bug-and-crash-reports)
* [Machine requirements for development](#machine-requirements-for-development)
* [I want to translate Pororoca to my language](#i-want-to-translate-pororoca-to-my-language)
* [I want to make my own colour theme](#i-want-to-make-my-own-colour-theme)
* [I want a custom keyboard shortcut](#i-want-a-custom-keyboard-shortcut)
* [I want a predefined or random variable](#i-want-a-predefined-or-random-variable)
* [Is there anything else I can contribute with?](#is-there-anything-else-i-can-contribute-with)

## Bug and crash reports

Fatal crashes and some other errors are logged inside the [user data folder](https://pororoca.io/docs/collections#saved-location). If you are opening a bug report, please check if there is an error log to include in the issue.

## Machine requirements for development

* [.NET 8](https://dotnet.microsoft.com)
* [VS Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/pt-br/)

The system requirements are the same as those for .NET 8. Development can be done on Linux, Windows and MacOS.

The `makereleases.ps1` and `rununittests.ps1` scripts require [PowerShell](https://github.com/PowerShell/PowerShell).

To generate the Windows Installer, you will need [NSIS](https://nsis.sourceforge.io/Main_Page) installed with `makensis` added to PATH.

## I want to translate Pororoca to my language

Pororoca uses JSON files for internationalization. 

The [WebTranslateIt](https://converter.webtranslateit.com/) online tool allows you to convert those JSON files into other formats that you might feel more comfortable working on. After finishing your translations, you can convert again to JSON. There are several tools that can help you, just search online `"automate translation json"`. Another good tool is [ResX Resource Manager](https://github.com/dotnet/ResXResourceManager).

1) Add your language to the [enum and extensions](https://github.com/alexandrehtrb/Pororoca/blob/develop/src/Pororoca.Desktop.Localization.SourceGeneration/Language.cs).

2) Create a [JSON language file](https://github.com/alexandrehtrb/Pororoca/tree/develop/src/Pororoca.Desktop/Localization) and reference it [in the csproj](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/Pororoca.Desktop.csproj#L58).

3) Insert a key `"TopMenuLanguage/YourLanguage"` in [`i18n_keys.json`](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/Localization/i18n_keys.json#L272) with translations for each language.

4) Add language to top menu ([xaml](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/Views/MainWindow.xaml#L172) and [ViewModel](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/ViewModels/MainWindowViewModel.cs#L355)).

5) Define decimal separator in [TimeTextFormatter](https://github.com/alexandrehtrb/Pororoca/blob/develop/src/Pororoca.Desktop/Localization/TimeTextFormatter.cs).

6) (RECOMMENDED) Add a README for your language.

7) (OPTIONAL) Add language to [Windows installer](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop.WindowsInstaller/Installer.nsi#L107).

8) If you want to translate the documentation to your language, open an issue on GitHub and I will concede access to the documentation website repo.

9) Done!

## I want to make my own colour theme

1) Create a new theme file inside the [Accents](https://github.com/alexandrehtrb/Pororoca/tree/develop/src/Pororoca.Desktop/Styles/Accents) folder. If dark-based, copy from `AmazonianNight`; if light-based, copy from `Pampa`. Light-based themes require a few more colour definitions than dark-based themes.

2) Reference it on [`Themes.xaml`](https://github.com/alexandrehtrb/Pororoca/blob/develop/src/Pororoca.Desktop/Styles/Themes.xaml) and [`PororocaThemeManager`](https://github.com/alexandrehtrb/Pororoca/blob/develop/src/Pororoca.Desktop/PororocaThemeManager.cs).

3) Insert a key `"TopMenuLanguage/YourTheme"` in [`i18n_keys.json`](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/Localization/i18n_keys.json#L272) with translations for each language.

4) Add theme to top menu ([xaml](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/Views/MainWindow.xaml#L84) and [ViewModel](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/ViewModels/MainWindowViewModel.cs#L375)).

5) Done!

## I want a custom keyboard shortcut

1) Shortcuts related to the collection tree (left-side panel) are registered in [`CollectionsGroupView.xaml`](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/Views/CollectionsGroupView.xaml#L149). Shortcuts related to the main window reside in [`MainWindow.xaml`](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/Views/MainWindow.xaml#L23).

2) Add ViewModel [command and logic](https://github.com/alexandrehtrb/Pororoca/blob/78abc423c8f61c99331d85b4ab19ff304ae155d6/src/Pororoca.Desktop/HotKeys/KeyboardShortcuts.cs#L16).

## I want a predefined or random variable

Add logic in [`PororocaPredefinedVariableEvaluator`](https://github.com/alexandrehtrb/Pororoca/blob/develop/src/Pororoca.Domain/Features/VariableResolution/PororocaPredefinedVariableEvaluator.cs).

## Is there anything else I can contribute with?

Yes! Check the GitHub issues track.
