using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.TranslateRequest;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;

namespace Pororoca.Domain.Tests.Features.TranslateRequest.Common;

public static class PororocaRequestCommonTranslatorTests
{
    #region RESOLVE KEY VALUE PARAMS

    [Fact]
    public static void Should_resolve_null_key_value_params_correctly() =>
        // GIVEN, WHEN AND THEN
        Assert.Equal([], ResolveKVParams([], null));

    [Fact]
    public static void Should_resolve_non_null_key_value_params_correctly()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "K1", "V1", false),
            new(true, "K2", "V2", false),
            new(true, "K3", "P3", true),
            new(true, "V3", "+V3+", true),
        ];
        List<PororocaKeyValueParam> unresolvedParams = [
            new(false, "P0", "V0"),
            new(true, "P1", "{{K1}}"),
            new(true, "P2", "{{K2}}"),
            new(true, "{{K3}}", "{{V3}}"),
            new(true, "P4", "V4"),
        ];

        // WHEN AND THEN
        Assert.Equal([
            new(true, "P1", "V1"),
            new(true, "P2", "V2"),
            new(true, "P3", "+V3+"),
            new(true, "P4", "V4")],
            ResolveKVParams(effectiveVars, unresolvedParams));
    }

    #endregion

    #region REQUEST URL

    [Fact]
    public static void Should_make_uri_if_valid_url_is_resolved_from_variable()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "BaseUrl", "http://www.pudim.com.br", false)
        ];

        // WHEN
        bool valid = TryResolveAndMakeRequestUri(effectiveVars, "{{BaseUrl}}", out var uri, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);
        Assert.NotNull(uri);
        Assert.Equal("http://www.pudim.com.br/", uri!.ToString());
    }

    [Fact]
    public static void Should_make_uri_if_valid_url_but_not_resolved_from_variable()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "BaseUrl", "http://www.pudim.com.br", false)
        ];

        // WHEN
        bool valid = TryResolveAndMakeRequestUri(effectiveVars, "https://www.miniclip.com", out var uri, out string? errorCode);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);
        Assert.NotNull(uri);
        Assert.Equal("https://www.miniclip.com/", uri!.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("{{BaseUrl2}}")]
    [InlineData("{{BaseUrl3}}")]
    [InlineData("fafffafaf")]
    public static void Should_return_error_and_not_make_uri_if_invalid_url_pure_or_from_variable(string unresolvedUrl)
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "BaseUrl", "http://www.pudim.com.br", false),
            new(true, "BaseUrl3", "https:/www.aaa.gov", false),
        ];

        // WHEN
        bool valid = TryResolveAndMakeRequestUri(effectiveVars, unresolvedUrl, out var uri, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRequestErrors.InvalidUrl, errorCode);
        Assert.Null(uri);
    }

    [Theory]
    [InlineData("ftp://192.168.0.1")]
    [InlineData("smtp://user:port@host:25")]
    public static void Should_return_error_and_not_make_uri_if_url_is_not_http_or_websocket(string unresolvedUrl)
    {
        // GIVEN
        PororocaVariable[] effectiveVars = [];

        // WHEN
        bool valid = TryResolveAndMakeRequestUri(effectiveVars, unresolvedUrl, out var uri, out string? errorCode);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRequestErrors.InvalidUrl, errorCode);
        Assert.Null(uri);
    }

    #endregion

    #region HTTP VERSION

    [Theory]
    [InlineData(1, 0, 1.0)]
    [InlineData(1, 1, 1.1)]
    [InlineData(2, 0, 2.0)]
    [InlineData(3, 0, 3.0)]
    public static void Should_make_http_version_object_correctly(int versionMajor, int versionMinor, decimal httpVersion)
    {
        // GIVEN

        // WHEN
        var v = MakeHttpVersion(httpVersion);

        // THEN
        Assert.Equal(versionMajor, v.Major);
        Assert.Equal(versionMinor, v.Minor);
    }

    #endregion

    #region HTTP HEADERS

    [Theory]
    [InlineData("Allow")]
    [InlineData("Content-Disposition")]
    [InlineData("Content-Encoding")]
    [InlineData("Content-Language")]
    [InlineData("Content-Length")]
    [InlineData("Content-Location")]
    [InlineData("Content-MD5")]
    [InlineData("Content-Range")]
    [InlineData("Content-Type")]
    [InlineData("Expires")]
    [InlineData("Last-Modified")]
    public static void Should_detect_content_http_headers_by_the_specification(string headerName) =>
        Assert.True(IsContentHeader(headerName));

    [Theory]
    [InlineData("Date")]
    [InlineData("Authorization")]
    public static void Should_detect_non_content_http_headers_by_the_specification(string headerName) =>
        Assert.False(IsContentHeader(headerName));

    [Fact]
    public static void Should_make_content_headers_correctly()
    {
        // GIVEN
        PororocaKeyValueParam[] resolvedHeaders =
        [
            new(true, "Content-Language", "pt-BR"),
            new(true, "Expires", "2021-12-02")
        ];

        // WHEN
        var contentHeaders = MakeContentHeaders(resolvedHeaders);

        // THEN
        Assert.Equal(2, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("Content-Language"));
        Assert.Equal("pt-BR", contentHeaders["Content-Language"]);
        Assert.True(contentHeaders.ContainsKey("Expires"));
        Assert.Equal("2021-12-02", contentHeaders["Expires"]);
    }

    [Fact]
    public static void Should_make_non_content_headers_no_auth_correctly()
    {
        // GIVEN
        PororocaKeyValueParam[] resolvedHeaders =
        [
            new(true, "If-Modified-Since", "2021-10-03"),
            new(true, "Cookie", "cookie2"),
        ];

        // WHEN
        var contentHeaders = MakeNonContentHeaders(null, resolvedHeaders);

        // THEN
        Assert.Equal(2, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
    }

    [Fact]
    public static void Should_make_non_content_headers_auth_but_not_custom_correctly()
    {
        // GIVEN
        PororocaKeyValueParam[] resolvedHeaders =
        [
            new(true, "If-Modified-Since", "2021-10-03"),
            new(true, "Cookie", "cookie2"),
            new(true, "Authorization", "tkn")
        ];

        // WHEN
        var contentHeaders = MakeNonContentHeaders(null, resolvedHeaders);

        // THEN
        Assert.Equal(3, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
        Assert.True(contentHeaders.ContainsKey("Authorization"));
        Assert.Equal("tkn", contentHeaders["Authorization"]);
    }

    [Fact]
    public static void Should_make_non_content_headers_with_custom_basic_auth_correctly()
    {
        // GIVEN
        PororocaKeyValueParam[] resolvedHeaders =
        [
            new(true, "If-Modified-Since", "2021-10-03"),
            new(true, "Cookie", "cookie2")
        ];

        var resolvedAuth = PororocaRequestAuth.MakeBasicAuth("usr", "pwd");

        // WHEN
        var contentHeaders = MakeNonContentHeaders(resolvedAuth, resolvedHeaders);

        // THEN
        Assert.Equal(3, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
        Assert.True(contentHeaders.ContainsKey("Authorization"));
        Assert.Equal("Basic dXNyOnB3ZA==", contentHeaders["Authorization"]);
    }

    [Fact]
    public static void Should_make_non_content_headers_with_custom_bearer_auth_correctly()
    {
        // GIVEN
        PororocaKeyValueParam[] resolvedHeaders =
        [
            new(true, "If-Modified-Since", "2021-10-03"),
            new(true, "Cookie", "cookie2")
        ];

        var resolvedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");

        // WHEN
        var contentHeaders = MakeNonContentHeaders(resolvedAuth, resolvedHeaders);

        // THEN
        Assert.Equal(3, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
        Assert.True(contentHeaders.ContainsKey("Authorization"));
        Assert.Equal("Bearer tkn", contentHeaders["Authorization"]);
    }

    #endregion

    #region AUTH

    [Fact]
    public static void Should_resolve_no_auth_to_null()
    {
        // GIVEN
        PororocaVariable[] effectiveVars = [];
        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, collectionScopedAuth: null, reqAuth: null);
        // THEN
        Assert.Null(resolvedAuth);
    }

    [Fact]
    public static void Should_resolve_to_collection_scoped_auth_if_request_inherits()
    {
        // GIVEN
        var collectionScopedAuth = PororocaRequestAuth.MakeBearerAuth("{{BearerAuthToken}}");
        var reqAuth = PororocaRequestAuth.InheritedFromCollection;
        PororocaVariable[] effectiveVars =
        [
            new(true, "BearerAuthToken", "tkn", false),
        ];

        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, collectionScopedAuth, reqAuth);

        // THEN
        Assert.NotNull(resolvedAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, resolvedAuth.Mode);
        Assert.Equal("tkn", resolvedAuth.BearerToken);
    }

    [Fact]
    public static void Should_resolve_basic_auth_if_specified()
    {
        // GIVEN
        var reqAuth = PororocaRequestAuth.MakeBasicAuth("{{BasicAuthLogin}}", "{{BasicAuthPassword}}");
        PororocaVariable[] effectiveVars =
        [
            new(true, "BasicAuthLogin", "usr", false),
            new(true, "BasicAuthPassword", "pwd", false)
        ];

        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, null, reqAuth);

        // THEN
        Assert.NotNull(resolvedAuth);
        Assert.Equal(PororocaRequestAuthMode.Basic, resolvedAuth.Mode);
        Assert.Equal("usr", resolvedAuth.BasicAuthLogin);
        Assert.Equal("pwd", resolvedAuth.BasicAuthPassword);
    }

    [Fact]
    public static void Should_resolve_bearer_auth_if_specified()
    {
        // GIVEN
        var reqAuth = PororocaRequestAuth.MakeBearerAuth("{{BearerAuthToken}}");
        PororocaVariable[] effectiveVars =
        [
            new(true, "BearerAuthToken", "tkn", false),
        ];

        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, null, reqAuth);

        // THEN
        Assert.NotNull(resolvedAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, resolvedAuth.Mode);
        Assert.Equal("tkn", resolvedAuth.BearerToken);
    }

    [Fact]
    public static void Should_resolve_PKCS12_client_certificate_auth_if_specified()
    {
        // GIVEN
        var reqAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, "{{PrivateKeyFilePassword}}");
        PororocaVariable[] effectiveVars =
        [
            new(true, "CertificateFilePath", "./cert.p12", false),
            new(true, "PrivateKeyFilePassword", "my_pwd", false)
        ];

        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, null, reqAuth);

        // THEN
        Assert.NotNull(resolvedAuth);
        Assert.NotNull(resolvedAuth.ClientCertificate);
        Assert.Equal(PororocaRequestAuthMode.ClientCertificate, resolvedAuth.Mode);
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pkcs12, resolvedAuth.ClientCertificate.Type);
        Assert.Equal("./cert.p12", resolvedAuth.ClientCertificate.CertificateFilePath);
        Assert.Null(resolvedAuth.ClientCertificate.PrivateKeyFilePath);
        Assert.Equal("my_pwd", resolvedAuth.ClientCertificate.FilePassword);
    }

    [Fact]
    public static void Should_resolve_PEM_client_certificate_params_correctly_separate_private_key()
    {
        // GIVEN
        var reqAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", "{{PrivateKeyFilePath}}", "{{PrivateKeyFilePassword}}");
        PororocaVariable[] effectiveVars =
        [
            new(true, "CertificateFilePath", "./cert.pem", false),
            new(true, "PrivateKeyFilePath", "./private_key.key", false),
            new(true, "PrivateKeyFilePassword", "my_pwd", false)
        ];

        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, null, reqAuth);

        // THEN
        Assert.NotNull(resolvedAuth);
        Assert.Equal(PororocaRequestAuthMode.ClientCertificate, resolvedAuth!.Mode);
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pem, resolvedAuth.ClientCertificate!.Type);
        Assert.Equal("./cert.pem", resolvedAuth.ClientCertificate.CertificateFilePath);
        Assert.Equal("./private_key.key", resolvedAuth.ClientCertificate.PrivateKeyFilePath);
        Assert.Equal("my_pwd", resolvedAuth.ClientCertificate.FilePassword);
    }

    [Fact]
    public static void Should_resolve_PEM_client_certificate_params_correctly_conjoined_private_key()
    {
        // GIVEN
        var reqAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", null, "{{PrivateKeyFilePassword}}");
        PororocaVariable[] effectiveVars =
        [
            new(true, "CertificateFilePath", "./cert.pem", false),
            new(true, "PrivateKeyFilePassword", "my_pwd", false)
        ];

        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, null, reqAuth);

        // THEN
        Assert.NotNull(resolvedAuth);
        Assert.Equal(PororocaRequestAuthMode.ClientCertificate, resolvedAuth!.Mode);
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pem, resolvedAuth.ClientCertificate!.Type);
        Assert.Equal("./cert.pem", resolvedAuth.ClientCertificate.CertificateFilePath);
        Assert.Null(resolvedAuth.ClientCertificate.PrivateKeyFilePath);
        Assert.Equal("my_pwd", resolvedAuth.ClientCertificate.FilePassword);
    }


    [Fact]
    public static void Should_resolve_windows_auth_different_user_if_specified()
    {
        // GIVEN
        var reqAuth = PororocaRequestAuth.MakeWindowsAuth(false, "{{win_login}}", "{{win_pwd}}", "{{win_domain}}");
        PororocaVariable[] effectiveVars =
        [
            new(true, "win_login", "alexandre123", false),
            new(true, "win_pwd", "my_pwd", false),
            new(true, "win_domain", "alexandre.mydomain.net", false)
        ];

        // WHEN
        var resolvedAuth = ResolveRequestAuth(effectiveVars, null, reqAuth);

        // THEN
        Assert.NotNull(resolvedAuth);
        Assert.Equal(PororocaRequestAuthMode.Windows, resolvedAuth!.Mode);
        Assert.NotNull(resolvedAuth.Windows);
        Assert.False(resolvedAuth.Windows.UseCurrentUser);
        Assert.Equal("alexandre123", resolvedAuth.Windows.Login);
        Assert.Equal("my_pwd", resolvedAuth.Windows.Password);
        Assert.Equal("alexandre.mydomain.net", resolvedAuth.Windows.Domain);
    }

    #endregion
}