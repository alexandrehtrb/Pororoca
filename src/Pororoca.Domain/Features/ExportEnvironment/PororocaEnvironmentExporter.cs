using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PororocaEnvironmentExporter
{
    public static string ExportAsPororocaEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(shouldHideSecrets ? GenerateEnvironmentWithHiddenSecrets(env) : env, options: ExporterImporterJsonOptions);

    internal static PororocaEnvironment GenerateEnvironmentWithHiddenSecrets(PororocaEnvironment env)
    {
        static PororocaVariable HideSecretVariableInNewVariable(PororocaVariable v) =>
            v.IsSecret ? new PororocaVariable(v.Enabled, v.Key, string.Empty, v.IsSecret) : v;

        PororocaEnvironment shallowClonedEnv = new(env.Id, env.Name, env.CreatedAt);
        shallowClonedEnv.IsCurrent = false; // Always export as non current environment
        shallowClonedEnv.UpdateVariables(
            env.Variables.Select(HideSecretVariableInNewVariable)
        );
        return shallowClonedEnv;
    }
}