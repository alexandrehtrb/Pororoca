using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportEnvironment;

public static class PororocaEnvironmentImporter
{
    public static bool TryImportPororocaEnvironment(string pororocaEnvironmentFileContent, out PororocaEnvironment? pororocaEnvironment)
    {
        try
        {
            pororocaEnvironment = JsonSerializer.Deserialize<PororocaEnvironment>(pororocaEnvironmentFileContent, options: ExporterImporterJsonOptions);
            
            // Always generating new id, in case user imports the same environment twice
            // And always set as not current environment
            if (pororocaEnvironment != null)
            {
                pororocaEnvironment.Id = Guid.NewGuid();
                pororocaEnvironment.IsCurrent = false;
            }
            
            return pororocaEnvironment != null
                && pororocaEnvironment.Variables != null;
        }
        catch
        {
            pororocaEnvironment = null;
            return false;
        }
    }
}