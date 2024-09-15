using System.Diagnostics;
using System.Text.Json.Serialization;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;

namespace Pororoca.Domain.Features.Entities.Pororoca;

#if DEBUG
[DebuggerDisplay("{Name,nq}")]
#endif
public sealed record PororocaCollectionFolder
(
    string Name,
    List<PororocaCollectionFolder> Folders,
    List<PororocaRequest> Requests
)
{
#if DEBUG
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    [JsonIgnore] // JSON IGNORE
    public IReadOnlyList<PororocaHttpRequest> HttpRequests =>
        Requests.OfType<PororocaHttpRequest>()
                .ToList()
                .AsReadOnly();

#if DEBUG
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    [JsonIgnore] // JSON IGNORE
    public IReadOnlyList<PororocaWebSocketConnection> WebSocketConnections =>
        Requests.OfType<PororocaWebSocketConnection>()
                .ToList()
                .AsReadOnly();

#nullable disable warnings
    public PororocaCollectionFolder() : this(string.Empty)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaCollectionFolder(string name) : this(name, [], []) { }

    public PororocaCollectionFolder Copy() => this with
    {
        Folders = Folders.Select(f => f.Copy()).ToList(),
        Requests = Requests.Select(r => r.CopyAbstract()).ToList()
    };
}