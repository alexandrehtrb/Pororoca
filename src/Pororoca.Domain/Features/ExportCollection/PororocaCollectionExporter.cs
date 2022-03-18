using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.ExportEnvironment.PororocaEnvironmentExporter;

namespace Pororoca.Domain.Features.ExportCollection;

public static class PororocaCollectionExporter
{
    public static string ExportAsPororocaCollection(PororocaCollection col, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(GenerateCollectionToExport(col, shouldHideSecrets), options: ExporterImporterJsonOptions);

    internal static PororocaCollection GenerateCollectionToExport(PororocaCollection col, bool shouldHideSecrets)
    {
        PororocaCollection shallowClonedCol = (PororocaCollection) col.Clone();
        shallowClonedCol.Id = col.Id;

        shallowClonedCol.UpdateVariables(
            shouldHideSecrets ?
            col.Variables.Select(HideSecretVariableInNewVariable) :
            col.Variables);

        shallowClonedCol.UpdateEnvironments(
            col.Environments.Select(e => GenerateEnvironmentToExport(e, shouldHideSecrets)));

        return shallowClonedCol;
    }

    private static PororocaVariable HideSecretVariableInNewVariable(PororocaVariable v) =>
        v.IsSecret ? new PororocaVariable(v.Enabled, v.Key, string.Empty, v.IsSecret) : v;
}