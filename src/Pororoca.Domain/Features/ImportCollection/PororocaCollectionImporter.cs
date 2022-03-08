using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportCollection;

public static class PororocaCollectionImporter
{
    public static bool TryImportPororocaCollection(string pororocaCollectionFileContent, out PororocaCollection? pororocaCollection)
    {
        try
        {
            pororocaCollection = JsonSerializer.Deserialize<PororocaCollection>(pororocaCollectionFileContent, options: ExporterImporterJsonOptions);
            
            // Always generating new id, in case user imports the same collection twice
            // This is to avoid overwriting when saving user collections
            if (pororocaCollection != null)
            {
                pororocaCollection.Id = Guid.NewGuid();
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