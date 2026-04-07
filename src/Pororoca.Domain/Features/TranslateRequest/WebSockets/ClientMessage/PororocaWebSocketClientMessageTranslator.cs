using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage;

public static class PororocaWebSocketClientMessageTranslator
{
    public static bool TryTranslateClientMessage(IEnumerable<PororocaVariable> effectiveVars,
                                                 PororocaWebSocketClientMessage wsCliMsg,
                                                 out byte[]? resolvedMsgBytes,
                                                 out FileStream? resolvedMsgStream,
                                                 out string? errorCode)
    {
        try
        {
            switch (wsCliMsg.ContentMode)
            {                
                case PororocaWebSocketClientMessageContentMode.Raw:
                    resolvedMsgBytes = GetBytesToSendFromRawContent(effectiveVars, wsCliMsg);
                    resolvedMsgStream = null;
                    break;
                default:
                case PororocaWebSocketClientMessageContentMode.File:
                    resolvedMsgBytes = null;
                    resolvedMsgStream = GetStreamToSendFromFileContent(effectiveVars, wsCliMsg);
                    break;
            }
            errorCode = null;
            return true;
        }
        catch
        {
            resolvedMsgBytes = null;
            resolvedMsgStream = null;
            errorCode = TranslateRequestErrors.WebSocketUnknownClientMessageTranslationError;
            return false;
        }
    }

    private static byte[] GetBytesToSendFromRawContent(IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketClientMessage wsCliMsg)
    {
        string txt = IPororocaVariableResolver.ReplaceTemplates(wsCliMsg.RawContent, effectiveVars);
        return Encoding.UTF8.GetBytes(txt);
    }

    private static FileStream GetStreamToSendFromFileContent(IEnumerable<PororocaVariable> effectiveVars, PororocaWebSocketClientMessage wsCliMsg)
    {
        string resolvedFilePath = IPororocaVariableResolver.ReplaceTemplates(wsCliMsg.FileSrcPath, effectiveVars);
        // DO NOT USE "USING" FOR FILESTREAM HERE --> it will be disposed later, inside the WebSocketConnector
        return File.OpenRead(resolvedFilePath);
    }
}