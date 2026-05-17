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

using System.ComponentModel;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Pororoca.Desktop.Others;
using Pororoca.Domain.Features.Common;

namespace Pororoca.Desktop.Controls;

/// <summary>
/// Base class of definition of syntax highlighting.
/// </summary>
public sealed class SyntaxHighlightingDefinition : INotifyPropertyChanged, IDisposable, IRegexDefiner
{
    // Fields.
    private WeakEventHandlerAdapter<AvaloniaObject, AvaloniaPropertyChangedEventArgs>? backgroundPropertyChangedHandlerToken;
    private WeakEventHandlerAdapter<AvaloniaObject, AvaloniaPropertyChangedEventArgs>? foregroundPropertyChangedHandlerToken;

    /// <summary>
    /// Initialize new <see cref="SyntaxHighlightingDefinition"/> instance.
    /// </summary>
    /// <param name="name">Name.</param>
    public SyntaxHighlightingDefinition(int id, string name, Regex pattern)
    {
        Id = id;
        Name = name;
        this.patternField = pattern;
    }

    /// <summary>
    /// Raised when property of rule has been changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Get Id of definition.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Get name of definition.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Get or set pattern of the token.
    /// </summary>
    private Regex patternField;
    public Regex Pattern
    {
        get => this.patternField;
        set
        {
            if (ArePatternsEqual(this.patternField, value))
                return;
            this.patternField = value;
            Validate();
            OnPropertyChanged(nameof(Pattern));
        }
    }

    /// <summary>
    /// Check whether the definition is valid or not.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Get or set background brush of the definition.
    /// </summary>
    private IBrush? backgroundField;
    public IBrush? Background
    {
        get => this.backgroundField;
        set
        {
            if (ReferenceEquals(this.backgroundField, value))
                return;
            this.backgroundPropertyChangedHandlerToken?.Dispose();
            if (value is AvaloniaObject aobj)
            {
                this.backgroundPropertyChangedHandlerToken = new(aobj, nameof(AvaloniaObject.PropertyChanged), OnBrushPropertyChanged);
            }
            this.backgroundField = value;
            Validate();
            OnPropertyChanged(nameof(Background));
        }
    }

    /// <summary>
    /// Get or set font family of the definition.
    /// </summary>
    private FontFamily? fontFamilyField;
    public FontFamily? FontFamily
    {
        get => this.fontFamilyField;
        set
        {
            if (this.fontFamilyField?.Equals(value) ?? value is null)
                return;
            this.fontFamilyField = value;
            Validate();
            OnPropertyChanged(nameof(FontFamily));
        }
    }

