using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Pororoca.Desktop.TextEditorConfig;
using TextMateSharp.Grammars;

namespace Pororoca.Desktop;

public enum PororocaTheme
{
    Light = 0,
    Dark = 1,
    Pampa = 2,
    AmazonianNight = 3,
    Light2 = 4,
}

public static class PororocaThemeManager
{
    public static readonly ThemeVariant Light = ThemeVariant.Light;
    public static readonly ThemeVariant Light2 = new("Light2", ThemeVariant.Light);
    public static readonly ThemeVariant Dark = ThemeVariant.Dark;
    public static readonly ThemeVariant Pampa = new("Pampa", ThemeVariant.Light);
    public static readonly ThemeVariant AmazonianNight = new("AmazonianNight", ThemeVariant.Dark);

    private static PororocaTheme currentThemeField;

    public static PororocaTheme CurrentTheme
    {
        get => currentThemeField;
        set => ApplyTheme(value);
    }

    public static PororocaTheme DefaultTheme => PororocaTheme.Dark;

    public static SolidColorBrush RegularVariableForegroundBrush =>
        GetResource<SolidColorBrush>("RegularVariableForegroundBrush");

    public static SolidColorBrush PredefinedVariableForegroundBrush =>
        GetResource<SolidColorBrush>("PredefinedVariableForegroundBrush");

    public static SolidColorBrush NoMatchingVariableForegroundBrush =>
        GetResource<SolidColorBrush>("NoMatchingVariableForegroundBrush");

    public static SolidColorBrush PVSHTextBlockDefaultForegroundBrush =>
        GetResource<SolidColorBrush>("PVSHTextBlockDefaultForegroundBrush");

    public static ThemeName TextEditorThemeName =>
        Enum.Parse<ThemeName>(GetResource<string>("TextEditorTheme"));

    public static SolidColorBrush TextEditorHyperlinkForegroundBrush =>
        GetResource<SolidColorBrush>("TextEditorHyperlinkForegroundBrush");

    public static SolidColorBrush TextEditorSelectionHighlightBrush =>
        GetResource<SolidColorBrush>("TextControlSelectionHighlightColor");

    private static void ApplyTheme(PororocaTheme theme)
    {
        var themeVariant = theme switch
        {
            PororocaTheme.Light => Light,
            PororocaTheme.Light2 => Light2,
            PororocaTheme.Dark => Dark,
            PororocaTheme.Pampa => Pampa,
            PororocaTheme.AmazonianNight => AmazonianNight,
            _ => AmazonianNight
        };

        Application.Current!.RequestedThemeVariant = themeVariant;

        // GAMBIARRA to fix SystemAccentColor not being updated from Avalonia 12 onwards
        // due to new color palette system
        // https://docs.avaloniaui.net/docs/styling/themes#creating-custom-color-palettes
        // https://github.com/Mbucari/AvaloniaThemeColorChanger/blob/master/AvaloniaThemeColorChanger/AvaloniaTheme.cs
        // https://github.com/AvaloniaUI/Avalonia/discussions/12042#discussioncomment-6366474
        var fluentTheme = Application.Current!.Styles.OfType<FluentTheme>().Single();
        var baseThemeVariant = themeVariant == ThemeVariant.Light || themeVariant == ThemeVariant.Dark ?
                               themeVariant : themeVariant.InheritVariant!;
        var accentColor = GetResource<Color>("SystemAccentColor");
        if (fluentTheme.Palettes.TryGetValue(baseThemeVariant, out var colorPaletteResources))
        {
            colorPaletteResources.Accent = accentColor;
        }
        else
        {
            colorPaletteResources = new();
            colorPaletteResources.Accent = accentColor;
            fluentTheme.Palettes.Add(baseThemeVariant, colorPaletteResources);
        }

        // Only setting text editors' theme if the TextEditors were already setup,
        // otherwise, a wrong text editor theme will be loaded before the corresponding user saved theme
        if (TextEditorConfiguration.TextMateInstallations.Count > 0)
        {
            var textEditorTheme = TextEditorConfiguration.DefaultRegistryOptions!.Value!.LoadTheme(TextEditorThemeName);
            TextEditorConfiguration.TextMateInstallations.ForEach(tmi =>
            {
                tmi.Item1.TextArea.SelectionBrush = TextEditorSelectionHighlightBrush;
                tmi.Item1.TextArea.TextView.LinkTextForegroundBrush = TextEditorHyperlinkForegroundBrush;
                tmi.Item2.SetTheme(textEditorTheme);
            });
        }

        currentThemeField = theme;
    }

    private static T GetResource<T>(string key)
    {
        Application.Current!.TryGetResource(
            key,
            Application.Current!.RequestedThemeVariant,
            out object? resourceValue);

        return (T)resourceValue!;
    }
}