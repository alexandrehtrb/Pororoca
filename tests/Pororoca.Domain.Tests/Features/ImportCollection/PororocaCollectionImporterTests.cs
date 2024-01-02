using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Xunit;
using static Pororoca.Domain.Features.ImportCollection.PororocaCollectionImporter;

namespace Pororoca.Domain.Tests.Features.ImportCollection;

public static class PororocaCollectionImporterTests
{
    [Fact]
    public static void Should_import_valid_empty_pororoca_collection_correctly()
    {
        // GIVEN
        string json = ReadTestFileText("EmptyCollection.pororoca_collection.json");

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
    public static void Should_import_valid_full_pororoca_collection_correctly()
    {
        // this test also validates the distinguishment between HTTP and WebSocket requestTypes

        // GIVEN
        string json = ReadTestFileText("FullCollection.pororoca_collection.json");
        const string testFilesDirPath = "C:\\PROJETOS\\Pororoca\\src\\Pororoca.Desktop\\PororocaUserData\\TestFiles";

        // WHEN AND THEN
        Assert.True(TryImportPororocaCollection(json, preserveId: true, out var col));

        // THEN
        Assert.NotNull(col);
        // Generates a new id when importing a collection manually, in case user imports the same collection twice
        // This is to avoid overwriting when saving user collections
        // But if importing a collection from saved data, the id should be preserved
        Assert.Equal(Guid.Parse("36211950-c703-4629-a138-fbd241e8eed0"), col!.Id);
        Assert.Equal(DateTimeOffset.Parse("2023-12-01T11:08:39.9830082-03:00"), col.CreatedAt);
        Assert.Equal("COL1", col.Name);
        Assert.Equal([
            new(true, "ClientCertificatesDir", testFilesDirPath + "\\ClientCertificates", false),
            new(true, "TestFilesDir", testFilesDirPath, false),
            new(true, "SpecialHeaderKey", "Header2", false),
            new(true, "SpecialHeaderValue", "ciao", false),
            new(true, "SpecialValue1", "Tailândia", false)],
            col.Variables);

        Assert.NotNull(col.CollectionScopedAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, col.CollectionScopedAuth.Mode);
        Assert.Equal("token", col.CollectionScopedAuth.BearerToken);

        var env1 = Assert.Single(col.Environments);
        Assert.Equal(Guid.Parse("bdd5a627-ccf4-4ef6-9aa3-3030ba1a81d0"), env1.Id);
        Assert.Equal(DateTimeOffset.Parse("2023-12-01T11:08:40.8639007-03:00"), env1.CreatedAt);
        Assert.Equal("ENV1", env1.Name);
        Assert.True(env1.IsCurrent);
        Assert.Equal([
            new(true, "BaseUrl", "https://localhost:5001", false),
            new(true, "BaseUrlWs", "wss://localhost:5001", false),
            new(true, "BadSslSelfSignedTestsUrl", "https://self-signed.badssl.com/", false),
            new(true, "BadSslClientCertTestsUrl", "https://client.badssl.com", false),
            new(true, "BadSslClientCertFilePassword", "badssl.com", false),
            new(true, "BasicAuthLogin", "usr", false),
            new(true, "BasicAuthPassword", "pwd", false),
            new(true, "BearerAuthToken", "token_local", false)],
            env1.Variables);

        var req = Assert.Single(col.HttpRequests);
        Assert.Equal("HTTPHEADERS", req.Name);
        Assert.Equal(1.0m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/get/headers", req.Url);
        Assert.Equal([
            new(false, "Header1", "ValueHeader1"),
            new(true, "Header1", "Header1Value"),
            new(true, "oi_{{SpecialHeaderKey}}", "oi-{{SpecialHeaderValue}}")],
            req.Headers);
        Assert.Null(req.Body);
        Assert.Null(req.CustomAuth);

        var dir1 = Assert.Single(col.Folders);
        Assert.Equal("DIR1", dir1.Name);
        Assert.Equal(6, dir1.Requests.Count);

        req = dir1.HttpRequests[0];
        Assert.Equal("HTTPNONEBASICAUTH", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/auth", req.Url);
        Assert.Null(req.Headers);
        Assert.Null(req.Body);
        Assert.NotNull(req.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.Basic, req.CustomAuth.Mode);
        Assert.Equal("{{BasicAuthLogin}}", req.CustomAuth.BasicAuthLogin);
        Assert.Equal("{{BasicAuthPassword}}", req.CustomAuth.BasicAuthPassword);

        req = dir1.HttpRequests[1];
        Assert.Equal("HTTPNONEBEARERAUTH", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/auth", req.Url);
        Assert.Null(req.Headers);
        Assert.Null(req.Body);
        Assert.NotNull(req.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, req.CustomAuth.Mode);
        Assert.Equal("{{BearerAuthToken}}", req.CustomAuth.BearerToken);

        req = dir1.HttpRequests[2];
        Assert.Equal("HTTPNONEWINDOWSAUTH", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/auth", req.Url);
        Assert.Null(req.Headers);
        Assert.Null(req.Body);
        Assert.NotNull(req.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.Windows, req.CustomAuth.Mode);
        Assert.NotNull(req.CustomAuth.Windows);
        Assert.False(req.CustomAuth.Windows.UseCurrentUser);
        Assert.Equal("{{WindowsAuthLogin}}", req.CustomAuth.Windows.Login);
        Assert.Equal("{{WindowsAuthPassword}}", req.CustomAuth.Windows.Password);
        Assert.Equal("{{WindowsAuthDomain}}", req.CustomAuth.Windows.Domain);

        req = dir1.HttpRequests[3];
        Assert.Equal("HTTPNONEPKCS12AUTH", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BadSslClientCertTestsUrl}}", req.Url);
        Assert.Null(req.Headers);
        Assert.Null(req.Body);
        Assert.NotNull(req.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.ClientCertificate, req.CustomAuth.Mode);
        Assert.NotNull(req.CustomAuth.ClientCertificate);
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pkcs12, req.CustomAuth.ClientCertificate.Type);
        Assert.Equal("{{ClientCertificatesDir}}/badssl.com-client.p12", req.CustomAuth.ClientCertificate.CertificateFilePath);
        Assert.Equal("{{BadSslClientCertFilePassword}}", req.CustomAuth.ClientCertificate.FilePassword);

        req = dir1.HttpRequests[4];
        Assert.Equal("HTTPFORMDATA", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/post/multipartformdata", req.Url);
        Assert.Null(req.Headers);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.FormData, req.Body.Mode);
        Assert.Equal([
            new(true, PororocaHttpRequestFormDataParamType.Text, "a", "xyz{{SpecialValue1}}", "text/plain", null),
            new(true, PororocaHttpRequestFormDataParamType.Text, "b", "[]", "application/json", null),
            new(true, PororocaHttpRequestFormDataParamType.File, "arq", null, "text/plain", "{{TestFilesDir}}/arq.txt")],
            req.Body.FormDataValues);
        Assert.Null(req.CustomAuth);

        req = dir1.HttpRequests[5];
        Assert.Equal("HTTPGRAPHQL", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/post/graphql", req.Url);
        Assert.Null(req.Headers);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.GraphQl, req.Body.Mode);
        Assert.NotNull(req.Body.GraphQlValues);
        Assert.Equal("query", req.Body.GraphQlValues.Query);
        Assert.Equal("variables", req.Body.GraphQlValues.Variables);
        Assert.Null(req.CustomAuth);

        var dir2 = Assert.Single(dir1.Folders);
        Assert.Equal("DIR2", dir2.Name);
        Assert.Equal(2, dir2.HttpRequests.Count);

        req = dir2.HttpRequests[0];
        Assert.Equal("HTTPRAW", req.Name);
        Assert.Equal(2.0m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/post/json", req.Url);
        Assert.Null(req.Headers);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, req.Body.Mode);
        Assert.Equal("application/json", req.Body.ContentType);
        Assert.Equal("{\"myValue\":\"{{SpecialValue1}}\"}", req.Body.RawContent);
        Assert.Null(req.CustomAuth);

        req = dir2.HttpRequests[1];
        Assert.Equal("HTTPFILE", req.Name);
        Assert.Equal(2.0m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/post/file", req.Url);
        Assert.Null(req.Headers);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.File, req.Body.Mode);
        Assert.Equal("image/jpeg", req.Body.ContentType);
        Assert.Equal("{{TestFilesDir}}/homem_aranha.jpg", req.Body.FileSrcPath);
        Assert.Null(req.CustomAuth);

        var ws = Assert.Single(dir2.WebSocketConnections);
        Assert.Equal("WS", ws.Name);
        Assert.Equal(2.0m, ws.HttpVersion);
        Assert.Equal("{{BaseUrlWs}}/{{WsHttp2Endpoint}}", ws.Url);
        Assert.Equal([new(false, "Header1", "ValueHeader1")], ws.Headers);
        Assert.NotNull(ws.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.Basic, ws.CustomAuth.Mode);
        Assert.Equal("{{BasicAuthLogin}}", ws.CustomAuth.BasicAuthLogin);
        Assert.Equal("{{BasicAuthPassword}}", ws.CustomAuth.BasicAuthPassword);
        Assert.Equal(new(11, false, 10, true), ws.CompressionOptions);
        Assert.Equal([new(true, "Sub1", null)], ws.Subprotocols);

        Assert.NotNull(ws.ClientMessages);
        Assert.Equal(2, ws.ClientMessages.Count);

        var wsmsg = ws.ClientMessages[0];
        Assert.Equal("WSMSGJSON", wsmsg.Name);
        Assert.Equal(PororocaWebSocketClientMessageContentMode.Raw, wsmsg.ContentMode);
        Assert.Equal("{\"elemento\":\"{{SpecialValue1}}\"}", wsmsg.RawContent);
        Assert.Equal(PororocaWebSocketMessageRawContentSyntax.Json, wsmsg.RawContentSyntax);
        Assert.False(wsmsg.DisableCompressionForThis);
        Assert.Equal(PororocaWebSocketMessageDirection.FromClient, wsmsg.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Text, wsmsg.MessageType);

        wsmsg = ws.ClientMessages[1];
        Assert.Equal("WSMSGFILE", wsmsg.Name);
        Assert.Equal(PororocaWebSocketClientMessageContentMode.File, wsmsg.ContentMode);
        Assert.Equal("{{TestFilesDir}}/homem_aranha.jpg", wsmsg.FileSrcPath);
        Assert.Null(wsmsg.RawContent);
        Assert.Null(wsmsg.RawContentSyntax);
        Assert.False(wsmsg.DisableCompressionForThis);
        Assert.Equal(PororocaWebSocketMessageDirection.FromClient, wsmsg.Direction);
        Assert.Equal(PororocaWebSocketMessageType.Binary, wsmsg.MessageType);

        var dir3 = Assert.Single(dir2.Folders);
        Assert.Equal("DIR3", dir3.Name);
        Assert.Equal(2, dir3.HttpRequests.Count);

        req = dir3.HttpRequests[0];
        Assert.Equal("HTTPURLENCODED", req.Name);
        Assert.Equal(3.0m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/test/post/urlencoded", req.Url);
        Assert.Null(req.Headers);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.UrlEncoded, req.Body.Mode);
        Assert.Equal([
            new(true, "a", "xyz"),
            new(true, "b", "123"),
            new(false, "c", "false"),
            new(true, "c", "true"),
            new(true, "myIdSecret", "{{SpecialValue1}}")],
            req.Body.UrlEncodedValues);
        Assert.Null(req.CustomAuth);

        req = dir3.HttpRequests[1];
        Assert.Equal("HTTPNONEPEMAUTH", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BadSslClientCertTestsUrl}}", req.Url);
        Assert.Null(req.Headers);
        Assert.Null(req.Body);
        Assert.NotNull(req.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.ClientCertificate, req.CustomAuth.Mode);
        Assert.NotNull(req.CustomAuth.ClientCertificate);
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pem, req.CustomAuth.ClientCertificate.Type);
        Assert.Equal("{{ClientCertificatesDir}}/badssl.com-client-certificate-without-private-key.pem", req.CustomAuth.ClientCertificate.CertificateFilePath);
        Assert.Equal("{{ClientCertificatesDir}}/badssl.com-client-encrypted-private-key.key", req.CustomAuth.ClientCertificate.PrivateKeyFilePath);
        Assert.Equal("{{BadSslClientCertFilePassword}}", req.CustomAuth.ClientCertificate.FilePassword);
    }
}