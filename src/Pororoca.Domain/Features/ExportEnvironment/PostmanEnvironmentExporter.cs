using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Postman;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportEnvironment;

public static class PostmanEnvironmentExporter
{
    public static string ExportAsPostmanEnvironment(PororocaEnvironment env) =>
        JsonSerializer.Serialize(ConvertToPostmanEnvironment(env), MainJsonCtx.PostmanEnvironment);

    internal static PostmanEnvironment ConvertToPostmanEnvironment(PororocaEnvironment env) =>
        new()
        {
            Id = env.Id,
            Name = env.Name,
            Values = env.Variables
                        .Select(v => new PostmanEnvironmentVariable()
                        {
                            Key = v.Key,
                            Value = v.Value,
                            Type = v.IsSecret ? "secret" : null,
                            Enabled = v.Enabled
                        })
                        .ToArray(),
            Scope = "environment",
            ExportedAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
            ExportedUsing = "Postman/10.5.2" // Exporting with Postman label to avoid blocking by them
        };
}