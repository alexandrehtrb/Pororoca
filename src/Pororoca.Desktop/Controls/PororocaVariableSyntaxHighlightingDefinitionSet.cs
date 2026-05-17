using System.Text.RegularExpressions;
using Avalonia.Media;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.VariableResolution.PororocaPredefinedVariableEvaluator;

namespace Pororoca.Desktop.Controls;

// TODO: Use this as a singleton,
// testing if it works with the same text whilst changing between Collections
internal sealed class PororocaVariableSyntaxHighlightingDefinitionSet : SyntaxHighlightingDefinitionSet
{
    private readonly IPororocaVariableResolver varResolver;

    // IMPORTANTE: este método deve receber um CollectionViewModel,
    // e não simplesmente uma coleção, pois senão não vai atualizar
    // as variáveis de coleção e de ambiente.
    internal PororocaVariableSyntaxHighlightingDefinitionSet(IPororocaVariableResolver varResolver) : base(string.Empty)
    {
        this.varResolver = varResolver;
        TokenDefinitions =
        [
            new(id: 1, name: "Pororoca Variable", IPororocaVariableResolver.PororocaVariableRegex)
            {
                RegexMatchIdMapper = MapPororocaVariableRegexMatchToId,
                RegexMatchIdForegroundMapper = MapPororocaVariableRegexMatchIdToForeground
            }
        ];
    }

    private IBrush MapPororocaVariableRegexMatchIdToForeground(int regexMatchId) => regexMatchId switch
    {
        0 => PororocaThemeManager.NoMatchingVariableForegroundBrush,
        1 => PororocaThemeManager.RegularVariableForegroundBrush,
        2 => PororocaThemeManager.PredefinedVariableForegroundBrush,
        _ => Brushes.Red
    };

    private int MapPororocaVariableRegexMatchToId(Match match)
    {
        string keyName = match.Groups["k"].Value;

        if (IsPredefinedVariable(keyName))
        {
            return 2;
        }
        else
        {
            return this.varResolver.IsEffectiveVariable(keyName) ? 1 : 0;
        }
    }
}