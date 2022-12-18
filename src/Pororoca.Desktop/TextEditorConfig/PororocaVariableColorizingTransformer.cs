using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace Pororoca.Desktop.TextEditorConfig;

internal class PororocaVariableColorizingTransformer : DocumentColorizingTransformer
{
    private static readonly Regex pororocaVarRegex = new("\\{\\{[\\w\\d]+\\}\\}");

    protected override void ColorizeLine(DocumentLine line)
    {
        string lineText = CurrentContext.Document.GetText(line);
        var matches = pororocaVarRegex.Matches(lineText);
        foreach (object objM in matches)
        {
            var match = (Match)objM;
            foreach (object objC in match.Captures)
            {
                var capture = (Capture)objC;
                ChangeLinePart(
                    line.Offset + capture.Index,
                    line.Offset + capture.Index + capture.Length,
                    visualLine =>
                    {
                        visualLine.TextRunProperties.SetForegroundBrush(Brushes.Gold);
                    }
                );
            }
        }
    }
}