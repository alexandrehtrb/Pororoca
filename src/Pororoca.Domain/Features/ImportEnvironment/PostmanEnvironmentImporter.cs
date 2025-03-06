using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Postman;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportEnvironment;

public static class PostmanEnvironmentImporter
{
    public static bool TryImportPostmanEnvironment(string postmanEnvironmentFileContent, out PororocaEnvironment? pororocaEnvironment)
    {
        try
        {
            var postmanEnvironment = JsonSerializer.Deserialize(postmanEnvironmentFileContent, MainJsonCtx.PostmanEnvironment);
            if (postmanEnvironment == null
             || postmanEnvironment.Name == null)
            {
                pororocaEnvironment = null;
                return false;
            }

            return TryConvertPostmanEnvironment(postmanEnvironment, out pororocaEnvironment);
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Error, "Failed to import Postman environment.", ex);
            pororocaEnvironment = null;
            return false;
        }
    }

    internal static bool TryConvertPostmanEnvironment(PostmanEnvironment postmanEnvironment, out PororocaEnvironment? pororocaEnvironment)
    {
        try
        {
            var envVars = postmanEnvironment.Values != null ?
                          postmanEnvironment.Values.Select(ConvertPostmanEnvironmentVariable).ToList() :
                          new();

            pororocaEnvironment = new(
                // Always generating new id, in case user imports the same environment twice
                Id: Guid.NewGuid(),
                Name: postmanEnvironment.Name,
                CreatedAt: DateTimeOffset.Now,
                IsCurrent: false,
                Variables: envVars);

            return true;
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Error, "Failed to convert Postman environment to Pororoca.", ex);
            pororocaEnvironment = null;
            return false;
        }
    }

    private static PororocaVariable ConvertPostmanEnvironmentVariable(PostmanEnvironmentVariable envVar) =>
        new(envVar.Enabled, envVar.Key, envVar.Value, envVar.Type == "secret");
}