using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageTranslator
{
    public static bool TryTranslateClientMessage(IEnumerable<PororocaVariable> effectiveVars,
                                                 PororocaWebSocketClientMessage wsCliMsg,
                                                 out Stream? resolvedMsgStream,
                                                 out string? errorCode)
    {
        try
        {
            resolvedMsgStream = wsCliMsg.ContentMode switch
            {
                PororocaWebSocketClientMessageContentMode.File => GetStreamToSendFromFileContent(effectiveVars, wsCliMsg),
                _ => GetStreamToSendFromRawContent(effectiveVars, wsCliMsg)
            };
            errorCode = null;
            return true;
        }
        catch
        {
            resolvedMsgStream = null;
            errorCode = TranslateRequestErrors.WebSocketUnknownClientMessageTranslationError;
            return false;
        }
    }

    private static Stream GetStreamToSendFromRawContent(IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketClientMessage wsCliMsg)
    {
        string txt = IPororocaVariableResolver.ReplaceTemplates(wsCliMsg.RawContent, effectiveVars);
        return new MemoryStream(Encoding.UTF8.GetBytes(txt));
    }

    private static Stream GetStreamToSendFromFileContent(IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketClientMessage wsCliMsg)
    {
        string resolvedFilePath = IPororocaVariableResolver.ReplaceTemplates(wsCliMsg.FileSrcPath, effectiveVars);
        // DO NOT USE "USING" FOR FILESTREAM HERE --> it will be disposed later, by the PororocaWebSocketConnector
        return File.OpenRead(resolvedFilePath);
    }
}