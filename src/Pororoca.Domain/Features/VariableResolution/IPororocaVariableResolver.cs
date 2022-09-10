using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Domain.Features.VariableResolution;

public interface IPororocaVariableResolver
{
    const string VariableTemplateBeginToken = "{{";
    const string VariableTemplateEndToken = "}}";

    // Example of templated string:
    // "https://{{MyApiHost}}/api/location?city=Campinas"
    // If there is a variable with the key "MyApiHost" and with the value "www.api.com.br" (all without quotes)
    // Then this method should return:
    // "https://www.api.com.br/api/location?city=Campinas"
    // The variable resolution depends on collection and environment variables.
    // Environment variables have precedence over collection variables.
    // If the variable key is not declared or the variable is not enabled, then the raw key should be used as is.

    string ReplaceTemplates(string? strToReplaceTemplatedVariables);

    public IEnumerable<KeyValuePair<string, string>> ResolveKeyValueParams(IEnumerable<PororocaKeyValueParam>? kvParams) =>
        kvParams == null ?
        Array.Empty<KeyValuePair<string, string>>() :
        kvParams.Where(h => h.Enabled)
                .Select(h => new KeyValuePair<string, string>(
                    ReplaceTemplates(h.Key),
                    ReplaceTemplates(h.Value)
                ));
}