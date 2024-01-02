using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportCollection;

public static class PororocaCollectionImporter
{
    public static bool TryImportPororocaCollection(string pororocaCollectionFileContent, bool preserveId, out PororocaCollection? pororocaCollection)
    {
        try
        {
            pororocaCollection = JsonSerializer.Deserialize(pororocaCollectionFileContent, MainJsonCtxWithConverters.PororocaCollection);

            // Generates a new id when importing a collection manually, in case user imports the same collection twice
            // This is to avoid overwriting when saving user collections
            // But if importing a collection from saved data, the id should be preserved
            if (pororocaCollection != null && preserveId == false)
            {
                pororocaCollection = pororocaCollection with { Id = Guid.NewGuid() };
            }

            return pororocaCollection != null
                && pororocaCollection.Requests != null
                && pororocaCollection.Folders != null
                && pororocaCollection.Variables != null
                && pororocaCollection.Environments != null;
        }
        catch
        {
            pororocaCollection = null;
            return false;
        }
    }
}