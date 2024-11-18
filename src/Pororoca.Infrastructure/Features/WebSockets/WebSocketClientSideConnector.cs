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

    public async Task ConnectAsync(ClientWebSocket client, HttpClient httpClient, Uri uri, CancellationToken cancellationToken = default)
    {
        if (ConnectionState == WebSocketConnectorState.Connected || ConnectionState == WebSocketConnectorState.Connecting)
            return;  // Not throwing exception if user tried to connect whilst WebSocket is connected

        Stopwatch sw = new();
        try
        {
            SetIsConnecting();
            sw.Start();
            await client.ConnectAsync(uri!, httpClient, cancellationToken);
            sw.Stop();
            SetupAfterConnected(client, sw.Elapsed);
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

        CancellationTokenSource cts = new(); // no maximum lifetime period
        base.SetupAfterConnected(cts);

        ConnectionHttpStatusCode = ((ClientWebSocket)this.ws).HttpStatusCode;
        ConnectionHttpHeaders = ((ClientWebSocket)this.ws).HttpResponseHeaders;
        ElapsedConnectionTimeSpan = elapsedConnectionTimeSpan;
    }

    #endregion
}