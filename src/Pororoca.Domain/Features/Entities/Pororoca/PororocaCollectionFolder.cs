using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaCollectionFolder : PororocaCollectionItem, ICloneable
{
    [JsonInclude]
    public Guid Id { get; init; }

    [JsonInclude]
    public string Name { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaCollectionFolder> Folders { get; private set; }

    [JsonInclude]
    public IReadOnlyList<PororocaRequest> Requests { get; private set; }

    #nullable disable warnings
    public PororocaCollectionFolder() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
    #nullable restore warnings

    public PororocaCollectionFolder(string name) : this(Guid.NewGuid(), name)
    {
    }
    
    public PororocaCollectionFolder(Guid id, string name)
    {
        Id = id;
        Name = name;
        Folders = new List<PororocaCollectionFolder>().AsReadOnly();
        Requests = new List<PororocaRequest>().AsReadOnly();
    }

    public void UpdateName(string name) =>
        Name = name;

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

    public object Clone() =>
        new PororocaCollectionFolder(Name)
        {
            Folders = Folders.Select(f => (PororocaCollectionFolder)f.Clone()).ToList(),
            Requests = Requests.Select(r => (PororocaRequest)r.Clone()).ToList()
        };
}