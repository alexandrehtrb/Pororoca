using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PororocaEnvironmentExporter
{
    public static string ExportAsPororocaEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(GenerateEnvironmentToExport(env, shouldHideSecrets), options: ExporterImporterJsonOptions);

    internal static PororocaEnvironment GenerateEnvironmentToExport(PororocaEnvironment env, bool shouldHideSecrets)
    {
        PororocaEnvironment shallowClonedEnv = new(env.Id, env.Name, env.CreatedAt);
        shallowClonedEnv.IsCurrent = false; // Always export as non current environment
        
        shallowClonedEnv.UpdateVariables(
            shouldHideSecrets ?
            env.Variables.Select(HideSecretVariableInNewVariable) :
            env.Variables);
                
        return shallowClonedEnv;
    }

    private static PororocaVariable HideSecretVariableInNewVariable(PororocaVariable v) =>
        v.IsSecret ? new PororocaVariable(v.Enabled, v.Key, string.Empty, v.IsSecret) : v;
}