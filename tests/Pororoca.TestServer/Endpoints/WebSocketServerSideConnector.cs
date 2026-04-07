using System.Net.WebSockets;
using Pororoca.Infrastructure.Features.WebSockets;

namespace Pororoca.TestServer.Endpoints;

public sealed class WebSocketServerSideConnector : WebSocketConnector
{
    protected override WebSocketMessageDirection DirectionFromThis => WebSocketMessageDirection.FromServer;

    public WebSocketServerSideConnector(WebSocket ws, bool collectOnlyClientSideMessages, int bufferSize = DefaultBufferSize) : base(collectOnlyClientSideMessages, bufferSize)
    {
        // when this connector gets created, the connection is already established
        SetIsConnected();
        base.SetupAfterConnected(ws);
    }
}