using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;
using static Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection.PororocaWebSocketConnectionValidator;

namespace Pororoca.Domain.Tests.Features.TranslateRequest.WebSockets.Connection;

public static class PororocaWebSocketConnectionValidatorTests
{
    #region MOCKERS

    private static HttpVersionAvailableVerifier MockHttpVersionOSVerifier(bool valid, string? errorCode) =>
        (decimal x, out string? z) =>
        {
            z = errorCode;
            return valid;
        };

    private static IPororocaVariableResolver MockVariableResolver(string key, string value) =>
        MockVariableResolver(new() { { key, value } });

    private static IPororocaVariableResolver MockVariableResolver(Dictionary<string, string> kvs)
    {
        PororocaCollection col = new("col");
        col.Variables.AddRange(kvs.Select(kv => new PororocaVariable(true, kv.Key, kv.Value, false)));
        return col;
    }

    private static FileExistsVerifier MockFileExistsVerifier(bool exists) =>
        (string s) => exists;

    private static FileExistsVerifier MockFileExistsVerifier(Dictionary<string, bool> fileList) =>
        (string s) =>
        {
            bool exists = fileList.ContainsKey(s) && fileList[s];
            return exists;
        };

    #endregion

    #region IS VALID REQUEST URL

    [Fact]
    public static void Should_detect_invalid_connection_if_URL_is_invalid()
    {
        // GIVEN
        var varResolver = MockVariableResolver("url", "url/api/qry?x=abc");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "{{url}}");

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.InvalidUrl, errorCode);
    }

    [Fact]
    public static void Should_allow_connection_if_URL_is_valid()
    {
        // GIVEN
        var varResolver = MockVariableResolver("url", "ws://www.pudim.com.br");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "{{url}}");

        // WHEN
        Assert.True(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Null(errorCode);
    }

    #endregion

    #region IS VALID WEBSOCKET HTTP VERSION

    [Fact]
    public static void Should_detect_invalid_request_if_websocket_http_version_is_not_supported()
    {
        // GIVEN
        var varResolver = MockVariableResolver("url", "ws://www.pudim.com.br");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(false, TranslateRequestErrors.WebSocketHttpVersionUnavailable);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "{{url}}", HttpVersion: 2.0m);

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.WebSocketHttpVersionUnavailable, errorCode);
    }

    [Fact]
    public static void Should_allow_connection_if_websocket_http_version_is_supported()
    {
        // GIVEN
        var varResolver = MockVariableResolver(new());
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.url.br", HttpVersion: 2.0m);

        // WHEN
        Assert.True(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out _, out string? errorCode));

        // THEN
        Assert.Null(errorCode);
    }

    #endregion

    #region IS VALID CLIENT CERTIFICATE AUTH

    [Fact]
    public static void Should_reject_ws_with_client_certificate_auth_if_PKCS12_cert_file_is_not_found()
    {
        // GIVEN
        var varResolver = MockVariableResolver("CertificateFilePath", "./cert.p12");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        var auth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, "prvkeypwd");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.ClientCertificatePkcs12CertificateFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_reject_ws_with_client_certificate_auth_if_PEM_cert_file_is_not_found()
    {
        // GIVEN
        var varResolver = MockVariableResolver("CertificateFilePath", "./cert.pem");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.pem", false },
            { "./private_key.key", true }
        });
        var auth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", "./private_key.key", "prvkeypwd");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.ClientCertificatePemCertificateFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_reject_ws_with_client_certificate_auth_if_PEM_private_key_file_is_specified_and_not_found()
    {
        // GIVEN
        var varResolver = MockVariableResolver("PrivateKeyFilePath", "./private_key.key");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.pem", true },
            { "./private_key.key", false }
        });
        var auth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "./cert.pem", "{{PrivateKeyFilePath}}", "prvkeypwd");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound, errorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public static void Should_reject_ws_with_client_certificate_auth_if_PKCS12_without_password(string? filePassword)
    {
        // GIVEN
        var varResolver = MockVariableResolver("CertificateFilePath", "./cert.p12");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        var auth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, filePassword);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank, errorCode);
    }

    [Fact]
    public static void Should_allow_req_with_client_certificate_auth_and_valid_PKCS12_params()
    {
        // GIVEN
        var varResolver = MockVariableResolver(new()
        {
            {"CertificateFilePath", "./cert.p12"},
            {"PrivateKeyFilePassword", "prvkeypwd"}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        var auth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, "{{PrivateKeyFilePassword}}");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.True(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Null(errorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("my_password")]
    public static void Should_allow_req_with_client_certificate_auth_and_valid_PEM_params_separate_private_key_file(string filePassword)
    {
        // GIVEN
        var varResolver = MockVariableResolver(new()
        {
            {"CertificateFilePath", "./cert.pem"},
            {"PrivateKeyFilePath", "./private_key.key"},
            {"PrivateKeyFilePassword", filePassword}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        var auth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", "{{PrivateKeyFilePath}}", "{{PrivateKeyFilePassword}}");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.True(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Null(errorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("my_password")]
    public static void Should_allow_req_with_client_certificate_auth_and_valid_PEM_params_conjoined_private_key_file(string filePassword)
    {
        // GIVEN
        var varResolver = MockVariableResolver(new()
        {
            {"CertificateFilePath", "./cert.pem"},
            {"FilePassword", filePassword}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        var auth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", null, "{{FilePassword}}");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.True(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Null(errorCode);
    }

    #endregion

    #region IS VALID WINDOWS AUTH

    [Fact]
    public static void Should_reject_ws_with_invalid_windows_auth()
    {
        // GIVEN
        var varResolver = MockVariableResolver("win_pwd", "");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        var auth = PororocaRequestAuth.MakeWindowsAuth(false, "win_login", "{{win_pwd}}", "win_domain");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: auth);

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank, errorCode);
    }

    #endregion

    #region IS VALID COLLECTION-SCOPED AUTH

    [Fact]
    public static void Should_reject_ws_with_invalid_collection_scoped_windows_auth()
    {
        // GIVEN
        var varResolver = MockVariableResolver("win_pwd", "");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        var collectionScopedAuth = PororocaRequestAuth.MakeWindowsAuth(false, "win_login", "{{win_pwd}}", "win_domain");
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CustomAuth: PororocaRequestAuth.InheritedFromCollection);

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), collectionScopedAuth, ws, out var resolvedUri, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank, errorCode);
    }

    #endregion

    #region ARE VALID WEBSOCKET COMPRESSION OPTIONS

    [Fact]
    public static void Should_allow_websocket_connection_without_compression_options_specified()
    {
        // GIVEN
        var varResolver = MockVariableResolver(new());
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br");

        // WHEN
        Assert.True(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out _, out string? errorCode));

        // THEN
        Assert.Null(errorCode);
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(12, 24)]
    [InlineData(50, 12)]
    [InlineData(-1, 99)]
    public static void Should_forbid_websocket_connection_with_compression_options_max_window_bits_out_of_range(int clientMaxWindowBits, int serverMaxWindowBits)
    {
        // GIVEN
        var varResolver = MockVariableResolver(new());
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CompressionOptions: new(clientMaxWindowBits, true, serverMaxWindowBits, true));

        // WHEN
        Assert.False(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out _, out string? errorCode));

        // THEN
        Assert.Equal(TranslateRequestErrors.WebSocketCompressionMaxWindowBitsOutOfRange, errorCode);
    }

    [Theory]
    [InlineData(9, 9)]
    [InlineData(15, 15)]
    [InlineData(10, 14)]
    [InlineData(12, 9)]
    public static void Should_allow_websocket_connection_with_compression_options_max_window_bits_within_range(int clientMaxWindowBits, int serverMaxWindowBits)
    {
        // GIVEN
        var varResolver = MockVariableResolver(new());
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        PororocaWebSocketConnection ws = new(string.Empty, Url: "ws://www.pudim.com.br", CompressionOptions: new(clientMaxWindowBits, true, serverMaxWindowBits, true));

        // WHEN
        Assert.True(IsValidConnection(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, varResolver.GetEffectiveVariables(), varResolver.CollectionScopedAuth, ws, out _, out string? errorCode));

        // THEN
        Assert.Null(errorCode);
    }


    #endregion

}