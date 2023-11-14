using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageValidator
{

    public static bool IsValidClientMessage(IEnumerable<PororocaVariable> effectiveVars,
                                            PororocaWebSocketClientMessage wsCliMsg,
                                            out string? errorCode) =>
        IsValidClientMessage(effectiveVars, wsCliMsg, File.Exists, out errorCode);

    internal static bool IsValidClientMessage(IEnumerable<PororocaVariable> effectiveVars,
                                              PororocaWebSocketClientMessage wsCliMsg,
                                              FileExistsVerifier fileExistsVerifier,
                                              out string? errorCode)
    {
        if (wsCliMsg.ContentMode == PororocaWebSocketClientMessageContentMode.File)
        {
            return IsValidClientMessageWithFileContent(effectiveVars, wsCliMsg, fileExistsVerifier, out errorCode);
        }
        else
        {
            errorCode = null;
            return true;
        }
    }

    private static bool IsValidClientMessageWithFileContent(IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketClientMessage wsCliMsg, FileExistsVerifier fileExistsVerifier, out string? errorCode)
    {
        string resolvedFilePath = IPororocaVariableResolver.ReplaceTemplates(wsCliMsg.FileSrcPath, effectiveVars);
        bool valid = fileExistsVerifier(resolvedFilePath);
        errorCode = valid ? null : TranslateRequestErrors.WebSocketClientMessageContentFileNotFound;
        return valid;
    }
}