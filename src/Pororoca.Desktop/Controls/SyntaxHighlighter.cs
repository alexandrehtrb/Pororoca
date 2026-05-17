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

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.Utilities;
using Pororoca.Desktop.Others;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.Controls;

/// <summary>
/// Syntax highlighter.
/// </summary>
public sealed class SyntaxHighlighter : AvaloniaObject, IDisposable
{
    /// <summary>
    /// Property of <see cref="Background"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, IBrush?> BackgroundProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, IBrush?>(nameof(Background), sh => sh.Background, (sh, b) => sh.Background = b);
    /// <summary>
    /// Property of <see cref="FlowDirection"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, FlowDirection> FlowDirectionProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, FlowDirection>(nameof(FlowDirection), sh => sh.FlowDirection, (sh, d) => sh.FlowDirection = d);
    /// <summary>
    /// Property of <see cref="FontFamily"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, FontFamily> FontFamilyProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, FontFamily>(nameof(FontFamily), sh => sh.FontFamily, (sh, f) => sh.FontFamily = f);
    /// <summary>
    /// Property of <see cref="FontStretch"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, FontStretch> FontStretchProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, FontStretch>(nameof(FontStretch), sh => sh.FontStretch, (sh, s) => sh.FontStretch = s);
    /// <summary>
    /// Property of <see cref="FontSize"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, double> FontSizeProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, double>(nameof(FontSize), sh => sh.FontSize, (sh, s) => sh.FontSize = s);
    /// <summary>
    /// Property of <see cref="FontStyle"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, FontStyle> FontStyleProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, FontStyle>(nameof(FontStyle), sh => sh.FontStyle, (sh, s) => sh.FontStyle = s);
    /// <summary>
    /// Property of <see cref="FontWeight"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, FontWeight> FontWeightProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, FontWeight>(nameof(FontWeight), sh => sh.FontWeight, (sh, w) => sh.FontWeight = w);
    /// <summary>
    /// Property of <see cref="Foreground"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, IBrush?> ForegroundProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, IBrush?>(nameof(Foreground), sh => sh.Foreground, (sh, b) => sh.Foreground = b);
    /// <summary>
    /// Property of <see cref="DefinitionSet"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, SyntaxHighlightingDefinitionSet?> DefinitionSetProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, SyntaxHighlightingDefinitionSet?>(nameof(DefinitionSet), sh => sh.DefinitionSet, (sh, ds) => sh.DefinitionSet = ds);
    /// <summary>
    /// Property of <see cref="LetterSpacing"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, double> LetterSpacingProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, double>(nameof(LetterSpacing), sh => sh.LetterSpacing, (sh, s) => sh.LetterSpacing = s);
    /// <summary>
    /// Property of <see cref="FlowDirection"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, double> LineHeightProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, double>(nameof(LineHeight), sh => sh.LineHeight, (sh, h) => sh.LineHeight = h);
    /// <summary>
    /// Property of <see cref="MaxHeight"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, double> MaxHeightProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, double>(nameof(MaxHeight), sh => sh.MaxHeight, (sh, h) => sh.MaxHeight = h);
    /// <summary>
    /// Property of <see cref="MaxLines"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, int> MaxLinesProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, int>(nameof(MaxLines), sh => sh.MaxLines, (sh, l) => sh.MaxLines = l);
    /// <summary>
    /// Property of <see cref="MaxTokenCount"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, int> MaxTokenCountProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, int>(nameof(MaxTokenCount), sh => sh.MaxTokenCount, (sh, c) => sh.MaxTokenCount = c);
    /// <summary>
    /// Property of <see cref="MaxWidth"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, double> MaxWidthProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, double>(nameof(MaxWidth), sh => sh.MaxWidth, (sh, w) => sh.MaxWidth = w);
    /// <summary>
    /// Property of <see cref="PreeditText"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, string?> PreeditTextProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, string?>(nameof(PreeditText), sh => sh.PreeditText, (sh, t) => sh.PreeditText = t);
    /// <summary>
    /// Property of <see cref="SelectionBackground"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, IBrush?> SelectionBackgroundProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, IBrush?>(nameof(SelectionBackground), sh => sh.SelectionBackground, (sh, b) => sh.SelectionBackground = b);
    /// <summary>
    /// Property of <see cref="SelectionEnd"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, int> SelectionEndProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, int>(nameof(SelectionEnd), sh => sh.SelectionEnd, (sh, i) => sh.SelectionEnd = i);
    /// <summary>
    /// Property of <see cref="SelectionForeground"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, IBrush?> SelectionForegroundProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, IBrush?>(nameof(SelectionForeground), sh => sh.SelectionForeground, (sh, b) => sh.SelectionForeground = b);
    /// <summary>
    /// Property of <see cref="SelectionStart"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, int> SelectionStartProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, int>(nameof(SelectionStart), sh => sh.SelectionStart, (sh, i) => sh.SelectionStart = i);
    /// <summary>
    /// Property of <see cref="Text"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, string?> TextProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, string?>(nameof(Text), sh => sh.Text, (sh, t) => sh.Text = t);
    /// <summary>
    /// Property of <see cref="TextAlignment"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, TextAlignment> TextAlignmentProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, TextAlignment>(nameof(TextAlignment), sh => sh.TextAlignment, (sh, a) => sh.TextAlignment = a);
    /// <summary>
    /// Property of <see cref="TextDecorations"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, TextDecorationCollection?> TextDecorationProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, TextDecorationCollection?>(nameof(TextDecorations), sh => sh.TextDecorations, (sh, d) => sh.TextDecorations = d);
    /// <summary>
    /// Property of <see cref="TextTrimming"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, TextTrimming> TextTrimmingProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, TextTrimming>(nameof(TextTrimming), sh => sh.TextTrimming, (sh, t) => sh.TextTrimming = t);
    /// <summary>
    /// Property of <see cref="TextWrapping"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlighter, TextWrapping> TextWrappingProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlighter, TextWrapping>(nameof(TextWrapping), sh => sh.TextWrapping, (sh, w) => sh.TextWrapping = w);

