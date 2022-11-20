namespace Pororoca.Infrastructure.Features.Requester;

public enum PororocaWebSocketConnectorState
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Disconnecting = 3
}