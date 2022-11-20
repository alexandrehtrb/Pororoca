using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Infrastructure.Features.Requester;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageTranslator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.ClientMessage.PororocaWebSocketClientMessageValidator;

namespace Pororoca.Test;

public sealed class PororocaTestWebSocketConnector : PororocaWebSocketConnector
{
    private readonly IPororocaVariableResolver varResolver;
    private readonly PororocaWebSocketConnection connection;

    public PororocaTestWebSocketConnector(IPororocaVariableResolver varResolver,
                                          PororocaWebSocketConnection connection,
                                          OnWebSocketConnectionChanged? onConnectionChanged = null,
                                          OnWebSocketMessageSending? onMessageBeingSent = null)
        : base(onConnectionChanged, onMessageBeingSent)
    {
        this.varResolver = varResolver;
        this.connection = connection;
    }

    public ValueTask SendMessageAsync(string msgName)
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

    public ValueTask SendMessageAsync(Guid msgId)
    {
        var msg = this.connection.ClientMessages?.FirstOrDefault(cm => cm.Id == msgId);
        if (msg is null)
        {
            throw new Exception($"Error: Could not send WebSocket client message. Cause: Message with the ID '{msgId}' was not found.");
        }
        else
        {
            return SendMessageAsync(msg!);
        }
    }

#pragma warning disable CA1061
    public ValueTask SendMessageAsync(PororocaWebSocketClientMessage msg)
#pragma warning restore CA1061
    {
        if (!IsValidClientMessage(this.varResolver, msg, out string? validationErrorCode))
        {
            throw new Exception($"Error: Could not send WebSocket client message. Cause: '{validationErrorCode}'.");
        }
        else if (!TryTranslateClientMessage(this.varResolver, msg, out var resolvedMsgToSend, out string? translationErrorCode))
        {
            throw new Exception($"Error: Could not send WebSocket client message. Cause: '{translationErrorCode}'.");
        }
        else
        {
            return base.SendMessageAsync(resolvedMsgToSend!);
        }
    }

    public override async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await base.DisconnectAsync(cancellationToken);
        if (ConnectionException is not null)
        {
            throw ConnectionException;
        }
    }
}