using System.Net.WebSockets;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.WebSockets;

public static class WebSocketMessageTypeExtensionsTests
{

    [Theory]
    [InlineData(WebSocketMessageType.Text, PororocaWebSocketMessageType.Text)]
    [InlineData(WebSocketMessageType.Close, PororocaWebSocketMessageType.Close)]
    [InlineData(WebSocketMessageType.Binary, PororocaWebSocketMessageType.Binary)]
    [InlineData(WebSocketMessageType.Binary, (PororocaWebSocketMessageType)11)]
    public static void Should_correctly_map_pororoca_ws_msg_type_to_dotnet_ws_msg_type(WebSocketMessageType expectedMappedWsMsgType, PororocaWebSocketMessageType pororocaWsMsgType) =>
        Assert.Equal(expectedMappedWsMsgType, pororocaWsMsgType.ToWebSocketMessageType());

}