using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PororocaEnvironmentExporter
{
    public static string ExportAsPororocaEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(GenerateEnvironmentToExport(env, shouldHideSecrets, false), options: ExporterImporterJsonOptions);

    internal static PororocaEnvironment GenerateEnvironmentToExport(PororocaEnvironment env, bool shouldHideSecrets, bool preserveIsCurrentEnvironment)
    {
        PororocaEnvironment shallowClonedEnv = new(env.Id, env.Name, env.CreatedAt);
        // Always export as non current environment,
        // unless if exporting environment inside of a collection
        shallowClonedEnv.IsCurrent = env.IsCurrent && preserveIsCurrentEnvironment;

        shallowClonedEnv.UpdateVariables(
            shouldHideSecrets ?
            env.Variables.Select(HideSecretVariableInNewVariable) :
            env.Variables);

        return shallowClonedEnv;
    }

    private static PororocaVariable HideSecretVariableInNewVariable(PororocaVariable v) =>
        v.IsSecret ? new PororocaVariable(v.Enabled, v.Key, string.Empty, v.IsSecret) : v;
}