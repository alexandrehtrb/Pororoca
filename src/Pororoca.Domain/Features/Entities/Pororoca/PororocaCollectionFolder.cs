using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed record PororocaCollectionFolder
(
    string Name,
    List<PororocaCollectionFolder> Folders,
    List<PororocaRequest> Requests
)
{
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

    public PororocaCollectionFolder(string name) : this(name,[],[]){}

    public PororocaCollectionFolder Copy() => this with
    {
        Folders = Folders.Select(f => f.Copy()).ToList(),
        Requests = Requests.Select(r => r.CopyAbstract()).ToList()
    };
}