using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageTranslator
{
    public static bool TryTranslateClientMessage(IPororocaVariableResolver varResolver,
                                                 PororocaWebSocketClientMessage wsCliMsg,
                                                 out PororocaWebSocketClientMessageToSend? resolvedMsgToSend,
                                                 out string? errorCode)
    {
        try
        {
            resolvedMsgToSend = wsCliMsg.ContentMode switch
            {
                PororocaWebSocketClientMessageContentMode.File => GetMessageToSendFromFileContent(varResolver, wsCliMsg),
                _ => GetMessageToSendFromRawContent(varResolver, wsCliMsg)
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

    private static PororocaWebSocketClientMessageToSend GetMessageToSendFromRawContent(IPororocaVariableResolver varResolver, PororocaWebSocketClientMessage wsCliMsg)
    {
        string txt = varResolver.ReplaceTemplates(wsCliMsg.RawContent);
        byte[] bytes = Encoding.UTF8.GetBytes(txt);
        MemoryStream ms = new();
        ms.Write(bytes);
        return new(wsCliMsg, ms, txt);
    }

    private static PororocaWebSocketClientMessageToSend GetMessageToSendFromFileContent(IPororocaVariableResolver varResolver, PororocaWebSocketClientMessage wsCliMsg)
    {
        const int fileStreamBufferSize = 4096;
        string resolvedFilePath = varResolver.ReplaceTemplates(wsCliMsg.FileSrcPath);
        FileStream fs = new(resolvedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, fileStreamBufferSize, useAsync: true);
        return new(wsCliMsg, fs, null);
    }
}