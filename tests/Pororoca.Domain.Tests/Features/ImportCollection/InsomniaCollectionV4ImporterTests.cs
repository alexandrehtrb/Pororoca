using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using SharpYaml.Tokens;
using Xunit;
using static Pororoca.Domain.Features.ImportCollection.InsomniaCollectionV4Importer;

namespace Pororoca.Domain.Tests.Features.ImportCollection;

public static class InsomniaCollectionV4ImporterTests
{
    [Fact]
    public static void Should_import_valid_full_insomnia_collection_correctly()
    {
        // GIVEN
        string json = ReadTestFileText("TestInsomniaCollection.json");

        // WHEN AND THEN
        Assert.True(TryImportInsomniaCollection(json, out var col));

        // THEN
        Assert.NotNull(col);
        // Generates a new id when importing a collection manually, in case user imports the same collection twice
        // This is to avoid overwriting when saving user collections
        // But if importing a collection from saved data, the id should be preserved
        Assert.Equal(DateTimeOffset.Parse("2024-07-26T16:09:47.183-03:00"), col.CreatedAt);
        Assert.Equal("Scratch Pad", col.Name);
        Assert.Empty(col.Variables);

        Assert.Null(col.CollectionScopedAuth);

        var env = Assert.Single(col.Environments);
        Assert.Equal(DateTimeOffset.Parse("2024-07-26T16:10:10.986-03:00"), env.CreatedAt);
        Assert.Equal("Base Environment", env.Name);
        Assert.False(env.IsCurrent);
        Assert.Equal([
            new(true, "_.TOKEN", "", true),
            new(true, "_.BASE_PATH", "http://localhost", true),
            new(true, "_.PORT", "12000", true),
            new(true, "_.A_DEPLOYMENT", "", true),
            new(true, "_.A_DEMO_NAME", "morbo", true),
            new(true, "_.A_TENANT", "60f9ce41097976465bc82d97", true),
            new(true, "_.TEMPLATE_TYPE", "generic0", true),
            new(true, "_.TEMPLATE_TITLE", "Generic 0", true),
            new(true, "_.USER_ID", "auth0|60f88ef08e31d50069f49d21", true),
            new(true, "_.USER_EMAIL", "prasanth@auth0.com", true)],
            env.Variables);

        Assert.Empty(col.HttpRequests);
        var dir = Assert.Single(col.Folders);
        Assert.Equal("Credentials (context)", dir.Name);
        Assert.Equal(3, dir.HttpRequests.Count);

        var req = dir.HttpRequests[0];
        Assert.Equal("RAW", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{ _.BASE_PATH }}:{{ _.PORT }}/api/v1/tenants", req.Url);
        Assert.Equal([
            new(true, "Authorization", "Bearer {{ _.TOKEN }}")],
            req.Headers);
        Assert.Equal(
            PororocaHttpRequestBody.MakeRawContent("{\n\t\"name\": \"pv-labs\"}", "application/json"),
            req.Body);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req.CustomAuth);

        req = dir.HttpRequests[1];
        Assert.Equal("Get public creds", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{ _.BASE_PATH }}:{{ _.PORT }}/api/v1/context/public/{{ _.A_DEMO_NAME }}", req.Url);
        Assert.Equal([
            new(true, "Authorization", "Bearer {{ _.TOKEN }}"),
            new(true, "Accept", "application/json")],
            req.Headers);
        Assert.Null(req.Body);
        Assert.Equal(
            PororocaRequestAuth.MakeBasicAuth("usr", "pwd"),
            req.CustomAuth);

        req = dir.HttpRequests[2];
        Assert.Equal("Get private creds", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{ _.BASE_PATH }}:{{ _.PORT }}/api/v1/context/private/{{ _.A_DEMO_NAME }}", req.Url);
        Assert.Equal([
            new(true, "Authorization", "Bearer {{ _.TOKEN }}"),
            new(true, "Accept", "application/json")],
            req.Headers);
        Assert.Null(req.Body);
        Assert.Equal(
            PororocaRequestAuth.MakeBearerAuth("bearertkn"),
            req.CustomAuth);

        dir = Assert.Single(dir.Folders);
        Assert.Equal("My Folder", dir.Name);
        Assert.Equal(2, dir.HttpRequests.Count);

        req = dir.HttpRequests[0];
        Assert.Equal("MULTIPARTFORMDATA", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal(string.Empty, req.Url);
        Assert.Equal([
            new(true, "User-Agent", "insomnia/9.3.2")],
            req.Headers);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.FormData, req.Body.Mode);
        Assert.NotNull(req.Body.FormDataValues);
        Assert.Equal(2, req.Body.FormDataValues.Count);
        Assert.Equal(
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "fafa", "fafasfaaf", "text/plain"),
            req.Body.FormDataValues[0]);
        Assert.Equal(
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "regege", "gergege", "text/plain"),
            req.Body.FormDataValues[1]);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req.CustomAuth);

        req = dir.HttpRequests[1];
        Assert.Equal("URLENCODED", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("PATCH", req.HttpMethod);
        Assert.Equal(string.Empty, req.Url);
        Assert.Equal([
            new(true, "User-Agent", "insomnia/9.3.2")],
            req.Headers);

        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.UrlEncoded, req.Body.Mode);
        Assert.NotNull(req.Body.UrlEncodedValues);
        var urlEncodedParam = Assert.Single(req.Body.UrlEncodedValues);
        Assert.Equal(
            new PororocaKeyValueParam(true, "sdszgzzg", "zfgzdfgzdg"),
            urlEncodedParam);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req.CustomAuth);

        var ws = Assert.Single(dir.WebSocketConnections);
        Assert.Equal("WEBSOCKET", ws.Name);
        Assert.Equal(1.1m, ws.HttpVersion);
        Assert.Equal("wss://localhost:5000", ws.Url);
        Assert.Equal([
            new(true, "User-Agent", "insomnia/9.3.2")],
            req.Headers);
        Assert.Equal(
            PororocaRequestAuth.MakeBasicAuth("usr", "pwd"),
            ws.CustomAuth);
        Assert.Null(ws.CompressionOptions);
        Assert.Null(ws.Subprotocols);

        dir = Assert.Single(dir.Folders);
        Assert.Equal("My Folder2", dir.Name);
        Assert.Equal(2, dir.HttpRequests.Count);

        req = dir.HttpRequests[0];
        Assert.Equal("FILE", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal(string.Empty, req.Url);
        Assert.Equal([
            new(true, "User-Agent", "insomnia/9.3.2")],
            req.Headers);
        Assert.Equal(
            PororocaHttpRequestBody.MakeFileContent(
                "C:\\Users\\Alexandre\\Desktop\\Insomnia_2024-07-26.json",
                "application/json"),
            req.Body);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req.CustomAuth);

        req = dir.HttpRequests[1];
        Assert.Equal("GRAPHQL", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("https://localhost:50001/graphql", req.Url);
        Assert.Equal([
            new(true, "User-Agent", "insomnia/9.3.2")],
            req.Headers);
        Assert.Equal(
            PororocaHttpRequestBody.MakeGraphQlContent("query", "variables"),
            req.Body);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req.CustomAuth);
    }
}