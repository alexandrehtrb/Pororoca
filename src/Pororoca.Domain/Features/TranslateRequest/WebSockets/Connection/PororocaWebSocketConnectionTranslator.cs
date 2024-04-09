using System.Net.WebSockets;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection;

public static class PororocaWebSocketConnectionTranslator
{
    public static bool TryTranslateConnection(IEnumerable<PororocaVariable> effectiveVars,
                                              PororocaRequestAuth? collectionScopedAuth,
                                              List<PororocaKeyValueParam>? collectionScopedReqHeaders,
                                              IPororocaHttpClientProvider httpClientProvider,
                                              PororocaWebSocketConnection wsConn,
                                              bool disableTlsVerification,
                                              out (ClientWebSocket? wsCli, HttpClient? httpCli) wsAndHttpCli,
                                              out string? errorCode) =>
        TryTranslateConnection(IsWebSocketHttpVersionAvailableInOS,
                               effectiveVars,
                               collectionScopedAuth,
                               collectionScopedReqHeaders,
                               httpClientProvider,
                               wsConn,
                               disableTlsVerification,
                               out wsAndHttpCli,
                               out errorCode);

    internal static bool TryTranslateConnection(HttpVersionAvailableVerifier httpVersionVerifier,
                                                IEnumerable<PororocaVariable> effectiveVars,
                                                PororocaRequestAuth? collectionScopedAuth,
                                                List<PororocaKeyValueParam>? collectionScopedReqHeaders,
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
                var resolvedAuth = ResolveRequestAuth(effectiveVars, collectionScopedAuth, wsConn.CustomAuth);
                var resolvedHeaders = ResolveRequestHeaders(effectiveVars, collectionScopedReqHeaders, wsConn.Headers);
                var resolvedSubprotocols = ResolveSubprotocols(effectiveVars, wsConn);
                var httpCli = httpClientProvider.Provide(disableTlsVerification, resolvedAuth);

                var wsCli = new ClientWebSocket();
                SetHttpVersion(wsConn, wsCli);
                SetConnectionRequestHeaders(resolvedAuth, resolvedHeaders, wsCli);
                SetSubprotocols(resolvedSubprotocols, wsCli);
                SetCompressionOptions(wsConn, wsCli);
                SetCollectHttpResponseDetails(wsCli);

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

    #region RESOLUTION / REPLACE VARIABLE TEMPLATES

    private static PororocaKeyValueParam[] ResolveSubprotocols(IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketConnection wsConn) =>
        wsConn.Subprotocols?
              .Where(x => x.Enabled)
              .Select(x => new PororocaKeyValueParam(true, IPororocaVariableResolver.ReplaceTemplates(x.Key, effectiveVars), null))
              .ToArray() ?? Array.Empty<PororocaKeyValueParam>();

    #endregion

    private static void SetHttpVersion(PororocaWebSocketConnection wsConn, ClientWebSocket wsCli)
    {
        wsCli.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        wsCli.Options.HttpVersion = MakeHttpVersion(wsConn.HttpVersion);
    }

    private static void SetConnectionRequestHeaders(PororocaRequestAuth? resolvedAuth, List<PororocaKeyValueParam> resolvedHeaders, ClientWebSocket wsCli)
    {
        var resolvedNonContentHeaders = MakeNonContentHeaders(resolvedAuth, resolvedHeaders);
        foreach (var header in resolvedNonContentHeaders)
        {
            wsCli.Options.SetRequestHeader(header.Key, header.Value);
        }
    }

    private static void SetSubprotocols(PororocaKeyValueParam[] resolvedSubprotocols, ClientWebSocket wsCli)
    {
        var subprotocols = resolvedSubprotocols.Select(kv => kv.Key);
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
    private static void SetCollectHttpResponseDetails(ClientWebSocket wsCli) => wsCli.Options.CollectHttpResponseDetails = true;
}