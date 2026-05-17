using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.VariableResolution.PororocaPredefinedVariableEvaluator;

namespace Pororoca.Desktop.Converters;

public class PororocaVariableSyntaxHighlightingInlinesConverter : IMultiValueConverter
{
    public static readonly PororocaVariableSyntaxHighlightingInlinesConverter Instance = new();

    private PororocaVariableSyntaxHighlightingInlinesConverter() { }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is string srcTxt
            && values[1] is CollectionViewModel varResolver
            && targetType.IsAssignableTo(typeof(InlineCollection)))
        {
            return GenerateInlinesForText(varResolver, srcTxt);
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static InlineCollection GenerateInlinesForText(IPororocaVariableResolver varResolver, string txt)
    {
        var parts = RegexUtils.DelimitTextPartsOverRegexes(PororocaVarRegexDefiner.SingletonCollection, txt);

        InlineCollection inlineCollection = new();
        foreach (var (_, start, length, match) in parts)
        {
            Run r = new(txt!.Substring(start, length));

            if (match != null)
            {
                string keyName = match.Groups["k"].Value;

                r.Foreground = IsPredefinedVariable(keyName) ?
                    PororocaThemeManager.PredefinedVariableForegroundBrush :
                    varResolver.IsEffectiveVariable(keyName) ?
                    PororocaThemeManager.RegularVariableForegroundBrush :
                    PororocaThemeManager.NoMatchingVariableForegroundBrush;
            }
            else
            {
                // GAMBIARRA!!!
                // O Brush abaixo é necessário porque o Brush padrão do TextBlock do Avalonia
                // é um StaticResource e não se atualiza quando o tema muda, então,
                // precisamos declarar essa côr para usá-la nos inlines.
                // Nos PVSHTextBlocks que não têm inlines,
                // não precisamos usar o Brush abaixo como Foreground, pois nesse caso
                // o Avalonia atualiza a côr corretamente.
                r.Foreground = PororocaThemeManager.PVSHTextBlockDefaultForegroundBrush;
            }

            inlineCollection.Add(r);
        }
        return inlineCollection;
    }
}

internal sealed class PororocaVarRegexDefiner : IRegexDefiner
{
    internal static readonly PororocaVarRegexDefiner[] SingletonCollection = [new()];

    public Regex Pattern => IPororocaVariableResolver.PororocaVariableRegex;

    private PororocaVarRegexDefiner() { }
}