    private static readonly List<Action> invalidateSyntaxHighlightersTextBoxesTextsCallbacks = new();

    // Fields.
    private WeakEventHandlerAdapter<AvaloniaObject, AvaloniaPropertyChangedEventArgs>? backgroundPropertyChangedHandlerToken = null;
    private WeakEventHandlerAdapter<AvaloniaObject, AvaloniaPropertyChangedEventArgs>? foregroundPropertyChangedHandlerToken;
    private WeakEventHandlerAdapter<AvaloniaObject, AvaloniaPropertyChangedEventArgs>? selectionBackgroundPropertyChangedHandlerToken;
    private WeakEventHandlerAdapter<AvaloniaObject, AvaloniaPropertyChangedEventArgs>? selectionForegroundPropertyChangedHandlerToken;
    private readonly Dictionary<int, TextRunProperties> cachedTextRunProps = new();
    private readonly List<ValueSpan<TextRunProperties>> textProperties = new();
    private TextLayout? textLayout;

    /// <summary>
    /// Initialize new <see cref="SyntaxHighlighter"/> instance.
    /// </summary>
    public SyntaxHighlighter()
    {
        invalidateSyntaxHighlightersTextBoxesTextsCallbacks.Add(InvalidateTextProperties);
    }

    /// <summary>
    /// Get or set base background brush.
    /// </summary>
    private IBrush? backgroundField;
    public IBrush? Background
    {
        get => this.backgroundField;
        set
        {
            VerifyAccess();
            if (ReferenceEquals(this.backgroundField, value))
                return;
            this.backgroundPropertyChangedHandlerToken?.Dispose();
            if (value is AvaloniaObject aobj)
            {
                this.backgroundPropertyChangedHandlerToken = new(aobj, nameof(AvaloniaObject.PropertyChanged), OnBrushPropertyChanged);
            }
            SetAndRaise(BackgroundProperty, ref this.backgroundField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set syntax highlighting definition set.
    /// </summary>
    private SyntaxHighlightingDefinitionSet? definitionSetField;
    public SyntaxHighlightingDefinitionSet? DefinitionSet
    {
        get => this.definitionSetField;
        set
        {
            VerifyAccess();
            if (this.definitionSetField == value)
                return;
            SetAndRaise(DefinitionSetProperty, ref this.definitionSetField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set flow direction.
    /// </summary>
    private FlowDirection flowDirectionField = FlowDirection.LeftToRight;
    public FlowDirection FlowDirection
    {
        get => this.flowDirectionField;
        set
        {
            VerifyAccess();
            if (this.flowDirectionField == value)
                return;
            SetAndRaise(FlowDirectionProperty, ref this.flowDirectionField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set base font family.
    /// </summary>
    private FontFamily fontFamilyField = FontManager.Current.DefaultFontFamily;
    public FontFamily FontFamily
    {
        get => this.fontFamilyField;
        set
        {
            VerifyAccess();
            if (this.fontFamilyField == value)
                return;
            SetAndRaise(FontFamilyProperty, ref this.fontFamilyField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set base font size.
    /// </summary>
    private double fontSizeField = 12;
    public double FontSize
    {
        get => this.fontSizeField;
        set
        {
            VerifyAccess();
            if (Math.Abs(this.fontSizeField - value) <= 0.01)
                return;
            SetAndRaise(FontSizeProperty, ref this.fontSizeField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set base font stretch.
    /// </summary>
    private FontStretch fontStretchField = FontStretch.Normal;
    public FontStretch FontStretch
    {
        get => this.fontStretchField;
        set
        {
            VerifyAccess();
            if (this.fontStretchField == value)
                return;
            SetAndRaise(FontStretchProperty, ref this.fontStretchField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set base font style.
    /// </summary>
    private FontStyle fontStyleField = FontStyle.Normal;
    public FontStyle FontStyle
    {
        get => this.fontStyleField;
        set
        {
            VerifyAccess();
            if (this.fontStyleField == value)
                return;
            SetAndRaise(FontStyleProperty, ref this.fontStyleField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set base font weight.
    /// </summary>
    private FontWeight fontWeightField = FontWeight.Normal;
    public FontWeight FontWeight
    {
        get => this.fontWeightField;
        set
        {
            VerifyAccess();
            if (this.fontWeightField == value)
                return;
            SetAndRaise(FontWeightProperty, ref this.fontWeightField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set base foreground brush.
    /// </summary>
    private IBrush? foregroundField;
    public IBrush? Foreground
    {
        get => this.foregroundField;
        set
        {
            VerifyAccess();
            if (ReferenceEquals(this.foregroundField, value))
                return;
            this.foregroundPropertyChangedHandlerToken?.Dispose();
            if (value is AvaloniaObject aobj)
            {
                this.foregroundPropertyChangedHandlerToken = new(aobj, nameof(AvaloniaObject.PropertyChanged), OnBrushPropertyChanged);
            }
            SetAndRaise(ForegroundProperty, ref this.foregroundField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set letter spacing.
    /// </summary>
    private double letterSpacingField;
    public double LetterSpacing
    {
        get => this.letterSpacingField;
        set
        {
            VerifyAccess();
            if (!double.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (Math.Abs(this.letterSpacingField - value) <= 0.01)
                return;
            SetAndRaise(LetterSpacingProperty, ref this.letterSpacingField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set line height
    /// </summary>
    private double lineHeightField = double.NaN;
    public double LineHeight
    {
        get => this.lineHeightField;
        set
        {
            VerifyAccess();
            if (double.IsNaN(value) && double.IsNaN(this.lineHeightField))
                return;
            else if (!double.IsFinite(value) || value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            else if (double.IsFinite(this.lineHeightField) && Math.Abs(this.lineHeightField - value) <= 0.01)
                return;
            SetAndRaise(LineHeightProperty, ref this.lineHeightField, value);
            InvalidateTextLayout();
        }
    }


    /// <summary>
    /// Get or set maximum height of text layout.
    /// </summary>
    private double maxHeightField = double.PositiveInfinity;
    public double MaxHeight
    {
        get => this.maxHeightField;
        set
        {
            VerifyAccess();
            if (double.IsInfinity(value))
            {
                if (value.Equals(this.maxHeightField))
                    return;
            }
            else if (double.IsNaN(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            else if (double.IsFinite(this.maxHeightField) && Math.Abs(this.maxHeightField - value) <= 0.01)
                return;
            SetAndRaise(MaxHeightProperty, ref this.maxHeightField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set maximum number of lines.
    /// </summary>
    private int maxLinesField;
    public int MaxLines
    {
        get => this.maxLinesField;
        set
        {
            VerifyAccess();
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            else if (this.maxLinesField == value)
                return;
            SetAndRaise(MaxLinesProperty, ref this.maxLinesField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set maximum number of token should be highlighted. Negative value if there is no limitation.
    /// </summary>
    private int maxTokenCountField = -1;
    public int MaxTokenCount
    {
        get => this.maxTokenCountField;
        set
        {
            VerifyAccess();
            if (this.maxTokenCountField == value)
                return;
            SetAndRaise(MaxTokenCountProperty, ref this.maxTokenCountField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set maximum width of text layout.
    /// </summary>
    private double maxWidthField = double.PositiveInfinity;
    public double MaxWidth
    {
        get => this.maxWidthField;
        set
        {
            VerifyAccess();
            if (double.IsInfinity(value))
            {
                if (value.Equals(this.maxWidthField))
                    return;
            }
            else if (double.IsNaN(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            else if (double.IsFinite(this.maxWidthField) && Math.Abs(this.maxWidthField - value) <= 0.01)
                return;
            SetAndRaise(MaxWidthProperty, ref this.maxWidthField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set preedit text.
    /// </summary>
    private string? preeditTextField;
    public string? PreeditText
    {
        get => this.preeditTextField;
        set
        {
            VerifyAccess();
            if (this.preeditTextField == value)
                return;
            SetAndRaise(PreeditTextProperty, ref this.preeditTextField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set background brush for selected text.
    /// </summary>
    private IBrush? selectionBackgroundField;
    public IBrush? SelectionBackground
    {
        get => this.selectionBackgroundField;
        set
        {
            VerifyAccess();
            if (ReferenceEquals(this.selectionBackgroundField, value))
                return;
            this.selectionBackgroundPropertyChangedHandlerToken?.Dispose();
            if (value is AvaloniaObject aobj)
            {
                this.selectionBackgroundPropertyChangedHandlerToken = new(aobj, nameof(AvaloniaObject.PropertyChanged), OnBrushPropertyChanged);
            }
            SetAndRaise(SelectionBackgroundProperty, ref this.selectionBackgroundField, value);
            if (SelectionStart != SelectionEnd)
                InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set foreground brush for selected text.
    /// </summary>
    private IBrush? selectionForegroundField;
    public IBrush? SelectionForeground
    {
        get => this.selectionForegroundField;
        set
        {
            VerifyAccess();
            if (ReferenceEquals(this.selectionForegroundField, value))
                return;
            this.selectionForegroundPropertyChangedHandlerToken?.Dispose();
            if (value is AvaloniaObject aobj)
            {
                this.selectionForegroundPropertyChangedHandlerToken = new(aobj, nameof(AvaloniaObject.PropertyChanged), OnBrushPropertyChanged);
            }
            SetAndRaise(SelectionForegroundProperty, ref this.selectionForegroundField, value);
            if (SelectionStart != SelectionEnd)
                InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set start (inclusive) index of selected text.
    /// </summary>
    private int selectionStartField;
    public int SelectionStart
    {
        get => this.selectionStartField;
        set
        {
            VerifyAccess();
            if (this.selectionStartField == value)
                return;
            SetAndRaise(SelectionStartProperty, ref this.selectionStartField, value);
            if (SelectionForeground != null)
                InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set end (exclusive) index of selected text.
    /// </summary>
    private int selectionEndField;
    public int SelectionEnd
    {
        get => this.selectionEndField;
        set
        {
            VerifyAccess();
            if (this.selectionEndField == value)
                return;
            SetAndRaise(SelectionEndProperty, ref this.selectionEndField, value);
            if (SelectionForeground != null)
                InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set text.
    /// </summary>
    private string? textField;
    public string? Text
    {
        get => this.textField;
        set
        {
            VerifyAccess();
            if ((this.textField?.Length ?? 0) < 1024
                && (value?.Length ?? 0) < 1024
                && this.textField == value)
            {
                return;
            }
            SetAndRaise(TextProperty, ref this.textField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Get or set text alignment.
    /// </summary>
    private TextAlignment textAlignmentField = TextAlignment.Left;
    public TextAlignment TextAlignment
    {
        get => this.textAlignmentField;
        set
        {
            VerifyAccess();
            if (this.textAlignmentField == value)
                return;
            SetAndRaise(TextAlignmentProperty, ref this.textAlignmentField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set base text decorations.
    /// </summary>
    private TextDecorationCollection? textDecorationsField;
    public TextDecorationCollection? TextDecorations
    {
        get => this.textDecorationsField;
        set
        {
            VerifyAccess();
            if (this.textDecorationsField == value)
                return;
            SetAndRaise(TextDecorationProperty, ref this.textDecorationsField, value);
            InvalidateTextProperties();
        }
    }

    /// <summary>
    /// Raised when text layout of the instance was invalidated.
    /// </summary>
    public event EventHandler? TextLayoutInvalidated;

    // Get text with pre-edit text.
    private string TextWithPreeditText
    {
        get
        {
            string? text = Text;
            if (!string.IsNullOrEmpty(PreeditText))
            {
                if (text == null)
                    text = PreeditText;
                else
                {
                    int caretIndex = Math.Min(SelectionStart, SelectionEnd);
                    if (caretIndex < 0)
                        text = PreeditText + text;
                    else if (caretIndex >= text.Length)
                        text += PreeditText;
                    else
                        text = text[..caretIndex] + PreeditText + text[caretIndex..];
                }
            }
            return text ?? "";
        }
    }

    /// <summary>
    /// Get or set text trimming.
    /// </summary>
    private TextTrimming textTrimmingField = TextTrimming.CharacterEllipsis;
    public TextTrimming TextTrimming
    {
        get => this.textTrimmingField;
        set
        {
            VerifyAccess();
            if (this.textTrimmingField == value)
                return;
            SetAndRaise(TextTrimmingProperty, ref this.textTrimmingField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Get or set text wrapping.
    /// </summary>
    private TextWrapping textWrappingField = TextWrapping.NoWrap;
    public TextWrapping TextWrapping
    {
        get => this.textWrappingField;
        set
        {
            VerifyAccess();
            if (this.textWrappingField == value)
                return;
            SetAndRaise(TextWrappingProperty, ref this.textWrappingField, value);
            InvalidateTextLayout();
        }
    }

    /// <summary>
    /// Create text layout.
    /// </summary>
    /// <returns>Text layout.</returns>
    public TextLayout CreateTextLayout()
    {
        // use created text layout
        if (this.textLayout != null)
            return this.textLayout;

        // get text
        string text = TextWithPreeditText;

        // create type face
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        // prepare base run properties
        var defaultRunProperties = new GenericTextRunProperties(
            typeface,
            FontSize,
            TextDecorations,
            Foreground,
            Background
        );

        // create text runs and source
        if (this.textProperties.Count == 0)
        {
            CreateTextProperties(defaultRunProperties);
        }

        // create text layout
        this.textLayout = new TextLayout(
            text,
            typeface,
            FontSize,
            Foreground,
            TextAlignment,
            TextWrapping,
            TextTrimming,
            maxLines: MaxLines,
            maxWidth: MaxWidth,
            maxHeight: MaxHeight,
            textStyleOverrides: this.textProperties,
            flowDirection: FlowDirection,
            lineHeight: LineHeight,
            letterSpacing: LetterSpacing
        );
#if DEBUG
        var textLines = this.textLayout.TextLines;
        for (int lineIndex = textLines.Count - 1; lineIndex >= 0; --lineIndex)
        {
            var textRuns = textLines[lineIndex].TextRuns;
            for (int runIndex = textRuns.Count - 1; runIndex >= 0; --runIndex)
            {
                var textRun = textRuns[runIndex];
                if (textRun is ShapedTextRun shapedTextRun && textRun.Length > 0 && shapedTextRun.ShapedBuffer.Length == 0)
                    throw new InvalidOperationException($"Text run with empty shaped buffer created at line {lineIndex} run {runIndex}, text: '{new string(textRun.Text.ToArray())}'.");
            }
        }
#endif
        return this.textLayout;
    }

    private void CreateTextProperties(TextRunProperties defaultRunProperties)
    {
        // check text
        string text = Text ?? string.Empty;
        string preeditText = PreeditText ?? string.Empty;

        // insert text style for text
        if (!string.IsNullOrEmpty(text))
        {
            CreateTextPropertiesForText(defaultRunProperties, text);
        }

        // insert text style for preedit text
        if (!string.IsNullOrEmpty(preeditText))
        {
            CreateTextPropertiesForPreeditText(defaultRunProperties, text, preeditText);
        }
    }

    private void CreateTextPropertiesForText(TextRunProperties defaultRunProperties, string text)
    {
        var defs = DefinitionSet?.TokenDefinitions ?? [];
        var parts = RegexUtils.DelimitTextPartsOverRegexes(defs, text);
        foreach (var (definition, start, length, match) in parts)
        {
            if (definition == null) // normal text
            {
                AddTextProperties(start, length, defaultRunProperties);
            }
            else // token to be highlighted
            {
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
                AddTextProperties(start, length, tokenRunProperties);
            }
        }
    }

    private void CreateTextPropertiesForPreeditText(TextRunProperties defaultRunProperties, string text, string preeditText)
    {
        int preeditTextLength = preeditText.Length;
        int caretIndex = Math.Min(SelectionStart, SelectionEnd);
        var runProperties = new GenericTextRunProperties(
            defaultRunProperties.Typeface,
            defaultRunProperties.FontRenderingEmSize,
            Avalonia.Media.TextDecorations.Underline,
            defaultRunProperties.ForegroundBrush
        );
        if (caretIndex <= 0)
        {
            this.textProperties.Add(new(0, preeditTextLength, runProperties));
            for (int i = this.textProperties.Count - 1; i > 0; --i)
            {
                var properties = this.textProperties[i];
                this.textProperties[i] = new(properties.Start + preeditTextLength, properties.Length, properties.Value);
            }
        }
        else if (caretIndex >= text.Length)
        {
            this.textProperties.Add(new(text.Length, preeditTextLength, runProperties));
        }
        else
        {
            int indexOfTextPropertiesToInsert = this.textProperties.Count - 1;
            var textPropertiesToInsert = this.textProperties[indexOfTextPropertiesToInsert];
            if (textPropertiesToInsert.Start > caretIndex)
            {
                for (int i = this.textProperties.Count - 2; i >= 0; --i)
                {
                    var properties = this.textProperties[i];
                    if (properties.Start <= caretIndex)
                    {
                        indexOfTextPropertiesToInsert = i;
                        textPropertiesToInsert = properties;
                        break;
                    }
                }
            }
            if (textPropertiesToInsert.Start == caretIndex)
                this.textProperties.Insert(indexOfTextPropertiesToInsert, new(caretIndex, preeditTextLength, runProperties));
            else
            {
                this.textProperties[indexOfTextPropertiesToInsert++] = new(textPropertiesToInsert.Start, caretIndex - textPropertiesToInsert.Start, textPropertiesToInsert.Value);
                this.textProperties.Insert(indexOfTextPropertiesToInsert++, new(caretIndex, preeditTextLength, runProperties));
                this.textProperties.Insert(indexOfTextPropertiesToInsert, new(caretIndex + preeditTextLength, textPropertiesToInsert.Length - (caretIndex - textPropertiesToInsert.Start), textPropertiesToInsert.Value));
            }
            for (int i = this.textProperties.Count - 1; i > indexOfTextPropertiesToInsert; --i)
            {
                var properties = this.textProperties[i];
                this.textProperties[i] = new(properties.Start + preeditTextLength, properties.Length, properties.Value);
            }
        }
    }

    private void AddTextProperties(int start, int length, TextRunProperties runProperties) =>
        this.textProperties.Add(new(start, length, runProperties));

    // Called when property of attached brush has been changed.
    private void OnBrushPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) =>
        InvalidateTextProperties();

    // Invalidate text layout.
    private void InvalidateTextLayout()
    {
        this.textLayout = null;
        TextLayoutInvalidated?.Invoke(this, EventArgs.Empty);
    }

    // Invalidate text properties.
    private void InvalidateTextProperties()
    {
        this.textProperties.Clear();
        InvalidateTextLayout();
    }

    internal static void InvalidateSyntaxHighlighterTextBoxesTexts() =>
        Dispatcher.UIThread.Post(() => invalidateSyntaxHighlightersTextBoxesTextsCallbacks.ForEach(c => c()));

    /// <inheritdoc/>
    public void Dispose()
    {
        this.backgroundPropertyChangedHandlerToken?.Dispose();
        this.foregroundPropertyChangedHandlerToken?.Dispose();
        this.selectionBackgroundPropertyChangedHandlerToken?.Dispose();
        this.selectionForegroundPropertyChangedHandlerToken?.Dispose();
    }
}