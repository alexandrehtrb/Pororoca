using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.VariableResolution.IPororocaVariableResolver;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaCollection : IPororocaVariableResolver, ICloneable
{
    public const string SchemaVersion = "Pororoca/1";

    public string Schema => SchemaVersion; // Needs to be object variable, not static

    [JsonInclude]
    public Guid Id { get; set; }

    [JsonInclude]
    public string Name { get; private set; }

    public DateTimeOffset CreatedAt { get; init; }

    [JsonInclude]
    public IReadOnlyList<PororocaVariable> Variables { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaEnvironment> Environments { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaCollectionFolder> Folders { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaRequest> Requests { get; private set; }

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

#nullable disable warnings
    public PororocaCollection()
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaCollection(string name) : this(Guid.NewGuid(), name, DateTimeOffset.Now)
    {
    }

    public PororocaCollection(Guid guid, string name, DateTimeOffset createdAt)
    {
        Id = guid;
        Name = name;
        CreatedAt = createdAt;
        Folders = new List<PororocaCollectionFolder>().AsReadOnly();
        Requests = new List<PororocaRequest>().AsReadOnly();
        Variables = new List<PororocaVariable>().AsReadOnly();
        Environments = new List<PororocaEnvironment>().AsReadOnly();
    }

    public void UpdateName(string name) =>
        Name = name;

    public void UpdateVariables(IEnumerable<PororocaVariable> vars) =>
        Variables = vars.ToList().AsReadOnly();

    public void AddFolder(PororocaCollectionFolder folder)
    {
        List<PororocaCollectionFolder> newList = new(Folders);
        newList.Add(folder);
        Folders = newList.AsReadOnly();
    }

    public void RemoveFolder(PororocaCollectionFolder subFolder)
    {
        List<PororocaCollectionFolder> newList = new(Folders);
        newList.Remove(subFolder);
        Folders = newList.AsReadOnly();
    }

    public void UpdateFolders(IEnumerable<PororocaCollectionFolder> folders) =>
        Folders = folders.ToList().AsReadOnly();

    public void AddRequest(PororocaRequest req)
    {
        List<PororocaRequest> newList = new(Requests);
        newList.Add(req);
        Requests = newList.AsReadOnly();
    }

    public void UpdateRequests(IEnumerable<PororocaRequest> reqs) =>
        Requests = reqs.ToList().AsReadOnly();

    public void AddVariable(PororocaVariable variable)
    {
        List<PororocaVariable> newList = new(Variables);
        newList.Add(variable);
        Variables = newList.AsReadOnly();
    }

    public void RemoveVariable(string variableKey)
    {
        List<PororocaVariable> newList = new(Variables);
        newList.RemoveAll(i => i.Key == variableKey);
        Variables = newList.AsReadOnly();
    }

    public void AddEnvironment(PororocaEnvironment env)
    {
        List<PororocaEnvironment> newList = new(Environments);
        newList.Add(env);
        Environments = newList.AsReadOnly();
    }

    public void RemoveEnvironment(Guid envId)
    {
        List<PororocaEnvironment> newList = new(Environments);
        newList.RemoveAll(i => i.Id == envId);
        Environments = newList.AsReadOnly();
    }

    public void UpdateEnvironments(IEnumerable<PororocaEnvironment> envs) =>
        Environments = envs.ToList().AsReadOnly();

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

    public PororocaCollection ClonePreservingIds() => ConditionalClone(true);

    public object Clone() => ConditionalClone(false);

    private PororocaCollection ConditionalClone(bool preserveIds)
    {
        var id = preserveIds ? Id : Guid.NewGuid();

        return new(id, Name, CreatedAt)
        {
            Folders = Folders.Select(f => (PororocaCollectionFolder)f.Clone()).ToList().AsReadOnly(),
            Requests = Requests.Select(f => (PororocaRequest)f.Clone()).ToList().AsReadOnly(),
            Variables = Variables.Select(v => (PororocaVariable)v.Clone()).ToList().AsReadOnly(),
            Environments = Environments.Select(e => preserveIds ? e.ClonePreservingId() : (PororocaEnvironment)e.Clone()).ToList().AsReadOnly()
        };
    }
}