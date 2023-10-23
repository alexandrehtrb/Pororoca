using System.Net.WebSockets;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest.Common;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection;

public static class PororocaWebSocketConnectionTranslator
{
    public static bool TryTranslateConnection(IPororocaVariableResolver varResolver,
                                              IEnumerable<PororocaVariable> effectiveVars,
                                              IPororocaHttpClientProvider httpClientProvider,
                                              PororocaWebSocketConnection wsConn,
                                              bool disableTlsVerification,
                                              out (ClientWebSocket? wsCli, HttpClient? httpCli) wsAndHttpCli,
                                              out string? errorCode) =>
        TryTranslateConnection(IsWebSocketHttpVersionAvailableInOS,
                               varResolver,
                               effectiveVars,
                               httpClientProvider,
                               wsConn,
                               disableTlsVerification,
                               out wsAndHttpCli,
                               out errorCode);

    internal static bool TryTranslateConnection(HttpVersionAvailableVerifier httpVersionVerifier,
                                                IPororocaVariableResolver varResolver,
                                                IEnumerable<PororocaVariable> effectiveVars,
                                                IPororocaHttpClientProvider httpClientProvider,
                                                PororocaWebSocketConnection wsConn,
                                                bool disableTlsVerification,
                                                out (ClientWebSocket? wsCli, HttpClient? httpCli) wsAndHttpCli,
                                                out string? errorCode)
    {
        if (!httpVersionVerifier(wsConn.HttpVersion, out errorCode))
        {
            wsAndHttpCli = (null, null);
            return false;
        }
        else
        {
            try
            {
                var resolvedAuth = ResolveRequestAuth(varResolver, effectiveVars, wsConn.CustomAuth);
                var httpCli = httpClientProvider.Provide(disableTlsVerification, resolvedAuth);

                var wsCli = new ClientWebSocket();
                SetHttpVersion(wsConn, wsCli);
                SetConnectionRequestHeaders(varResolver, effectiveVars, wsConn, wsCli);
                SetSubprotocols(varResolver, effectiveVars, wsConn, wsCli);
                SetCompressionOptions(wsConn, wsCli);

                wsAndHttpCli = (wsCli, httpCli);
                return true;
            }
            catch
            {
                wsAndHttpCli = (null, null);
                errorCode = TranslateRequestErrors.WebSocketUnknownConnectionTranslationError;
                return false;
            }
        }
    }

    private static void SetHttpVersion(PororocaWebSocketConnection wsConn, ClientWebSocket wsCli)
    {
        wsCli.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        wsCli.Options.HttpVersion = PororocaRequestCommonTranslator.ResolveHttpVersion(wsConn.HttpVersion);
    }

    private static void SetConnectionRequestHeaders(IPororocaVariableResolver varResolver, IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketConnection wsConn, ClientWebSocket wsCli)
    {
        var resolvedNonContentHeaders = ResolveNonContentHeaders(varResolver, effectiveVars, wsConn.Headers, wsConn.CustomAuth);
        foreach (var header in resolvedNonContentHeaders)
        {
            wsCli.Options.SetRequestHeader(header.Key, header.Value);
        }
    }

    private static void SetSubprotocols(IPororocaVariableResolver varResolver, IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketConnection wsConn, ClientWebSocket wsCli)
    {
        var subprotocols = varResolver.ResolveKeyValueParams(wsConn.Subprotocols, effectiveVars).Select(kv => kv.Key);
        foreach (string subprotocol in subprotocols)
        {
            wsCli.Options.AddSubProtocol(subprotocol);
        }
    }

    private static void SetCompressionOptions(PororocaWebSocketConnection wsConn, ClientWebSocket wsCli)
    {
        if (wsConn.EnableCompression)
        {
            wsCli.Options.DangerousDeflateOptions = new()
            {
                ClientContextTakeover = wsConn.CompressionOptions!.ClientContextTakeover,
                ClientMaxWindowBits = wsConn.CompressionOptions!.ClientMaxWindowBits,
                ServerContextTakeover = wsConn.CompressionOptions!.ServerContextTakeover,
                ServerMaxWindowBits = wsConn.CompressionOptions!.ServerMaxWindowBits
            };
        }
    }
}