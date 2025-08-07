using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.TextEditorConfig;

internal sealed class PororocaVariableTextEditorCompletionData : ICompletionData
{
    public PororocaVariableTextEditorCompletionData(PororocaVariable v)
    {
        Text = v.Key;
        // this.descriptionText = v.Value ?? string.Empty
    }

    public IImage Image => null!;

    public string Text { get; }

    // Use this property if you want to show a fancy UIElement in the list.
    private Control? _contentControl;
    public object Content => _contentControl ??= BuildContentControl();

    // reconsider Description text in the future;
    // just draw a TextBlock like Content above
    public object Description => null!;

    public double Priority { get; } = 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }

    private Control BuildContentControl()
    {
        TextBlock textBlock = new TextBlock();
        textBlock.Text = Text;
        textBlock.Margin = new Thickness(5);
        textBlock.Classes.Add("AutoCompleteListOption");

        return textBlock;
    }
}