using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PororocaEnvironmentExporter
{
    public static string ExportAsPororocaEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(GenerateEnvironmentToExport(env, shouldHideSecrets, false), MainJsonCtx.PororocaEnvironment);

    internal static PororocaEnvironment GenerateEnvironmentToExport(PororocaEnvironment env, bool shouldHideSecrets, bool preserveIsCurrentEnvironment) =>
        new(Id: env.Id,
            Name: env.Name,
            CreatedAt: env.CreatedAt,
            // Always export as non current environment,
            // unless if exporting environment inside of a collection
            IsCurrent: env.IsCurrent && preserveIsCurrentEnvironment,
            Variables: shouldHideSecrets ?
                       env.Variables.Select(HideSecretVariableInNewVariable).ToList() :
                       env.Variables);

    private static PororocaVariable HideSecretVariableInNewVariable(PororocaVariable v) =>
        v.IsSecret ? new PororocaVariable(v.Enabled, v.Key, string.Empty, v.IsSecret) : v;
}