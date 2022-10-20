using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaCollectionFolder : PororocaCollectionItem, ICloneable
{
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
    public PororocaCollectionFolder() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaCollectionFolder(string name) : this(Guid.NewGuid(), name)
    {
    }

    public PororocaCollectionFolder(Guid id, string name) : base(id, name)
    {
        Folders = new List<PororocaCollectionFolder>().AsReadOnly();
        Requests = new List<PororocaRequest>().AsReadOnly();
    }

    public void AddFolder(PororocaCollectionFolder folder)
    {
        List<PororocaCollectionFolder> newList = new(Folders);
        newList.Add(folder);
        Folders = newList.AsReadOnly();
    }

    public void RemoveFolder(Guid subFolderId)
    {
        List<PororocaCollectionFolder> newList = new(Folders);
        newList.RemoveAll(i => i.Id == subFolderId);
        Folders = newList.AsReadOnly();
    }

    public void AddRequest(PororocaRequest req)
    {
        List<PororocaRequest> newList = new(Requests);
        newList.Add(req);
        Requests = newList.AsReadOnly();
    }

    public void RemoveRequest(Guid reqId)
    {
        List<PororocaRequest> newList = new(Requests);
        newList.RemoveAll(i => i.Id == reqId);
        Requests = newList.AsReadOnly();
    }

    public override object Clone() =>
        new PororocaCollectionFolder(Name)
        {
            Folders = Folders.Select(f => (PororocaCollectionFolder)f.Clone()).ToList(),
            Requests = Requests.Select(r => (PororocaRequest)r.Clone()).ToList()
        };
}