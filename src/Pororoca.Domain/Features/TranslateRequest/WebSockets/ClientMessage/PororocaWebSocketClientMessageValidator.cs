using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageValidator
{

    public static bool IsValidClientMessage(IPororocaVariableResolver varResolver,
                                            PororocaWebSocketClientMessage wsCliMsg,
                                            out string? errorCode) =>
        IsValidClientMessage(varResolver, wsCliMsg, File.Exists, out errorCode);

    internal static bool IsValidClientMessage(IPororocaVariableResolver varResolver,
                                              PororocaWebSocketClientMessage wsCliMsg,
                                              FileExistsVerifier fileExistsVerifier,
                                              out string? errorCode)
    {
        if (wsCliMsg.ContentMode == PororocaWebSocketClientMessageContentMode.File)
        {
            return IsValidClientMessageWithFileContent(varResolver, wsCliMsg, fileExistsVerifier, out errorCode);
        }
        else
        {
            errorCode = null;
            return true;
        }
    }

    private static bool IsValidClientMessageWithFileContent(IPororocaVariableResolver varResolver, PororocaWebSocketClientMessage wsCliMsg, FileExistsVerifier fileExistsVerifier, out string? errorCode)
    {
        string resolvedFilePath = varResolver.ReplaceTemplates(wsCliMsg.FileSrcPath);
        bool valid = fileExistsVerifier(resolvedFilePath);
        errorCode = valid ? null : TranslateRequestErrors.WebSocketClientMessageContentFileNotFound;
        return valid;
    }
}