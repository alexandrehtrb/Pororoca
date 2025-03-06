using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportEnvironment;

public static class PororocaEnvironmentImporter
{
    public static bool TryImportPororocaEnvironment(string pororocaEnvironmentFileContent, out PororocaEnvironment? pororocaEnvironment)
    {
        try
        {
            pororocaEnvironment = JsonSerializer.Deserialize(pororocaEnvironmentFileContent, MainJsonCtx.PororocaEnvironment);

            // Always generating new id, in case user imports the same environment twice
            // And always set as not current environment
            if (pororocaEnvironment != null)
            {
                pororocaEnvironment = pororocaEnvironment with
                {
                    Id = Guid.NewGuid(),
                    IsCurrent = false
                };
            }

            return pororocaEnvironment != null
                && pororocaEnvironment.Variables != null;
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Error, "Failed to import Pororoca environment.", ex);
            pororocaEnvironment = null;
            return false;
        }
    }
}