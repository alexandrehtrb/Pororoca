using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using Pororoca.Domain.Features.TranslateRequest;
using Moq;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection.PororocaWebSocketConnectionTranslator;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Pororoca.Domain.Tests.Features.TranslateRequest.WebSockets.Connection;

public static class PororocaWebSocketCommonTranslatorTests
{
    #region MOCKERS

    private static HttpVersionAvailableVerifier MockHttpVersionOSVerifier(bool valid, string? errorCode) =>
        (decimal x, out string? z) =>
        {
            z = errorCode;
            return valid;
        };

    #endregion

    #region WEBSOCKET HTTP VERSION CHECK

    [Fact]
    public static void Should_not_allow_websocket_with_unsupported_http_version()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(false, TranslateRequestErrors.WebSocketHttpVersionUnavailable);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.HttpVersion = 3.0m;

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Null(clis.wsCli);
        Assert.Null(clis.httpCli);
        Assert.Equal(TranslateRequestErrors.WebSocketHttpVersionUnavailable, errorCode);
    }

    [Fact]
    public static void Should_allow_websocket_with_supported_http_version()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.HttpVersion = 2.0m;

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Null(errorCode);
        Assert.Equal(2, clis.wsCli.Options.HttpVersion.Major);
        Assert.Equal(0, clis.wsCli.Options.HttpVersion.Minor);
    }

    #endregion

    #region INCLUDE CLIENT CERTIFICATES

    [Fact]
    public static void Should_include_client_certificates_correctly()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.CustomAuth = new();
        ws.CustomAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "./cert.crt", null, null);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.Is<PororocaRequestAuthClientCertificate>(
            cc => cc.Type == PororocaRequestAuthClientCertificateType.Pem
               && cc.CertificateFilePath == "./cert.crt"
               && cc.PrivateKeyFilePath == null
               && cc.FilePassword == null
        ))).Returns(httpClient);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        // unfortunately, there is no way of asserting whether a HttpClient
        // has ClientCertificates or not
        // Assert.Single(clis.httpCli!.Options.ClientCertificates);
        // Assert.Equal(httpClient, clis.httpCli!.Options.ClientCertificates[0]);
    }

    [Fact]
    public static void Should_not_include_client_certificates_if_not_specified()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        // unfortunately, there is no way of asserting whether a HttpClient
        // has ClientCertificates or not
        // Assert.Single(clis.httpCli!.Options.ClientCertificates);
        // Assert.Empty(clis.httpCli!.Options.ClientCertificates);
    }

    #endregion

    #region REMOTE TLS CERTIFICATE VALIDATION

    [Fact]
    public static void Should_validate_remote_tls_certificate_if_required()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        // unfortunately, there is no way of asserting 
        // whether a HttpClient validates remote certificates or not
    }

    [Fact]
    public static void Should_not_validate_remote_tls_certificate_if_not_required()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = true;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        // unfortunately, there is no way of asserting 
        // whether a HttpClient validates remote certificates or not
    }

    #endregion

    #region CONNECTION REQUEST HEADERS

    [Fact]
    public static void Should_correctly_set_and_resolve_connection_request_headers_including_auth_headers()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        varResolver.AddVariable(new(true, "K3Value", "V3", true));
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.Headers = new()
        {
            new(false, "K1", "V1"),
            new(true, "K2", "V2"),
            new(true, "K3", "{{K3Value}}")
        };
        ws.CustomAuth = new();
        ws.CustomAuth.SetBasicAuth("usr", "{{K3Value}}");

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        // Cannot validate connection request headers in ClientWebSocket class...
        // This part uses ResolveNonContentHeaders, tested in PororocaRequestCommonTranslator
    }

    #endregion

    #region SUBPROTOCOLS

    [Fact]
    public static void Should_specify_subprotocols_if_required()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        varResolver.AddVariable(new(true, "K3Value", "V3", true));
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.Subprotocols = new()
        {
            new(true, "sub1", null),
            new(false, "sub2", null),
            new(true, "{{K3Value}}", null)
        };

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        // Cannot validate subprotocols in ClientWebSocket class...
        // Subprotocols should be resolved from variables too
    }

    #endregion

    #region COMPRESSION OPTIONS

    [Fact]
    public static void Should_not_enable_compression_if_not_specified()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        Assert.Null(clis.wsCli!.Options.DangerousDeflateOptions);
    }

    [Fact]
    public static void Should_enable_compression_if_specified()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        HttpClient httpClient = new();
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Returns(httpClient);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.CompressionOptions = new(14, true, 12, false);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(clis.wsCli);
        Assert.NotNull(clis.httpCli);
        Assert.Equal(httpClient, clis.httpCli);
        Assert.Null(errorCode);
        Assert.NotNull(clis.wsCli!.Options.DangerousDeflateOptions);
        Assert.Equal(14, clis.wsCli.Options.DangerousDeflateOptions!.ClientMaxWindowBits);
        Assert.True(clis.wsCli.Options.DangerousDeflateOptions!.ClientContextTakeover);
        Assert.Equal(12, clis.wsCli.Options.DangerousDeflateOptions!.ServerMaxWindowBits);
        Assert.False(clis.wsCli.Options.DangerousDeflateOptions!.ServerContextTakeover);
    }

    #endregion

    #region EXCEPTION HANDLING

    [Fact]
    public static void Should_return_generic_error_if_there_is_an_exception()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.CustomAuth = new();
        ws.CustomAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "./cert.crt", null, null);
        Mock<IPororocaHttpClientProvider> httpClientProviderMock = new();
        Exception ex = new("random exception");
        httpClientProviderMock.Setup(x => x.Provide(It.IsAny<bool>(), It.IsAny<PororocaRequestAuthClientCertificate>())).Throws(ex);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, httpClientProviderMock.Object, ws, disableTlsVerification, out var clis, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Null(clis.wsCli);
        Assert.Null(clis.httpCli);
        Assert.Equal(TranslateRequestErrors.WebSocketUnknownConnectionTranslationError, errorCode);
    }

    #endregion

}