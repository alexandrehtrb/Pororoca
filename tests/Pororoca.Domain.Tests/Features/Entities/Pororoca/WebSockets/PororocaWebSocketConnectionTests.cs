using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Xunit;
using static Pororoca.Domain.Features.Entities.Pororoca.PororocaRequestAuth;
using System.Net;
using System;
using System.Security.Authentication;
using System.Xml.Linq;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.WebSockets;

public static class PororocaWebSocketConnectionTests
{
    [Fact]
    public static void Should_copy_full_ws_creating_new_instance()
    {
        // GIVEN
        PororocaWebSocketConnection ws = new(
            Name: "name",
            HttpVersion: 2.0m,
            Url: "myurl",
            Headers: [new(true, "k1", "v1"), new(true, "k2", "v2")],
            CustomAuth: MakeBasicAuth("usr", "pwd"),
            CompressionOptions: new(13,true,11,false),
            Subprotocols: [new(true, "subptc1", null), new(true, "subptc3", null)],
            ClientMessages: [
                new(PororocaWebSocketMessageType.Text,
                    "climsg1",
                    PororocaWebSocketClientMessageContentMode.Raw,
                    "{\"id\":1}",
                    PororocaWebSocketMessageRawContentSyntax.Json,
                    null,
                    true),
                new(PororocaWebSocketMessageType.Close,
                    "climsg7",
                    PororocaWebSocketClientMessageContentMode.File,
                    null,
                    null,
                    "C:\\ARQUIVOS\\arq1.txt",
                    false),
            ]
        );

        // WHEN
        var copy = ws.Copy();

        // THEN
        Assert.NotSame(ws, copy);
        Assert.Equal(ws.Name, copy.Name);
        Assert.Equal(ws.HttpVersion, copy.HttpVersion);
        Assert.Equal(ws.Url, copy.Url);
        Assert.Equal(ws.Headers, copy.Headers);
        Assert.Equal(ws.CustomAuth, copy.CustomAuth);
        Assert.Equal(ws.CompressionOptions, copy.CompressionOptions);
        Assert.Equal(ws.Subprotocols, copy.Subprotocols);
        Assert.Equal(ws.ClientMessages, copy.ClientMessages);
    }

    [Fact]
    public static void Should_copy_empty_ws_creating_new_instance()
    {
        // GIVEN
        PororocaWebSocketConnection ws = new(
            Name: "name",
            HttpVersion: 2.0m,
            Url: "myurl",
            Headers: null,
            CustomAuth: null,
            CompressionOptions: null,
            Subprotocols: null,
            ClientMessages: null
        );

        // WHEN
        var copy = ws.Copy();

        // THEN
        Assert.NotSame(ws, copy);
        Assert.Equal(ws.Name, copy.Name);
        Assert.Equal(ws.HttpVersion, copy.HttpVersion);
        Assert.Equal(ws.Url, copy.Url);
        Assert.Equal(ws.Headers, copy.Headers);
        Assert.Equal(ws.CustomAuth, copy.CustomAuth);
        Assert.Equal(ws.CompressionOptions, copy.CompressionOptions);
        Assert.Equal(ws.Subprotocols, copy.Subprotocols);
        Assert.Equal(ws.ClientMessages, copy.ClientMessages);
    }
}