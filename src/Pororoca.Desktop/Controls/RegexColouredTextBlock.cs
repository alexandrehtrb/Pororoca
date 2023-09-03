using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Pororoca.Desktop.Controls;

public partial class RegexColouredTextBlock : TextBlock
{
    private static readonly Regex pororocaVarRegex = GeneratePororocaVariableRegex();

    [GeneratedRegex("(\\{\\{[\\w\\d]+\\}\\})")]
    private static partial Regex GeneratePororocaVariableRegex();

    public RegexColouredTextBlock() : base()
    {
        this.PropertyChanged += propChanged;
    }

    private void propChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(Text) && e.OldValue != e.NewValue && e.NewValue is not null)
        {
            var parts = SeparateRegexBlocks((string)e.NewValue);
            InlineCollection inlineCollection = new();
            foreach (var part in parts)
            {
                Run r = new(part.Text);
                if (part.MatchesRegex)
                {
                    r.Foreground = Brushes.Crimson;
                }
                inlineCollection.Add(r);
            }
            Inlines = inlineCollection;
        }
    }

    private static List<(bool MatchesRegex, string Text)> SeparateRegexBlocks(string txt)
    {
        string[] parts = pororocaVarRegex.Split(txt);
        return parts.Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => (pororocaVarRegex.IsMatch(p), p))
                    .ToList();
    }
}