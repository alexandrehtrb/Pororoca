using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed record PororocaCollection
(
    [property: JsonInclude] Guid Id,
    [property: JsonInclude] string Name,
    [property: JsonInclude] DateTimeOffset CreatedAt,
    [property: JsonInclude] List<PororocaVariable> Variables,
    [property: JsonInclude] PororocaRequestAuth? CollectionScopedAuth,
    [property: JsonInclude] List<PororocaKeyValueParam>? CollectionScopedRequestHeaders,
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
        Requests.OfType<PororocaHttpRequest>()
                .ToList()
                .AsReadOnly();

    [JsonIgnore] // JSON IGNORE
    public IReadOnlyList<PororocaWebSocketConnection> WebSocketConnections =>
        Requests.OfType<PororocaWebSocketConnection>()
                .ToList()
                .AsReadOnly();

    public PororocaCollection(Guid guid, string name, DateTimeOffset createdAt) : this(guid, name, createdAt, new(), null, null, new(), new(), new()) { }

    public PororocaCollection(string name) : this(Guid.NewGuid(), name, DateTimeOffset.Now) { }

    // Parameterless constructor for JSON deserialization
    public PororocaCollection() : this(string.Empty) { }

    public PororocaCollection Copy(bool preserveIds) => this with
    {
        Id = preserveIds ? Id : Guid.NewGuid(),
        Folders = Folders.Select(f => f.Copy()).ToList(),
        Requests = Requests.Select(f => f.CopyAbstract()).ToList(),
        Variables = Variables.Select(v => v.Copy()).ToList(),
        Environments = Environments.Select(e => e.Copy(preserveIds)).ToList(),
        CollectionScopedAuth = CollectionScopedAuth?.Copy(),
        CollectionScopedRequestHeaders = CollectionScopedRequestHeaders?.Select(v => v.Copy())?.ToList(),
    };

    public T? FindRequestInCollection<T>(Func<T, bool> criteria) where T : PororocaRequest
    {
        static T? FindRequestInFolder(PororocaCollectionFolder folder, Func<T, bool> criteria)
        {
            var reqInFolder = folder.Requests.FirstOrDefault(x => x is T t && criteria(t));
            if (reqInFolder != null)
            {
                return (T)reqInFolder;
            }
            else
            {
                foreach (var subFolder in folder.Folders)
                {
                    var reqInSubfolder = FindRequestInFolder(subFolder, criteria);
                    if (reqInSubfolder != null)
                    {
                        return reqInSubfolder;
                    }
                }
                return null;
            }
        }

        var reqInCol = Requests.FirstOrDefault(x => x is T t && criteria(t));
        if (reqInCol != null)
        {
            return (T)reqInCol;
        }
        else
        {
            foreach (var folder in Folders)
            {
                var reqInFolder = FindRequestInFolder(folder, criteria);
                if (reqInFolder != null)
                {
                    return reqInFolder;
                }
            }
            return null;
        }
    }

    public PororocaHttpRequest? GetHttpRequestByPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string[] parts = path.Split('/');
        if (parts.Length == 1)
        {
            return HttpRequests.FirstOrDefault(x => x.Name == parts[0]);
        }
        else
        {
            PororocaCollectionFolder? folder = null;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (i == parts.Length - 1)
                {
                    return folder!.HttpRequests.FirstOrDefault(x => x.Name.Replace("/", string.Empty) == part);
                }
                else if (i == 0)
                {
                    folder = Folders.FirstOrDefault(f => f.Name.Replace("/", string.Empty) == part);
                    if (folder is null) return null;
                }
                else
                {
                    folder = folder?.Folders?.FirstOrDefault(f => f.Name.Replace("/", string.Empty) == part);
                    if (folder is null) return null;
                }
            }
        }
        return null;
    }

    public List<string> ListHttpRequestsPathsInCollection()
    {
        static IEnumerable<string> ListHttpRequestsPathsInFolder(string basePath, PororocaCollectionFolder folder)
        {
            // '/' needs to removed from the name to avoid problem when splitting path by '/'
            string folderBasePath = basePath + folder.Name.Replace("/", string.Empty) + "/";
            var paths = folder.HttpRequests.Select(r => folderBasePath + r.Name.Replace("/", string.Empty)).ToList();
            foreach (var subFolder in folder.Folders)
            {
                paths.AddRange(ListHttpRequestsPathsInFolder(folderBasePath, subFolder));
            }
            return paths;
        }

        var paths = HttpRequests.Select(r => r.Name.Replace("/", string.Empty)).ToList();
        foreach (var folder in Folders)
        {
            paths.AddRange(ListHttpRequestsPathsInFolder(string.Empty, folder));
        }
        return paths;
    }
}