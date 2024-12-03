using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;

namespace Pororoca.Infrastructure.Features.WebSockets;

public class WebSocketClientSideConnector : WebSocketConnector
{
    protected override WebSocketMessageDirection DirectionFromThis => WebSocketMessageDirection.FromClient;

    public HttpStatusCode ConnectionHttpStatusCode { get; private set; }

    public IReadOnlyDictionary<string, IEnumerable<string>>? ConnectionHttpHeaders { get; private set; }

    public TimeSpan ElapsedConnectionTimeSpan { get; private set; }

    #region CONNECTION

    public async Task ConnectAsync(ClientWebSocket ws, HttpClient httpClient, Uri uri, CancellationToken cancellationToken = default)
    {
        if (ConnectionState == WebSocketConnectorState.Connected || ConnectionState == WebSocketConnectorState.Connecting)
            return;  // Not throwing exception if user tried to connect whilst WebSocket is connected

        Stopwatch sw = new();
        try
        {
            SetIsConnecting();
            sw.Start();
            await ws.ConnectAsync(uri!, httpClient, cancellationToken);
            sw.Stop();
            SetupAfterConnected(ws, sw.Elapsed);
            SetIsConnected();
        }
        catch (Exception ex)
        {
            SetIsDisconnected(ex);
        }
    }

    private void SetupAfterConnected(ClientWebSocket ws, TimeSpan elapsedConnectionTimeSpan)
    {
        this.ws = ws;
        base.SetupAfterConnected();

        ConnectionHttpStatusCode = ((ClientWebSocket)this.ws).HttpStatusCode;
        ConnectionHttpHeaders = ((ClientWebSocket)this.ws).HttpResponseHeaders;
        ElapsedConnectionTimeSpan = elapsedConnectionTimeSpan;
    }

    #endregion
}