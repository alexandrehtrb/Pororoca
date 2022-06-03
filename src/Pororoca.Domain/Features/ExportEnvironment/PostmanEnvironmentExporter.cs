using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Postman;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PostmanEnvironmentExporter
{
    public static string ExportAsPostmanEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(ConvertToPostmanEnvironment(env, shouldHideSecrets), options: ExporterImporterJsonOptions);

    internal static PostmanEnvironment ConvertToPostmanEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
        new()
        {
            Id = env.Id,
            Name = env.Name,
            Values = env.Variables
                        .Select(v => new PostmanEnvironmentVariable()
                        {
                            Key = v.Key,
                            Value = shouldHideSecrets && v.IsSecret ? string.Empty : v.Value,
                            Enabled = v.Enabled
                        })
                        .ToArray(),
            Scope = "environment",
            ExportedAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            ExportedUsing = "Postman/9.15.2" // Exporting with Postman label to avoid blocking by them
        };
}