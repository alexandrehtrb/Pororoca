using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
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

    public Task SendMessageAsync(string msgName, float waitingTimeInSeconds = 0.5f) =>
        SendMessageAsync(msgName, TimeSpan.FromSeconds(waitingTimeInSeconds));

    public Task SendMessageAsync(string msgName, TimeSpan waitingTime)
    {
        var msg = this.connection.ClientMessages?.FirstOrDefault(cm => cm.Name == msgName);
        if (msg is null)
        {
            throw new Exception($"Error: Could not send WebSocket client message. Cause: Message with the name '{msgName}' was not found.");
        }
        else
        {
            return SendMessageAsync(msg!, waitingTime);
        }
    }

#pragma warning disable CA1061
    public async Task SendMessageAsync(PororocaWebSocketClientMessage msg, TimeSpan waitingTimeInSeconds)
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
            var waitForSendingTask = Task.Delay(waitingTimeInSeconds);
            var sendingTask = base.SendMessageAsync(resolvedMsgToSend!).AsTask();
            await Task.WhenAll(waitForSendingTask, sendingTask);
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