using System.Net.WebSockets;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Requester;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection;

public static class PororocaWebSocketConnectionTranslator
{
    public static bool TryTranslateConnection(IPororocaVariableResolver varResolver,
                                              IPororocaClientCertificatesProvider clientCertsProvider,
                                              PororocaWebSocketConnection wsConn,
                                              bool disableTlsVerification,
                                              out ClientWebSocket? wsCli,
                                              out string? errorCode) =>
        TryTranslateConnection(IsWebSocketHttpVersionAvailableInOS,
                               varResolver,
                               clientCertsProvider,
                               wsConn,
                               disableTlsVerification,
                               out wsCli,
                               out errorCode);

    internal static bool TryTranslateConnection(HttpVersionAvailableVerifier httpVersionVerifier,
                                                IPororocaVariableResolver varResolver,
                                                IPororocaClientCertificatesProvider clientCertsProvider,
                                                PororocaWebSocketConnection wsConn,
                                                bool disableTlsVerification,
                                                out ClientWebSocket? wsCli,
                                                out string? errorCode)    
    {
        if (!httpVersionVerifier(wsConn.HttpVersion, out errorCode))
        {
            wsCli = null;
            return false;
        }
        else
        {
            try
            {
                wsCli = new();

                IncludeClientCertificateIfSet(varResolver, clientCertsProvider, wsConn, wsCli);
                SetTlsServerCertificateCheck(wsCli, disableTlsVerification);
                SetConnectionRequestHeaders(varResolver, wsConn, wsCli);
                SetSubprotocols(varResolver, wsConn, wsCli);
                SetCompressionOptions(wsConn, wsCli);

                return true;
            }
            catch
            {
                wsCli = null;
                errorCode = TranslateRequestErrors.WebSocketUnknownConnectionTranslationError;
                return false;
            }
        }
    }    

    private static void IncludeClientCertificateIfSet(IPororocaVariableResolver variableResolver,
                                                      IPororocaClientCertificatesProvider clientCertsProvider,
                                                      PororocaWebSocketConnection wsConn,
                                                      ClientWebSocket wsCli)
    {
        if (wsConn.CustomAuth?.Mode == PororocaRequestAuthMode.ClientCertificate
         && wsConn.CustomAuth?.ClientCertificate != null)
        {
            var resolvedClientCertModel = ResolveClientCertificate(variableResolver, wsConn.CustomAuth!.ClientCertificate!);
            var cert = clientCertsProvider.Provide(resolvedClientCertModel!);

            wsCli.Options.ClientCertificates ??= new();
            wsCli.Options.ClientCertificates.Add(cert);
        }
    }

    private static void SetTlsServerCertificateCheck(ClientWebSocket wsCli, bool disableSslVerification)
    {
        if (disableSslVerification)
        {
            wsCli.Options.RemoteCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }

    private static void SetConnectionRequestHeaders(IPororocaVariableResolver varResolver, PororocaWebSocketConnection wsConn, ClientWebSocket wsCli)
    {
        var resolvedNonContentHeaders = ResolveNonContentHeaders(varResolver, wsConn.Headers, wsConn.CustomAuth);
        foreach (var header in resolvedNonContentHeaders)
        {
            wsCli.Options.SetRequestHeader(header.Key, header.Value);
        }
    }

    private static void SetSubprotocols(IPororocaVariableResolver varResolver, PororocaWebSocketConnection wsConn, ClientWebSocket wsCli)
    {
        var subprotocols = varResolver.ResolveKeyValueParams(wsConn.Subprotocols).Select(kv => kv.Key);
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