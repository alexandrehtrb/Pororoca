// The SyntaxHighlightingTextBox code was originally taken
// from the ULogViewer project, by Carina Studio.
//
// https://github.com/carina-studio/AppSuiteBase/tree/master/SyntaxHighlighting
//
// MIT License
// 
// Copyright(c) 2021 Carina Studio
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.Utilities;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.Controls;

/// <summary>
/// ULTIMATE GAMBIARRA SOLUTION.
/// Let's make an exact replica of the original TextPresenter,
/// hijacking the CreateTextLayout method to be just like the original,
/// but passing additional `textStyleOverrides` that will modify some parts of text
/// - in our case, to apply syntax highlighting. 
/// Private fields and methods are accessed via UnsafeAccessor attributes, available from .NET 8 onwards.
/// https://www.meziantou.net/accessing-private-members-without-reflection-in-csharp.htm
/// https://github.com/AvaloniaUI/Avalonia/blob/release/11.0.5/src/Avalonia.Controls/Presenters/TextPresenter.cs
/// TODO: Update this code when migrating to newer versions of Avalonia.
/// </summary>
public class SyntaxHighlightingTextPresenter : TextPresenter
{
    /// <summary>
    /// Property of <see cref="DefinitionSet"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlightingTextPresenter, SyntaxHighlightingDefinitionSet?> DefinitionSetProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlightingTextPresenter, SyntaxHighlightingDefinitionSet?>(nameof(DefinitionSet), t => t.DefinitionSet, (t, ds) => t.DefinitionSet = ds);

    private static readonly List<Action> invalidateSyntaxHighlightersTextBoxesTextsCallbacks = new();

    private readonly Dictionary<int, TextRunProperties> cachedTextRunProps = new();
    private GenericTextRunProperties? defaultTextRunProperties;

    /// <summary>
    /// Get or set syntax highlighting definition set.
    /// </summary>
    public SyntaxHighlightingDefinitionSet? DefinitionSet { get; set; }

    public SyntaxHighlightingTextPresenter() : base()
    {
        invalidateSyntaxHighlightersTextBoxesTextsCallbacks.Add(InvalidateTextLayout);
    }

    /// <summary>
    /// This method gets the 'private Size _constraint;' field from TextPresenter class.
    /// Use "ref" to get a reference to the field, so you can read and write to it.
    /// </summary>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_constraint")]
    extern static ref Size GetSizeConstraint(TextPresenter @this);

