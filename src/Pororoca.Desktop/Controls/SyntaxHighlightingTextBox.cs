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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.Controls;

/// <summary>
/// <see cref="Avalonia.Controls.TextBox"/> which supports syntax highlighting.
/// </summary>
public class SyntaxHighlightingTextBox : TextBox
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(TextBox);

    /// <summary>
    /// Property of <see cref="DefinitionSet"/>.
    /// </summary>
    public static readonly DirectProperty<SyntaxHighlightingTextBox, SyntaxHighlightingDefinitionSet?> DefinitionSetProperty = AvaloniaProperty.RegisterDirect<SyntaxHighlightingTextBox, SyntaxHighlightingDefinitionSet?>(nameof(DefinitionSet), t => t.DefinitionSet, (t, ds) => t.DefinitionSet = ds);

    public SyntaxHighlightingDefinitionSet? DefinitionSet
    {
        get;
        set
        {
            VerifyAccess();
            if (field == value)
                return;
            SetAndRaise(DefinitionSetProperty, ref field, value);
            this.textPresenter?.DefinitionSet = field;
        }
    }

    // Fields.
    private SyntaxHighlightingTextPresenter? textPresenter;

    /// <summary>
    /// Initialize new <see cref="SyntaxHighlightingTextBox"/> instance.
    /// </summary>
    public SyntaxHighlightingTextBox()
    {
        PseudoClasses.Add(":syntaxHighlighted");
        PseudoClasses.Add(":syntaxHighlightingTextBox");
        PastingFromClipboard += async (_, e) =>
        {
            if (MainWindow.Instance!.Clipboard is IClipboard systemClipboard)
            {
                OnPastingFromClipboard(await ClipboardExtensions.TryGetTextAsync(systemClipboard));
            }
            e.Handled = true;
        };
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.textPresenter = e.NameScope.Find<SyntaxHighlightingTextPresenter>("PART_TextPresenter");
        this.textPresenter?.DefinitionSet = DefinitionSet;
    }

    /// <summary>
    /// Called when pasting text from clipboard
    /// </summary>
    /// <param name="text">The text from clipboard.</param>
    protected virtual void OnPastingFromClipboard(string? text)
    {
        if (text is null)
            return;

        SelectedText = AcceptsReturn ? text : RemoveLineBreaks(text);
    }

    private static string RemoveLineBreaks(string s) =>
        s.Replace("\\r\\n", string.Empty)
         .Replace("\\n", string.Empty);
}