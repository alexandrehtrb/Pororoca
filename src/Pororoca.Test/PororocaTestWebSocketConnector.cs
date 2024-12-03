using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.WebSockets;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageTranslator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageValidator;

namespace Pororoca.Test;

public sealed class PororocaTestWebSocketConnector : WebSocketClientSideConnector
{
    private readonly IPororocaVariableResolver varResolver;
    private readonly PororocaWebSocketConnection connection;

    public WebSocketConnectorState State => ConnectionState;

    public PororocaTestWebSocketConnector(IPororocaVariableResolver varResolver,
                                          PororocaWebSocketConnection connection,
                                          Action<WebSocketConnectorState, Exception?>? onConnectionChanged = null)
    {
        this.varResolver = varResolver;
        this.connection = connection;
        this.OnConnectionChanged = onConnectionChanged;
    }

    public Task SendMessageAsync(string msgName)
    {
        var msg = this.connection.ClientMessages?.FirstOrDefault(cm => cm.Name == msgName);
        if (msg is null)
        {
            throw new Exception($"Error: Could not send WebSocket client message. Cause: Message with the name '{msgName}' was not found.");
        }
        else
        {
            return SendMessageAsync(msg!);
        }
    }

#pragma warning disable CA1061
    public async Task SendMessageAsync(PororocaWebSocketClientMessage msg)
#pragma warning restore CA1061
    {
        var effectiveVars = this.varResolver.GetEffectiveVariables();
        if (!IsValidClientMessage(effectiveVars, msg, out string? validationErrorCode))
        {
            throw new Exception($"Error: Could not send WebSocket client message. Cause: '{validationErrorCode}'.");
        }
        else if (!TryTranslateClientMessage(effectiveVars, msg, out var resolvedStreamToSend, out string? translationErrorCode))
        {
            throw new Exception($"Error: Could not send WebSocket client message. Cause: '{translationErrorCode}'.");
        }
        else
        {
            await SendMessageAsync(msg.MessageType.ToWebSocketMessageType(), resolvedStreamToSend!, msg.DisableCompressionForThis);
        }
    }
}