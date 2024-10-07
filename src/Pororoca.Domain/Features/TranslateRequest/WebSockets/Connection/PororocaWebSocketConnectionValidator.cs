using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection;

public static class PororocaWebSocketConnectionValidator
{
    public static bool IsValidConnection(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaWebSocketConnection wsConn, out Uri? resolvedUri, out string? errorCode) =>
        IsValidConnection(IsWebSocketHttpVersionAvailableInOS, File.Exists, effectiveVars, collectionScopedAuth, wsConn, out resolvedUri, out errorCode);

    internal static bool IsValidConnection(HttpVersionAvailableVerifier httpVersionOSVerifier, FileExistsVerifier fileExistsVerifier, IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaWebSocketConnection wsConn, out Uri? resolvedUri, out string? errorCode) =>
        TryResolveAndMakeRequestUri(effectiveVars, wsConn.Url, out resolvedUri, out errorCode)
        && httpVersionOSVerifier(wsConn.HttpVersion, out errorCode)
        && ValidateAuthParams(effectiveVars, fileExistsVerifier, ChooseRequestAuth(collectionScopedAuth, wsConn.CustomAuth), out errorCode)
        && CheckWebSocketCompressionOptions(wsConn.CompressionOptions, out errorCode);

    private static bool CheckWebSocketCompressionOptions(PororocaWebSocketCompressionOptions? compressionOptions, out string? errorCode)
    {
        if (compressionOptions is null)
        {
            errorCode = null;
            return true;
        }
        else if (compressionOptions.ClientMaxWindowBits < 9 || compressionOptions.ClientMaxWindowBits > 15)
        {
            errorCode = TranslateRequestErrors.WebSocketCompressionMaxWindowBitsOutOfRange;
            return false;
        }
        else if (compressionOptions.ServerMaxWindowBits < 9 || compressionOptions.ServerMaxWindowBits > 15)
        {
            errorCode = TranslateRequestErrors.WebSocketCompressionMaxWindowBitsOutOfRange;
            return false;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }
}