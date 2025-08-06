using System.Text.RegularExpressions;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.VariableResolution.PororocaPredefinedVariableEvaluator;

namespace Pororoca.Domain.Features.VariableResolution;

public partial interface IPororocaVariableResolver
{
    public static readonly Regex PororocaVariableRegex = GeneratePororocaVariableRegex();

    [GeneratedRegex("\\{\\{\\s*(?<k>\\$?[\\w\\d_\\-\\.]+)\\s*\\}\\}")]
    private static partial Regex GeneratePororocaVariableRegex();

    List<PororocaVariable> Variables { get; } // collection variables
    PororocaRequestAuth? CollectionScopedAuth { get; } // collection scoped auth
    List<PororocaEnvironment> Environments { get; } // collection environments

    public IEnumerable<PororocaVariable> GetEffectiveVariables()
    {
        var currentEnv = Environments.FirstOrDefault(e => e.IsCurrent);
        var effectiveEnvVars = currentEnv?.Variables?.Where(sev => sev.Enabled) ?? Enumerable.Empty<PororocaVariable>();
        var effectiveColVars = Variables.Where(cv => cv.Enabled);
        var effectiveColVarsNotInEnv =
            effectiveColVars.Where(ecv => !effectiveEnvVars.Any(eev => eev.Key == ecv.Key));

        // Environment variable overrides collection variable
        return effectiveEnvVars.Concat(effectiveColVarsNotInEnv);
    }

    public static Dictionary<string, string> ResolveKeyValueParams(IEnumerable<PororocaKeyValueParam>? kvParams, IEnumerable<PororocaVariable> effectiveVars) =>
        kvParams == null ?
        new() :
        kvParams.Where(h => h.Enabled)
                .Select(h => new KeyValuePair<string, string>(
                    ReplaceTemplates(h.Key, effectiveVars),
                    ReplaceTemplates(h.Value, effectiveVars)
                ))
                .DistinctBy(h => h.Key) // Avoid duplicated pairs by key
                .ToDictionary(h => h.Key, h => h.Value);

    // Example of templated string:
    // "https://{{MyApiHost}}/api/location?city=Campinas"
    // If there is a variable with the key "MyApiHost" and with the value "www.api.com.br" (all without quotes)
    // Then this method should return:
    // "https://www.api.com.br/api/location?city=Campinas"
    // The variable resolution depends on collection and environment variables.
    // Environment variables have precedence over collection variables.
    // If the variable key is not declared or the variable is not enabled, then the raw key should be used as is.

    public static string ReplaceTemplates(string? strToReplaceTemplatedVariables, IEnumerable<PororocaVariable> effectiveVars)
    {
        if (string.IsNullOrEmpty(strToReplaceTemplatedVariables))
        {
            return strToReplaceTemplatedVariables ?? string.Empty;
        }
        else if (effectiveVars.Any() == false && !strToReplaceTemplatedVariables.Contains('$'))
        {
            // no need to run regex replacer if there are no effective variables and no predefined variables
            return strToReplaceTemplatedVariables;
        }
        else
        {
            return PororocaVariableRegex.Replace(strToReplaceTemplatedVariables, match =>
            {
                string keyName = match.Groups["k"].Value;
                if (IsPredefinedVariable(keyName, out string? predefinedVarValue))
                {
                    return predefinedVarValue!;
                }
                else
                {
                    var effectiveVar = effectiveVars.FirstOrDefault(v => v.Key == keyName);
                    return effectiveVar?.Value ?? match.Value;
                }
            });
        }
    }

    // Este método só é usado no Pororoca.Desktop,
    // porém está na camada de Domain porque é lógica pesada
    // e queremos testes unitários nele.
    public static string? GetPointerHoverVariable(string? lineText, int pointerIndex)
    {
        int startIndex = -1, endIndex = -1;

        if (string.IsNullOrWhiteSpace(lineText) || lineText.Length < 5)
        {
            return null; // no mínimo "{{x}}"
        }
        if (pointerIndex == lineText.Length)
        {
            return null; // ponteiro sobre o final da linha ('\n')
        }     

        if (pointerIndex <= (lineText.Length - 2) && lineText[pointerIndex] == '{' && lineText[pointerIndex + 1] == '{')
        {
            startIndex = pointerIndex;
        }
        else
        {
            for (int i = pointerIndex; i >= 1; i--)
            {
                // o i <= (pointerIndex - 2) é para aceitar caractér de fechamento
                // no caso de o mouse estar em cima da dupla de fechamento ("}}")
                if ((i <= (pointerIndex - 2)) && lineText[i] == '}')
                {
                    // encontrou caractér de fechamento ('}')
                    // antes de encontrar dupla de abertura ("{{")
                    break;
                }
                if (lineText[i] == '{' && lineText[i - 1] == '{')
                {
                    startIndex = i - 1;
                    break;
                }
            }
        }

        if (startIndex == -1)
        {
            return null;
        }

        if (pointerIndex >= 1 && lineText[pointerIndex] == '}' && lineText[pointerIndex - 1] == '}')
        {
            endIndex = pointerIndex;
        }
        else
        {
            for (int i = pointerIndex; i <= lineText.Length - 2; i++)
            {
                // o i >= (pointerIndex + 2) é para aceitar caractér de abertura
                // no caso de o mouse estar em cima da dupla de abertura ("{{")
                if ((i >= (pointerIndex + 2)) && lineText[i] == '{')
                {
                    // encontrou caractér de abertura ('{')
                    // antes de encontrar dupla de fechamento ("}}")
                    break;
                }
                if (lineText[i] == '}' && lineText[i + 1] == '}')
                {
                    endIndex = i + 1;
                    break;
                }
            }
        }

        if (endIndex == -1)
        {
            return null;
        }

        string hoveringVar = lineText[startIndex..(endIndex + 1)];

        var regexMatch = PororocaVariableRegex.Match(hoveringVar);
        return regexMatch.Success ? hoveringVar : null;
    }
}