    /// <summary>
    /// This method executes the 'private static string? GetCombinedText(string? text, int caretIndex, string? preeditText)' method from TextPresenter class.
    /// The first argument must be of the type containting the private method.
    /// Even if a static method doesn't use an instance, the runtime needs to know
    /// the type of the class.
    /// </summary>
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "GetCombinedText")]
    extern static string? GetCombinedText(TextPresenter @this, string? text, int caretIndex, string? preeditText);

    /// <summary>
    /// This method executes the 'private TextLayout CreateTextLayoutInternal(TextPresenter @this, Size constraint, string? text, Typeface typeface, IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides)' method from TextPresenter class.
    /// The first argument is the instance of the class containing the private method.
    /// Needs to be static even if original method is not.
    /// </summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "CreateTextLayoutInternal")]
    extern static TextLayout CreateTextLayoutInternal(TextPresenter @this, Size constraint, string? text, Typeface typeface,
        IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides);

    /// <summary>
    /// This is an exact replica of the original CreateTextLayout from TextPresenter,
    /// hijacking the CreateTextLayout method to be just like the original,
    /// but passing additional `textStyleOverrides` that will modify some parts of text
    /// - in our case, to apply syntax highlighting. 
    /// Private fields and methods are accessed via UnsafeAccessor attributes, available from .NET 8 onwards.
    /// </summary>
    /// <returns></returns>
    protected override TextLayout CreateTextLayout()
    {
        TextLayout result;

        var caretIndex = CaretIndex;
        var preeditText = PreeditText;
        var text = GetCombinedText(this, Text, caretIndex, preeditText);
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;
        var start = Math.Min(selectionStart, selectionEnd);
        var length = Math.Max(selectionStart, selectionEnd) - start;

        List<ValueSpan<TextRunProperties>>? textStyleOverrides = null;

        this.defaultTextRunProperties ??= new GenericTextRunProperties(
            typeface,
            FontSize,
            null,//TextDecorations,
            Foreground,
            Background
        );

        if (text != null)
        {
            textStyleOverrides = CreateTextProperties(this.defaultTextRunProperties, text);
        }

        var foreground = Foreground;

        if (!string.IsNullOrEmpty(preeditText))
        {
            var preeditHighlight = new ValueSpan<TextRunProperties>(caretIndex, preeditText.Length,
                    new GenericTextRunProperties(typeface, FontSize,
                    foregroundBrush: foreground,
                    textDecorations: TextDecorations.Underline));

            if (textStyleOverrides == null)
            {
                textStyleOverrides = [preeditHighlight];
            }
            else
            {
                textStyleOverrides.Add(preeditHighlight);
            }
        }
        else
        {
            if (length > 0 && SelectionForegroundBrush != null)
            {
                var selectionHighlight = new ValueSpan<TextRunProperties>(start, length,
                    new GenericTextRunProperties(typeface, FontSize,
                        foregroundBrush: SelectionForegroundBrush));

                if (textStyleOverrides == null)
                {
                    textStyleOverrides = [selectionHighlight];
                }
                else
                {
                    textStyleOverrides.Add(selectionHighlight);
                }
            }
        }

        // below needs to be ref,
        // elsewise it will throw a BadImageFormatException: 'Invalid usage of UnsafeAccessorAttribute.'.
        ref Size constraint = ref GetSizeConstraint(this);
        if (PasswordChar != default(char) && !RevealPassword)
        {
            result = CreateTextLayoutInternal(this, constraint, new string(PasswordChar, text?.Length ?? 0), typeface,
                textStyleOverrides);
        }
        else
        {
            result = CreateTextLayoutInternal(this, constraint, text, typeface, textStyleOverrides);
        }

        return result;
    }

    private List<ValueSpan<TextRunProperties>>? CreateTextProperties(TextRunProperties defaultRunProperties, string text)
    {
        var defs = DefinitionSet?.TokenDefinitions ?? [];
        var parts = RegexUtils.DelimitTextPartsOverRegexes(defs, text);

        if (parts.Count == 1 && parts[0].Pattern == null)
        {
            // no syntax highlighting
            // this means no text style overrides
            return null;
        }

        List<ValueSpan<TextRunProperties>> textProperties = new();
        foreach (var (definition, start, length, match) in parts)
        {
            // The if condition below code is NECESSARY,
            // because otherwise it will extend the highlighting for some reason.
            // For example:
            // "{{ aa }} some text {{ bb }}"
            // With the condition, ' some text ' remains with the default text colour.
            // Without the condition below, ' some text ' will get coloured too.
            if (definition == null)
            {
                // normal text
                textProperties.Add(new(start, length, defaultRunProperties));
            }
            else
            {
                // token to be highlighted
                int cacheId, regexMatchId;
                if (definition.RegexMatchIdMapper != null)
                {
                    // style varies according to regex match.
                    regexMatchId = definition.RegexMatchIdMapper(match!);
                    cacheId = definition.Id * 16 + regexMatchId;
                }
                else
                {
                    // fixed style for token. 
                    regexMatchId = -1;
                    cacheId = definition.Id;
                }

                if (!this.cachedTextRunProps.TryGetValue(cacheId, out var tokenRunProperties))
                {
                    tokenRunProperties =
                        this.cachedTextRunProps[cacheId] =
                        definition.MakeTextProperties(defaultRunProperties, FontStretch, regexMatchId);
                }
                textProperties.Add(new(start, length, tokenRunProperties));
            }
        }
        return textProperties;
    }

    protected override void InvalidateTextLayout()
    {
        this.defaultTextRunProperties = null;
        base.InvalidateTextLayout();
    }

    internal static void InvalidateSyntaxHighlighterTextBoxesTexts() =>
        Dispatcher.UIThread.Post(() => invalidateSyntaxHighlightersTextBoxesTextsCallbacks.ForEach(c => c()));
}