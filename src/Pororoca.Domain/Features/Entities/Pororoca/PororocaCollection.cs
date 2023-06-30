using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.VariableResolution.IPororocaVariableResolver;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed record PororocaCollection
(
    [property: JsonInclude] Guid Id,
    [property: JsonInclude] string Name,
    [property: JsonInclude] DateTimeOffset CreatedAt,
    [property: JsonInclude] List<PororocaVariable> Variables,
    [property: JsonInclude] List<PororocaEnvironment> Environments,
    [property: JsonInclude] List<PororocaCollectionFolder> Folders,
    [property: JsonInclude] List<PororocaRequest> Requests
) : IPororocaVariableResolver
{
    public const string SchemaVersion = "Pororoca/1";

    [JsonPropertyOrder(-1)]
    public string Schema => SchemaVersion; // Needs to be object variable, not static

    [JsonIgnore] // JSON IGNORE
    public IReadOnlyList<PororocaHttpRequest> HttpRequests =>
        Requests.Where(r => r is PororocaHttpRequest)
                .Cast<PororocaHttpRequest>()
                .ToList()
                .AsReadOnly();

    [JsonIgnore] // JSON IGNORE
    public IReadOnlyList<PororocaWebSocketConnection> WebSocketConnections =>
        Requests.Where(r => r is PororocaWebSocketConnection)
                .Cast<PororocaWebSocketConnection>()
                .ToList()
                .AsReadOnly();

    public PororocaCollection(Guid guid, string name, DateTimeOffset createdAt) : this(guid, name, createdAt, new(), new(), new(), new()) { }

    public PororocaCollection(string name) : this(Guid.NewGuid(), name, DateTimeOffset.Now) { }

    // Parameterless constructor for JSON deserialization
    public PororocaCollection() : this(string.Empty) { }

    public string ReplaceTemplates(string? strToReplaceTemplatedVariables)
    {
        if (string.IsNullOrWhiteSpace(strToReplaceTemplatedVariables))
        {
            return strToReplaceTemplatedVariables ?? string.Empty;
        }
        else
        {
            IEnumerable<PororocaVariable>? currentEnvironmentVariables = Environments.FirstOrDefault(e => e.IsCurrent)?.Variables;
            IEnumerable<PororocaVariable> effectiveVariables = PororocaVariablesMerger.MergeVariables(Variables, currentEnvironmentVariables);
            string resolvedStr = strToReplaceTemplatedVariables!;
            foreach (var v in effectiveVariables)
            {
                string variableTemplate = VariableTemplateBeginToken + v.Key + VariableTemplateEndToken;
                resolvedStr = resolvedStr.Replace(variableTemplate, v.Value ?? variableTemplate);
            }
            return resolvedStr;
        }
    }

    public PororocaCollection Copy(bool preserveIds) => this with
    {
        Id = preserveIds ? Id : Guid.NewGuid(),
        Folders = Folders.Select(f => (PororocaCollectionFolder)f.Clone()).ToList(),
        Requests = Requests.Select(f => (PororocaRequest)f.Clone()).ToList(),
        Variables = Variables.Select(v => v.Copy()).ToList(),
        Environments = Environments.Select(e => preserveIds ? e.ClonePreservingId() : (PororocaEnvironment)e.Clone()).ToList()
    };
}