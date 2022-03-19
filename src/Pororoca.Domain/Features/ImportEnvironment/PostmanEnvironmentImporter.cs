using System.Text.Json;
using Pororoca.Domain.Features.Entities.Postman;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportEnvironment;

public static class PostmanEnvironmentImporter
{
    public static bool TryImportPostmanEnvironment(string postmanEnvironmentFileContent, out PororocaEnvironment? pororocaEnvironment)
    {
        try
        {
            PostmanEnvironment? postmanEnvironment = JsonSerializer.Deserialize<PostmanEnvironment>(postmanEnvironmentFileContent, options: ExporterImporterJsonOptions);
            if (postmanEnvironment == null
             || postmanEnvironment.Name == null)
            {
                pororocaEnvironment = null;
                return false;
            }

            return TryConvertPostmanEnvironment(postmanEnvironment, out pororocaEnvironment);
        }
        catch
        {
            pororocaEnvironment = null;
            return false;
        }
    }

    internal static bool TryConvertPostmanEnvironment(PostmanEnvironment postmanEnvironment, out PororocaEnvironment? pororocaEnvironment)
    {
        try
        {
            // Always generating new id, in case user imports the same environment twice
            PororocaEnvironment myEnv = new(Guid.NewGuid(), postmanEnvironment.Name, DateTimeOffset.Now);
            if (postmanEnvironment.Values != null)
            {
                foreach (PostmanEnvironmentVariable envVar in postmanEnvironment.Values)
                {
                    myEnv.AddVariable(ConvertPostmanEnvironmentVariable(envVar));
                }
            }

            pororocaEnvironment = myEnv;
            return true;
        }
        catch
        {
            pororocaEnvironment = null;
            return false;
        }
    }

    private static PororocaVariable ConvertPostmanEnvironmentVariable(PostmanEnvironmentVariable envVar) =>
        new(envVar.Enabled, envVar.Key, envVar.Value, false);
}