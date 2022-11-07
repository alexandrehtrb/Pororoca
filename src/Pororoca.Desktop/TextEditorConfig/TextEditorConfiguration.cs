using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using Pororoca.Desktop.Localization;
using Avalonia;
using Avalonia.Platform;

namespace Pororoca.Desktop.TextEditorConfig;

internal static class TextEditorConfiguration
{
    public static CustomTextMateRegistryOptions? DefaultRegistryOptions;
    private static readonly object regOptionsLock = new();
    public static readonly List<TextMate.Installation> TextMateInstallations = new();

    private static void PrepareRegistryOptions()
    {
        if (DefaultRegistryOptions is not null)
            return;

        lock (regOptionsLock)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            DefaultRegistryOptions = new(assets!, ThemeName.DarkPlus);
        }
    }

    public static TextMate.Installation Setup(TextEditor editor, bool applyPororocaVariableHighlighting)
    {
        PrepareRegistryOptions();

        editor.ContextMenu = new ContextMenu
        {
            Items = new List<MenuItem>
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

        var jsonLanguage = DefaultRegistryOptions!.GetLanguageByExtension(".json");

        string scopeName = DefaultRegistryOptions!.GetScopeByLanguageId(jsonLanguage.Id);

        editor.Document = new(string.Empty);

        textMateInstallation.SetGrammar(DefaultRegistryOptions!.GetScopeByLanguageId(jsonLanguage.Id));

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