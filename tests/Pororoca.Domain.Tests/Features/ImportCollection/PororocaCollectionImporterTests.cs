using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Xunit;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;

namespace Pororoca.Domain.Tests.Features.ImportCollection;

public static class PororocaCollectionImporterTests
{

    [Fact]
    public static void Should_import_valid_pororoca_collection_correctly()
    {
        // GIVEN
        string json = GetTestFileJson("EmptyCollection.pororoca_collection.json");

        // WHEN AND THEN
        Assert.True(TryImportPororocaCollection(json, preserveId: false, out var col));

        // THEN
        Assert.NotNull(col);
        // Generates a new id when importing a collection manually, in case user imports the same collection twice
            // This is to avoid overwriting when saving user collections
            // But if importing a collection from saved data, the id should be preserved
        Assert.NotEqual(Guid.Parse("ec794541-5c81-49a2-b3d1-113df7432df1"), col!.Id);
        Assert.Equal(DateTimeOffset.Parse("2022-03-03T22:04:15.7115044-03:00"), col.CreatedAt);
        Assert.Equal("Nova coleção", col.Name);
        Assert.NotNull(col.Folders);
        Assert.Empty(col.Folders);
        Assert.NotNull(col.Requests);
        Assert.Empty(col.Requests);
        Assert.NotNull(col.Variables);
        Assert.Empty(col.Variables);
        Assert.NotNull(col.Environments);
        Assert.Empty(col.Environments);
    }

    [Fact]
    public static void Should_import_valid_pororoca_collection_distinguishing_http_and_websockets_correctly()
    {
        // GIVEN
        string json = GetTestFileJson("CollectionWithHttpAndWs.pororoca_collection.json");

        // WHEN AND THEN
        Assert.True(TryImportPororocaCollection(json, preserveId: false, out var col));

        // THEN
        Assert.NotNull(col);
        // Generates a new id when importing a collection manually, in case user imports the same collection twice
        // This is to avoid overwriting when saving user collections
        // But if importing a collection from saved data, the id should be preserved
        Assert.NotEqual(Guid.Parse("ecf7eab2-e65b-4913-82c7-555ecedca357"), col!.Id);
        Assert.Equal(DateTimeOffset.Parse("2022-10-13T16:28:05.9560779-03:00"), col.CreatedAt);
        Assert.Equal("CollectionWithHttpAndWs", col.Name);
        Assert.NotNull(col.Folders);
        Assert.Empty(col.Folders);
        Assert.NotNull(col.Variables);
        Assert.Empty(col.Variables);
        Assert.NotNull(col.Environments);
        Assert.Empty(col.Environments);
        Assert.NotNull(col.Requests);
        Assert.Equal(2, col.Requests.Count);
        Assert.Single(col.HttpRequests);
        Assert.Single(col.WebSocketConnections);

        var httpReq = col.HttpRequests[0];
        Assert.Equal(PororocaRequestType.Http, httpReq.RequestType);
        Assert.Equal("HTTP request", httpReq.Name);
        Assert.Equal(1.1m, httpReq.HttpVersion);
        Assert.Equal("GET", httpReq.HttpMethod);
        Assert.Equal("http://www.pudim.com.br", httpReq.Url);

        var wsConn = col.WebSocketConnections[0];
        Assert.Equal(PororocaRequestType.Websocket, wsConn.RequestType);
        Assert.Equal("WebSocket connection", wsConn.Name);
        Assert.Equal(1.1m, wsConn.HttpVersion);
        Assert.Equal("ws://localhost:5000/test/http1websocket", wsConn.Url);
        Assert.NotNull(wsConn.ClientMessages);
        Assert.Equal(2, wsConn.ClientMessages!.Count);

        var wsCliMsg1 = wsConn.ClientMessages[0];
        Assert.Equal("oi", wsCliMsg1.Name);
        Assert.Equal(PororocaWebSocketClientMessageContentMode.Raw, wsCliMsg1.ContentMode);
        Assert.Equal("oi", wsCliMsg1.RawContent);
        Assert.Equal(PororocaWebSocketMessageDirection.FromClient, wsCliMsg1.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Text, wsCliMsg1.MessageType);
        Assert.False(wsCliMsg1.DisableCompressionForThis);

        var wsCliMsg2 = wsConn.ClientMessages[1];
        Assert.Equal("fechando", wsCliMsg2.Name);
        Assert.Equal(PororocaWebSocketClientMessageContentMode.Raw, wsCliMsg2.ContentMode);
        Assert.Equal("xau", wsCliMsg2.RawContent);
        Assert.Equal(PororocaWebSocketMessageDirection.FromClient, wsCliMsg2.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Close, wsCliMsg2.MessageType);
        Assert.True(wsCliMsg2.DisableCompressionForThis);
    }

    private static string GetTestFileJson(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        string jsonFileInfoPath = Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
        return File.ReadAllText(jsonFileInfoPath, Encoding.UTF8);
    }
}