using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageTranslator
{
    public static bool TryTranslateClientMessage(IPororocaVariableResolver varResolver,
                                                 IEnumerable<PororocaVariable> effectiveVars,
                                                 PororocaWebSocketClientMessage wsCliMsg,
                                                 out PororocaWebSocketClientMessageToSend? resolvedMsgToSend,
                                                 out string? errorCode)
    {
        try
        {
            resolvedMsgToSend = wsCliMsg.ContentMode switch
            {
                PororocaWebSocketClientMessageContentMode.File => GetMessageToSendFromFileContent(varResolver, effectiveVars, wsCliMsg),
                _ => GetMessageToSendFromRawContent(varResolver, effectiveVars, wsCliMsg)
            };
            errorCode = null;
            return true;
        }
        catch
        {
            resolvedMsgToSend = null;
            errorCode = TranslateRequestErrors.WebSocketUnknownClientMessageTranslationError;
            return false;
        }
    }

    private static PororocaWebSocketClientMessageToSend GetMessageToSendFromRawContent(IPororocaVariableResolver varResolver, IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketClientMessage wsCliMsg)
    {
        string txt = varResolver.ReplaceTemplates(wsCliMsg.RawContent, effectiveVars);
        byte[] bytes = Encoding.UTF8.GetBytes(txt);
        MemoryStream ms = new();
        ms.Write(bytes);
        return new(wsCliMsg, ms, txt);
    }

    private static PororocaWebSocketClientMessageToSend GetMessageToSendFromFileContent(IPororocaVariableResolver varResolver, IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketClientMessage wsCliMsg)
    {
        const int fileStreamBufferSize = 4096;
        string resolvedFilePath = varResolver.ReplaceTemplates(wsCliMsg.FileSrcPath, effectiveVars);
        // DO NOT USE "USING" FOR FILESTREAM HERE --> it will be disposed later, by the PororocaWebSocketConnector
        FileStream fs = new(resolvedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, fileStreamBufferSize, useAsync: true);
        return new(wsCliMsg, fs, null);
    }
}