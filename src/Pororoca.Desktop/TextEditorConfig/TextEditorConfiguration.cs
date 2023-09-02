using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using Pororoca.Desktop.Localization;

namespace Pororoca.Desktop.TextEditorConfig;

internal static class TextEditorConfiguration
{
    public static readonly Lazy<CustomTextMateRegistryOptions> DefaultRegistryOptions = new(LoadDefaultRegistryOptions);
    public static readonly List<(TextEditor, TextMate.Installation)> TextMateInstallations = new();
    public static readonly List<PororocaVariableColorizingTransformer> PororocaVariableHighlightingTransformers = new();

    private static CustomTextMateRegistryOptions LoadDefaultRegistryOptions()
    {
        var initialTheme = PororocaThemeManager.MapTextEditorTheme(PororocaThemeManager.CurrentTheme);
        return new(initialTheme);
    }

    public static TextMate.Installation Setup(TextEditor editor, bool applyPororocaVariableHighlighting)
    {
        DefaultRegistryOptions.Value.PreLoadPororocaJsonGrammar();

        editor.ContextMenu = new ContextMenu
        {
            ItemsSource = new List<MenuItem>
            {
                // TODO: fix text editors' context menu (i18n and actions not working)
                new MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                new MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                new MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
            }
        };
        editor.Options.ShowBoxForControlCharacters = true;
        editor.Options.EnableEmailHyperlinks = true;
        editor.Options.EnableHyperlinks = true;
        editor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.DefaultIndentationStrategy();
        editor.TextArea.RightClickMovesCaret = true;
        editor.TextArea.TextView.LinkTextForegroundBrush = PororocaThemeManager.MapLinkColourForEditorTheme(PororocaThemeManager.CurrentTheme);

        var textMateInstallation = editor.InstallTextMate(DefaultRegistryOptions.Value!);
        // the line below must be added only after the TextMate installation above
        // otherwise, the pororoca variable highlighting may be bugged
        if (applyPororocaVariableHighlighting)
        {
            var initialVarHighlightBrush = PororocaThemeManager.MapPororocaVariableHighlightBrush(PororocaThemeManager.CurrentTheme);
            PororocaVariableColorizingTransformer transformer = new(initialVarHighlightBrush);
            PororocaVariableHighlightingTransformers.Add(transformer);
            editor.TextArea.TextView.LineTransformers.Add(transformer);
        }

        editor.Document = new(string.Empty);

        editor.AddHandler(InputElement.PointerWheelChangedEvent, (o, i) =>
        {
            if (i.KeyModifiers != KeyModifiers.Control)
                return;
            if (i.Delta.Y > 0)
                editor.FontSize++;
            else
                editor.FontSize = editor.FontSize > 1 ? editor.FontSize - 1 : 1;
        }, RoutingStrategies.Bubble, true);

        TextMateInstallations.Add((editor, textMateInstallation));
        return textMateInstallation;
    }
}