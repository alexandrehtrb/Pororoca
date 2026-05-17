using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.TextMate;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.VariableResolution.PororocaPredefinedVariableEvaluator;

namespace Pororoca.Desktop.TextEditorConfig;

internal static class TextEditorConfiguration
{
    public static readonly Lazy<CustomTextMateRegistryOptions> DefaultRegistryOptions = new(LoadDefaultRegistryOptions);
    public static readonly List<(TextEditor, TextMate.Installation)> TextMateInstallations = new();
    private static readonly List<Action> invalidateTextEditorsAreasCallbacks = new();

    private static CustomTextMateRegistryOptions LoadDefaultRegistryOptions() =>
        new(PororocaThemeManager.TextEditorThemeName);

    public static TextMate.Installation Setup(TextEditor editor, bool applyPororocaVariableHighlighting, Func<IPororocaVariableResolver>? varResolverObtainer)
    {
        DefaultRegistryOptions.Value.PreLoadPororocaJsonGrammar();

        editor.ContextMenu = new ContextMenu
        {
            ItemsSource = new List<MenuItem>
            {
                // TODO: fix text editors' context menu (i18n and actions not working)
                new() { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                new() { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                new() { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
            }
        };
        editor.Options.ShowBoxForControlCharacters = true;
        editor.Options.EnableEmailHyperlinks = true;
        editor.Options.EnableHyperlinks = true;
        editor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.DefaultIndentationStrategy();
        editor.GetObservable(Visual.BoundsProperty).Subscribe(bounds => editor.TextArea.Width = bounds.Width);
        editor.TextArea.RightClickMovesCaret = true;
        editor.TextArea.TextView.LinkTextForegroundBrush = PororocaThemeManager.TextEditorHyperlinkForegroundBrush;

        var textMateInstallation = editor.InstallTextMate(DefaultRegistryOptions.Value!, false);
        // the line below must be added only after the TextMate installation above
        // otherwise, the pororoca variable highlighting may be bugged
        if (applyPororocaVariableHighlighting)
        {
            ArgumentNullException.ThrowIfNull(varResolverObtainer, nameof(varResolverObtainer));
            PororocaVariableColorizingTransformer pororocaVarLineTransformer = new(varResolverObtainer);
            invalidateTextEditorsAreasCallbacks.Add(editor.TextArea.TextView.Redraw);
            editor.TextArea.TextView.LineTransformers.Add(pororocaVarLineTransformer);
            editor.TextArea.SelectionBrush = PororocaThemeManager.TextEditorSelectionHighlightBrush;
            editor.PointerHover += (sender, e) => OnTextEditorPointerHover(sender, e, varResolverObtainer!);
            editor.PointerHoverStopped += OnTextEditorPointerHoverStopped;
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

    internal static void InvalidateTextEditorsAreas() =>
        Dispatcher.UIThread.Post(() => invalidateTextEditorsAreasCallbacks.ForEach(c => c()));

    #region HOVER VARIABLE POPUP

    private static void OnTextEditorPointerHover(object? sender, PointerEventArgs e, Func<IPororocaVariableResolver> varResolverObtainer)
    {
        try
        {
            var te = ((TextEditor)sender!);
            var pos = te.GetPositionFromPoint(e.GetPosition(te));
            if (pos != null && pos.HasValue)
            {
                var line = te.Document.GetLineByNumber(pos.Value.Line);
                string lineText = te.Document.GetText(line.Offset, line.Length);
                int pointerIndex = pos.Value.Column - 1;

                string? hoveringVar = IPororocaVariableResolver.GetPointerHoverVariable(lineText, pointerIndex);

                if (hoveringVar != null)
                {
                    var flyout = (Flyout)(FlyoutBase.GetAttachedFlyout(te)!);
                    var flyoutStb = te.FindControl<SelectableTextBlock>("stbPopupText")!;
                    flyoutStb.Classes.RemoveAll(["NoMatchingVariable", "PredefinedVariable"]);

                    if (IsPredefinedVariable(hoveringVar))
                    {
                        // predefined var
                        flyoutStb.Classes.Add("PredefinedVariable");
                        flyoutStb.Text = Localizer.Instance.HoverVariableInEditor.PredefinedVariable;
                    }
                    else
                    {
                        var varResolver = varResolverObtainer();
                        if (varResolver.IsEffectiveVariable(hoveringVar))
                        {
                            // regular var
                            var effectiveVars = varResolver.GetEffectiveVariables();
                            string resolvedVar = effectiveVars.FirstOrDefault(v => v.Key == hoveringVar)?.Value ?? string.Empty;
                            flyoutStb.Text = resolvedVar;
                        }
                        else
                        {
                            // undefined
                            flyoutStb.Classes.Add("NoMatchingVariable");
                            flyoutStb.Text = Localizer.Instance.HoverVariableInEditor.NoMatchingVariable;
                        }
                    }

                    flyout.ShowAt(te, showAtPointer: true);
                    e.Handled = true;
                }
            }
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Warning, "Error when hovering mouse over text editor.", ex);
        }
    }

    private static void OnTextEditorPointerHoverStopped(object? sender, PointerEventArgs e)
    {
        try
        {
            var flyout = FlyoutBase.GetAttachedFlyout((TextEditor)sender!)!;
            if (flyout.IsOpen)
            {
                flyout!.Hide();
            }
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Warning, "Error when stopped hovering mouse over text editor.", ex);
        }
    }

    #endregion

    #region AUTO COMPLETE VARIABLES

    internal static void OnTextEnteringInEditorWithVariables(TextEditor textEditor, TextInputEventArgs e, ref CompletionWindow? completionWindow)
    {
        if (!string.IsNullOrWhiteSpace(e.Text) && completionWindow != null)
        {
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        // Do not set e.Handled=true.
        // We still want to insert the character that was typed.
    }

    internal static void OnTextEnteredInEditorWithVariables(TextEditor textEditor, TextInputEventArgs e, Func<IPororocaVariableResolver> varResolverObtainer, ref CompletionWindow? completionWindow, EventHandler<EventArgs> onCompletionWindowClosed)
    {
        int caretOffset = textEditor.TextArea.Caret.Offset;
        // input char is '{' and previous char is also '{'
        if (
            (e.Text == "{" && (caretOffset >= 2) && (textEditor.Text[caretOffset - 2] == '{'))
         || (e.Text == " " && (caretOffset >= 3) && (textEditor.Text[caretOffset - 3] == '{') && (textEditor.Text[caretOffset - 2] == '{'))
            )
        {
            char enteredChar = e.Text[0];
            var effectiveVars = varResolverObtainer().GetEffectiveVariables();

            if (effectiveVars.Any())
            {
                completionWindow = new CompletionWindow(textEditor.TextArea);
                // below line: closing " }}" after selection and insertion
                completionWindow.CompletionList.InsertionRequested += (sender, e) =>
                    textEditor.TextArea.PerformTextInput(
                        enteredChar == ' ' ? " }}" : "}}");
                completionWindow.Closed += onCompletionWindowClosed;// (o, args) => completionWindow = null;

                var data = completionWindow.CompletionList.CompletionData;

                foreach (var v in effectiveVars)
                {
                    data.Add(new PororocaVariableTextEditorCompletionData(v));
                }

                completionWindow.Show();
            }
        }
    }

    #endregion
}