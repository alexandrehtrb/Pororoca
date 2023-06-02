using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using Pororoca.Desktop.Localization;
using TextMateSharp.Grammars;

namespace Pororoca.Desktop.TextEditorConfig;

internal static class TextEditorConfiguration
{
    public static readonly CustomTextMateRegistryOptions DefaultRegistryOptions = new(ThemeName.DarkPlus);
    public static readonly List<TextMate.Installation> TextMateInstallations = new();

    public static TextMate.Installation Setup(TextEditor editor, bool applyPororocaVariableHighlighting)
    {
        DefaultRegistryOptions.PreLoadPororocaJsonGrammar();

        editor.ContextMenu = new ContextMenu
        {
            ItemsSource = new List<MenuItem>
            {
                new MenuItem { Header = Localizer.Instance["TextEditor/Copy"], InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                new MenuItem { Header = Localizer.Instance["TextEditor/Paste"], InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                new MenuItem { Header = Localizer.Instance["TextEditor/Cut"], InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
            }
        };
        editor.Options.ShowBoxForControlCharacters = true;
        editor.Options.EnableEmailHyperlinks = false;
        editor.Options.EnableHyperlinks = false;
        editor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.DefaultIndentationStrategy();
        editor.TextArea.RightClickMovesCaret = true;

        var textMateInstallation = editor.InstallTextMate(DefaultRegistryOptions!);
        // the line below must be added only after the TextMate installation above
        // otherwise, the pororoca variable highlighting may be bugged
        if (applyPororocaVariableHighlighting)
        {
            editor.TextArea.TextView.LineTransformers.Add(new PororocaVariableColorizingTransformer());
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

        TextMateInstallations.Add(textMateInstallation);
        return textMateInstallation;
    }
}