using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportCollection;

public static class PororocaCollectionExporter
{
    public static string ExportAsPororocaCollection(PororocaCollection col, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(shouldHideSecrets ? GenerateCollectionWithHiddenSecrets(col) : col, options: ExporterImporterJsonOptions);

    internal static PororocaCollection GenerateCollectionWithHiddenSecrets(PororocaCollection col)
    {
        static PororocaVariable HideSecretVariableInNewVariable(PororocaVariable v) =>
            v.IsSecret ? new PororocaVariable(v.Enabled, v.Key, string.Empty, v.IsSecret) : v;

        PororocaCollection shallowClonedCol = (PororocaCollection) col.Clone();
        shallowClonedCol.Id = col.Id;
        shallowClonedCol.UpdateVariables(
            col.Variables.Select(HideSecretVariableInNewVariable));
        shallowClonedCol.UpdateEnvironments(
            col.Environments.Select(e =>
            {
                PororocaEnvironment shallowClonedEnv = (PororocaEnvironment) e.Clone();
                shallowClonedEnv.Id = e.Id;
                shallowClonedEnv.UpdateVariables(
                    shallowClonedEnv.Variables.Select(HideSecretVariableInNewVariable)
                );
                return shallowClonedEnv;
            })
        );
        return shallowClonedCol;
    }
}