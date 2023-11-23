using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.ExportEnvironment.PororocaEnvironmentExporter;

namespace Pororoca.Domain.Features.ExportCollection;

public static class PororocaCollectionExporter
{
    public static string ExportAsPororocaCollection(PororocaCollection col, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(GenerateCollectionToExport(col, shouldHideSecrets), MainJsonCtxWithConverters.PororocaCollection);

    internal static PororocaCollection GenerateCollectionToExport(PororocaCollection col, bool shouldHideSecrets)
    {
        var fullClonedCol = col.Copy(preserveIds: true) with
        {
            Variables = shouldHideSecrets ? col.Variables.Select(HideSecretVariableInNewVariable).ToList() : col.Variables,
            Environments = col.Environments.Select(e => GenerateEnvironmentToExport(e, shouldHideSecrets, true)).ToList()
        };

        return fullClonedCol;
    }

    private static PororocaVariable HideSecretVariableInNewVariable(PororocaVariable v) =>
        v.IsSecret ? v with { Value = string.Empty } : v;
}