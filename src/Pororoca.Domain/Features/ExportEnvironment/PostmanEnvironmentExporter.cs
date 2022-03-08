using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Postman;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using System.Reflection;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PostmanEnvironmentExporter
{
    public static string ExportAsPostmanEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
        JsonSerializer.Serialize(ConvertToPostmanEnvironment(env, shouldHideSecrets), options: ExporterImporterJsonOptions);

    public static PostmanEnvironment ConvertToPostmanEnvironment(PororocaEnvironment env, bool shouldHideSecrets) =>
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
            ExportedAt = DateTimeOffset.Now,
            ExportedUsing = $"Pororoca/{Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}"
        };
}