    /// <summary>
    /// Get or set font size of the definition.
    /// </summary>
    private double fontSizeField = double.NaN;
    public double FontSize
    {
        get => this.fontSizeField;
        set
        {
            if (double.IsInfinity(value) || value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (AreFontSizesEqual(this.fontSizeField, value))
                return;
            this.fontSizeField = value;
            Validate();
            OnPropertyChanged(nameof(FontSize));
        }
    }

    /// <summary>
    /// Get or set font style of the definition.
    /// </summary>
    private FontStyle? fontStyleField;
    public FontStyle? FontStyle
    {
        get => this.fontStyleField;
        set
        {
            if (this.fontStyleField == value)
                return;
            this.fontStyleField = value;
            Validate();
            OnPropertyChanged(nameof(FontStyle));
        }
    }

    /// <summary>
    /// Get or set font weight of the definition.
    /// </summary>
    private FontWeight? fontWeightField;
    public FontWeight? FontWeight
    {
        get => this.fontWeightField;
        set
        {
            if (this.fontWeightField == value)
                return;
            this.fontWeightField = value;
            Validate();
            OnPropertyChanged(nameof(FontWeight));
        }
    }

    /// <summary>
    /// Get or set foreground brush of the definition.
    /// </summary>
    private IBrush? foregroundField;
    public IBrush? Foreground
    {
        get => this.foregroundField;
        set
        {
            if (ReferenceEquals(this.foregroundField, value))
                return;
            this.foregroundPropertyChangedHandlerToken?.Dispose();
            if (value is AvaloniaObject aobj)
            {
                this.foregroundPropertyChangedHandlerToken = new(aobj, nameof(AvaloniaObject.PropertyChanged), OnBrushPropertyChanged);
            }
            this.foregroundField = value;
            Validate();
            OnPropertyChanged(nameof(Foreground));
        }
    }

    private Func<Match, int>? regexMatchIdMapperField;
    public Func<Match, int>? RegexMatchIdMapper
    {
        get => this.regexMatchIdMapperField;
        set
        {
            if (this.regexMatchIdMapperField == value)
                return;
            this.regexMatchIdMapperField = value;
            Validate();
            OnPropertyChanged(nameof(RegexMatchIdMapper));
        }
    }

    private Func<int, IBrush>? regexMatchIdForegroundMapperField;
    public Func<int, IBrush>? RegexMatchIdForegroundMapper
    {
        get => this.regexMatchIdForegroundMapperField;
        set
        {
            if (this.regexMatchIdForegroundMapperField == value)
                return;
            this.regexMatchIdForegroundMapperField = value;
            Validate();
            OnPropertyChanged(nameof(RegexMatchIdForegroundMapper));
        }
    }

    /// <summary>
    /// Get or set text decorations of the definition.
    /// </summary>
    private TextDecorationCollection? textDecorationsField;
    public TextDecorationCollection? TextDecorations
    {
        get => this.textDecorationsField;
        set
        {
            if (this.textDecorationsField == value)
                return;
            this.textDecorationsField = value;
            Validate();
            OnPropertyChanged(nameof(TextDecorations));
        }
    }

    // Check whether two font sizes are equalivent or not.
    private static bool AreFontSizesEqual(double x, double y)
    {
        if (double.IsNaN(x))
            return double.IsNaN(y);
        if (double.IsNaN(y))
            return false;
        return Math.Abs(x - y) <= 0.01;
    }

    // Check equality of two patterns.
    private static bool ArePatternsEqual(Regex? x, Regex? y)
    {
        if (x is null)
            return y is null;
        if (y is null)
            return false;
        return x.ToString() == y.ToString() && x.Options == y.Options;
    }

    // Called when property of attached brush has been changed.
    private void OnBrushPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (ReferenceEquals(sender, Background))
            OnPropertyChanged(nameof(Background));
        else if (ReferenceEquals(sender, Foreground))
            OnPropertyChanged(nameof(Foreground));
    }

    /// <summary>
    /// Raise <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of changed property.</param>
    public void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new(propertyName));

    /// <summary>
    /// Called to validate whether the definition is valid or not.
    /// </summary>
    /// <returns>True if the definition is valid.</returns>
    public bool OnValidate() =>
        (Background is not null
        || FontFamily is not null
        || double.IsFinite(FontSize)
        || FontStyle.HasValue
        || FontWeight.HasValue
        || Foreground is not null
        || (RegexMatchIdMapper is not null && RegexMatchIdForegroundMapper is not null)
        || TextDecorations is not null)
        && Pattern is not null;

    /// <summary>
    /// Validate whether the definition is valid or not.
    /// </summary>
    public void Validate()
    {
        if (IsValid != OnValidate())
        {
            IsValid = !IsValid;
            OnPropertyChanged(nameof(IsValid));
        }
    }

    internal GenericTextRunProperties MakeTextProperties(TextRunProperties defaultRunProperties, FontStretch fontStretch, int regexMatchId)
    {
        var typeface = new Typeface(
            FontFamily ?? defaultRunProperties.Typeface.FontFamily,
            FontStyle ?? defaultRunProperties.Typeface.Style,
            FontWeight ?? defaultRunProperties.Typeface.Weight,
            fontStretch
        );

        var foreground = regexMatchId != -1 && RegexMatchIdForegroundMapper != null ?
                         RegexMatchIdForegroundMapper(regexMatchId) :
                         Foreground ?? defaultRunProperties.ForegroundBrush;

        return new GenericTextRunProperties(
            typeface,
            double.IsNaN(FontSize) ? defaultRunProperties.FontRenderingEmSize : FontSize,
            TextDecorations ?? defaultRunProperties.TextDecorations,
            foreground,
            Background ?? defaultRunProperties.BackgroundBrush
        );
    }

    /// <inheritdoc/>
    public override string ToString() =>
        string.IsNullOrEmpty(Name) ?
            $"{{{Pattern}}}" :
            $"[{Name}]{{{Pattern}}}";

    /// <inheritdoc/>
    public void Dispose()
    {
        this.backgroundPropertyChangedHandlerToken?.Dispose();
        this.foregroundPropertyChangedHandlerToken?.Dispose();
    }
}