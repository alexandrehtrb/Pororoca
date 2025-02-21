using System.Diagnostics;
using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportCollection;

public static class PororocaCollectionImporter
{
    public static async Task<PororocaCollection?> ImportPororocaCollectionAsync(Stream utf8JsonStream, bool preserveId)
    {
        try
        {
            var col = await JsonSerializer.DeserializeAsync(utf8JsonStream, MainJsonCtxWithConverters.PororocaCollection);

            // Generates a new id when importing a collection manually, in case user imports the same collection twice
            // This is to avoid overwriting when saving user collections
            // But if importing a collection from saved data, the id should be preserved
            if (col != null && preserveId == false)
            {
                return col with { Id = Guid.NewGuid() };
            }
            else
            {
                return col;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Import collection async error:\n\n" + ex.ToString());
            return null;
        }
    }

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
        catch (Exception ex)
        {
            Debug.WriteLine("Try import collection error:\n\n" + ex.ToString());
            pororocaCollection = null;
            return false;
        }
    }
}