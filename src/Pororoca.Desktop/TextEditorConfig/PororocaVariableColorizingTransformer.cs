using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Desktop.TextEditorConfig;

internal partial class PororocaVariableColorizingTransformer : DocumentColorizingTransformer
{
    public SolidColorBrush RegularVariableForegroundBrush { get; set; }
    public SolidColorBrush PredefinedVariableForegroundBrush { get; set; }

    public PororocaVariableColorizingTransformer(SolidColorBrush initialRegularVarForegroundBrush, SolidColorBrush initialPredefVarForegroundBrush)
    {
        RegularVariableForegroundBrush = initialRegularVarForegroundBrush;
        PredefinedVariableForegroundBrush = initialPredefVarForegroundBrush;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        string lineText = CurrentContext.Document.GetText(line);
        var matches = IPororocaVariableResolver.PororocaVariableRegex.Matches(lineText);
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
                        bool isPredefVar = capture.ValueSpan.Contains('$');
                        // TODO: Get variable highlight color from Styles.xaml
                        visualLine.TextRunProperties.SetForegroundBrush(
                            isPredefVar ?
                            PredefinedVariableForegroundBrush :
                            RegularVariableForegroundBrush);
                    }
                );
            }
        }
    }
}