using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;

namespace Pororoca.Infrastructure.Features.WebSockets;

// Source code taken from:
// https://github.com/alexandrehtrb/AlexandreHtrb.WebSocketExtensions
// 
// The MIT License(MIT)
// Copyright(c) 2026 Alexandre H.T.R. Bonfitto
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

public class WebSocketClientSideConnector : WebSocketConnector
{
    protected override WebSocketMessageDirection DirectionFromThis => WebSocketMessageDirection.FromClient;

    public HttpStatusCode ConnectionHttpStatusCode { get; private set; }

    public IReadOnlyDictionary<string, IEnumerable<string>>? ConnectionHttpHeaders { get; private set; }

    public TimeSpan ElapsedConnectionTimeSpan { get; private set; }

    public WebSocketClientSideConnector(bool collectOnlyServerSideMessages, int bufferSize = DefaultBufferSize) : base(collectOnlyServerSideMessages, bufferSize)
    {
    }

    #region CONNECTION

    public async Task ConnectAsync(ClientWebSocket ws, HttpClient httpClient, Uri uri, CancellationToken cancellationToken = default)
    {
        if (ConnectionState == WebSocketConnectionState.Connected || ConnectionState == WebSocketConnectionState.Connecting)
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
        base.SetupAfterConnected(ws);
        ConnectionHttpStatusCode = ws.HttpStatusCode;
        ConnectionHttpHeaders = ws.HttpResponseHeaders;
        ElapsedConnectionTimeSpan = elapsedConnectionTimeSpan;
    }

#endregion
}