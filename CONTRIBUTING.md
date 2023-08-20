# Contributing

This is a tutorial for code contributions for the Pororoca project. Welcome!

Remember to fork this repo and develop your code starting from the `develop` branch.

## I want to translate Pororoca to my language

1) In the `Pororoca.Desktop.Localization.SourceGeneration` project, add your language to the enum and extensions.

2) In the `Pororoca.Desktop` project, create a new JSON file with your language strings, inside the Localization folder.

3) Insert a new key in the `i18n_keys.json` file, of your language name, like: `"TopMenuLanguage/YourLanguage",`.

4) Edit `MainWindow.xaml` and `MainWindowViewModel.cs` to add your language to the top menu.

5) (OPTIONAL) Add a README for your language.

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

Yes! Currently, we need help with:

* UI testing, preferrably using [Appium](https://github.com/AvaloniaUI/Avalonia/tree/master/tests/Avalonia.IntegrationTests.Appium). (there are no automated UI tests yet)
* Migration to .NET 8
* NativeAOT for Pororoca.Desktop