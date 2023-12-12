using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Pororoca.Desktop.TextEditorConfig;
using TextMateSharp.Grammars;

namespace Pororoca.Desktop;

public enum PororocaTheme
{
    Light = 0,
    Dark = 1,
    Pampa = 2,
    AmazonianNight = 3
}

public static class PororocaThemeManager
{
    public static readonly ThemeVariant Light = ThemeVariant.Light;
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

    private static void ApplyTheme(PororocaTheme theme)
    {
        Application.Current!.RequestedThemeVariant = theme switch
        {
            PororocaTheme.Light => Light,
            PororocaTheme.Dark => Dark,
            PororocaTheme.Pampa => Pampa,
            PororocaTheme.AmazonianNight => AmazonianNight,
            _ => AmazonianNight
        };

        var pororocaVarHighlightBrush = MapPororocaVariableHighlightBrush(theme);

        TextEditorConfiguration.PororocaVariableHighlightingTransformers.ForEach(t => t.PororocaVariableHighlightBrush = pororocaVarHighlightBrush);

        // Only setting text editors' theme if the TextEditors were already setup,
        // otherwise, a wrong text editor theme will be loaded before the corresponding user saved theme
        if (TextEditorConfiguration.TextMateInstallations.Count > 0)
        {
            var textEditorThemeName = MapTextEditorTheme(theme);
            var textEditorTheme = TextEditorConfiguration.DefaultRegistryOptions!.Value!.LoadTheme(textEditorThemeName);
            TextEditorConfiguration.TextMateInstallations.ForEach(tmi =>
            {
                tmi.Item1.TextArea.TextView.LinkTextForegroundBrush = MapLinkColourForEditorTheme(theme);
                tmi.Item2.SetTheme(textEditorTheme);
            });
        }

        currentThemeField = theme;
    }

    public static ISolidColorBrush MapPororocaVariableHighlightBrush(PororocaTheme theme) =>
        theme switch
        {
            PororocaTheme.Light => Brushes.Green,
            PororocaTheme.Pampa => Brushes.Green,
            _ => Brushes.Gold
        };

    public static ThemeName MapTextEditorTheme(PororocaTheme theme) =>
        theme switch
        {
            PororocaTheme.Light => ThemeName.Light,
            PororocaTheme.Pampa => ThemeName.Light,
            PororocaTheme.Dark => ThemeName.DarkPlus,
            PororocaTheme.AmazonianNight => ThemeName.DarkPlus,
            _ => ThemeName.DarkPlus
        };

    public static IImmutableSolidColorBrush MapLinkColourForEditorTheme(PororocaTheme theme) =>
        theme switch
        {
            PororocaTheme.Light => Brushes.DarkBlue,
            PororocaTheme.Pampa => Brushes.DarkBlue,
            PororocaTheme.Dark => Brushes.LightBlue,
            PororocaTheme.AmazonianNight => Brushes.LightBlue,
            _ => Brushes.LightBlue
        };
}