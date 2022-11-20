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
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(false, TranslateRequestErrors.WebSocketHttpVersionUnavailable);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.HttpVersion = 2.0m;

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Null(wsCli);
        Assert.Equal(TranslateRequestErrors.WebSocketHttpVersionUnavailable, errorCode);
    }

    [Fact]
    public static void Should_allow_websocket_with_supported_http_version()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.HttpVersion = 2.0m;

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
        Assert.Null(errorCode);
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
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();
#pragma warning disable SYSLIB0026
        X509Certificate2 resolvedCert = new();
#pragma warning restore SYSLIB0026
        clientCertsProviderMock.Setup(x => x.Provide(It.Is<PororocaRequestAuthClientCertificate>(
            cc => cc.Type == PororocaRequestAuthClientCertificateType.Pem
               && cc.CertificateFilePath == "./cert.crt"
               && cc.PrivateKeyFilePath == null
               && cc.FilePassword == null
        ))).Returns(resolvedCert);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
        Assert.Null(errorCode);
        Assert.Single(wsCli!.Options.ClientCertificates);
        Assert.Equal(resolvedCert, wsCli!.Options.ClientCertificates[0]);
    }

    [Fact]
    public static void Should_not_include_client_certificates_if_not_specified()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
        Assert.Null(errorCode);
        Assert.Empty(wsCli!.Options.ClientCertificates);
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
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
        Assert.Null(errorCode);
        Assert.Null(wsCli!.Options.RemoteCertificateValidationCallback);
    }

    [Fact]
    public static void Should_not_validate_remote_tls_certificate_if_not_required()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = true;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        PororocaWebSocketConnection ws = new(string.Empty);
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
        Assert.Null(errorCode);
        Assert.NotNull(wsCli!.Options.RemoteCertificateValidationCallback);
        Assert.True(wsCli!.Options.RemoteCertificateValidationCallback!.Invoke(new(), null, null, SslPolicyErrors.None));
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
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();
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
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
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
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.Subprotocols = new()
        {
            new(true, "sub1", null),
            new(false, "sub2", null),
            new(true, "{{K3Value}}", null)
        };

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
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
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
        Assert.Null(errorCode);
        Assert.Null(wsCli!.Options.DangerousDeflateOptions);
    }

    [Fact]
    public static void Should_enable_compression_if_specified()
    {
        // GIVEN
        PororocaCollection varResolver = new(string.Empty);
        bool disableTlsVerification = false;
        var httpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();
        PororocaWebSocketConnection ws = new(string.Empty);
        ws.CompressionOptions = new(14, true, 12, false);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.NotNull(wsCli);
        Assert.Null(errorCode);
        Assert.NotNull(wsCli!.Options.DangerousDeflateOptions);
        Assert.Equal(14, wsCli.Options.DangerousDeflateOptions!.ClientMaxWindowBits);
        Assert.True(wsCli.Options.DangerousDeflateOptions!.ClientContextTakeover);
        Assert.Equal(12, wsCli.Options.DangerousDeflateOptions!.ServerMaxWindowBits);
        Assert.False(wsCli.Options.DangerousDeflateOptions!.ServerContextTakeover);
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
        Mock<IPororocaClientCertificatesProvider> clientCertsProviderMock = new();
        Exception ex = new("random exception");
        clientCertsProviderMock.Setup(x => x.Provide(It.IsAny<PororocaRequestAuthClientCertificate>())).Throws(ex);

        // WHEN
        bool valid = TryTranslateConnection(httpVersionVerifier, varResolver, clientCertsProviderMock.Object, ws, disableTlsVerification, out var wsCli, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Null(wsCli);
        Assert.Equal(TranslateRequestErrors.WebSocketUnknownConnectionTranslationError, errorCode);
    }

    #endregion

}