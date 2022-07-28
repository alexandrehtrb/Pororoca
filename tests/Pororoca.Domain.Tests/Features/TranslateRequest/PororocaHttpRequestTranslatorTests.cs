using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.PororocaHttpRequestTranslator;

namespace Pororoca.Domain.Tests.Features.TranslateRequest;

public static class PororocaHttpRequestTranslatorTests
{
    #region MOCKERS

    private static HttpVersionAvailableInOSVerifier MockHttpVersionOSVerifier(bool valid, string? errorCode) =>
        (decimal x, out string? z) =>
        {
            z = errorCode;
            return valid;
        };

    private static Mock<IPororocaVariableResolver> MockVariableResolver(string key, string value) =>
        MockVariableResolver(new Dictionary<string, string>() { { key, value } });

    private static Mock<IPororocaVariableResolver> MockVariableResolver(IDictionary<string, string> kvs)
    {
        Mock<IPororocaVariableResolver> mockedVariableResolver = new();

        string f(string? k) => k == null ? string.Empty : kvs.ContainsKey(k) ? kvs[k] : k;

        mockedVariableResolver.Setup(x => x.ReplaceTemplates(It.IsAny<string?>()))
                              .Returns((Func<string?, string>)f);

        return mockedVariableResolver;
    }

    private static Func<string, bool> MockFileExistsVerifier(bool exists) =>
        (string s) => exists;

    private static Func<string, bool> MockFileExistsVerifier(IDictionary<string, bool> fileList) =>
        (string s) =>
        {
            bool exists = fileList.ContainsKey(s) && fileList[s];
            return exists;
        };

    #endregion

    #region IS VALID REQUEST, REQUEST URL, CONTENT TYPE, FILE EXISTS

    [Fact]
    public static void Should_detect_invalid_request_if_URL_is_invalid()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "url/api/qry?x=abc";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.InvalidUrl, errorCode);
    }

    [Fact]
    public static void Should_detect_invalid_request_if_http_version_is_not_supported_in_operating_system()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(false, TranslateRequestErrors.Http3UnavailableInOSVersion);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(3.0m);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.Http3UnavailableInOSVersion, errorCode);
    }

    [Fact]
    public static void Should_block_request_if_has_raw_body_but_no_content_type()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        body.SetRawContent("", "");
        req.UpdateBody(body);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRawOrFile, errorCode);
    }

    [Fact]
    public static void Should_block_request_if_has_file_body_but_no_content_type()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        body.SetFileContent("", "");
        req.UpdateBody(body);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRawOrFile, errorCode);
    }

    [Fact]
    public static void Should_block_request_if_requires_valid_but_has_invalid_content_type()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        body.SetRawContent("abc", "flafubal");
        req.UpdateBody(body);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.InvalidContentTypeRawOrFile, errorCode);
    }

    [Fact]
    public static void Should_not_verify_content_type_if_has_no_request_body()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_not_verify_content_type_if_has_form_url_encoded_body()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        body.SetUrlEncodedContent(Array.Empty<PororocaKeyValueParam>());
        req.UpdateBody(body);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_reject_invalid_content_type_of_enabled_params_if_has_multipart_form_data_body()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        PororocaRequestFormDataParam p1 = new(true, "p1");
        p1.SetTextValue("oi", "text/plem");
        body.SetFormDataContent(new[] { p1 });
        req.UpdateBody(body);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.InvalidContentTypeFormData, errorCode);
    }

    [Fact]
    public static void Should_accept_invalid_content_type_of_disabled_params_if_has_multipart_form_data_body()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        PororocaRequestFormDataParam p1 = new(false, "p1");
        p1.SetTextValue("oi", "text/plem");
        body.SetFormDataContent(new[] { p1 });
        req.UpdateBody(body);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_accept_no_params_if_has_multipart_form_data_body()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        body.SetFormDataContent(Array.Empty<PororocaRequestFormDataParam>());
        req.UpdateBody(body);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_reject_file_body_if_file_not_found()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        body.SetFileContent("Ç://Uindous/sistem31/a.txt", "text/plain");
        req.UpdateBody(body);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.ReqBodyFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_reject_multipart_form_data_body_if_any_enabled_param_with_file_not_found()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        PororocaRequestFormDataParam p1 = new(true, "p1");
        p1.SetFileValue("Ç://Uindous/sistem31/a.txt", "text/plain");
        body.SetFormDataContent(new[] { p1 });
        req.UpdateBody(body);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Equal(TranslateRequestErrors.ReqBodyFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_accept_multipart_form_data_body_with_disabled_params_and_file_not_found()
    {
        // GIVEN
        const string urlTemplate = "{{url}}";
        const string url = "http://www.url.br";
        var mockedVariableResolver = MockVariableResolver(urlTemplate, url);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
        PororocaHttpRequest req = new();
        req.UpdateUrl(urlTemplate);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        PororocaRequestFormDataParam p1 = new(false, "p1");
        p1.SetFileValue("Ç://Uindous/sistem31/a.txt", "text/plain");
        body.SetFormDataContent(new[] { p1 });
        req.UpdateBody(body);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(urlTemplate), Times.Once);
        Assert.Null(errorCode);
    }

    #region IS VALID CLIENT CERTIFICATE AUTH

    [Fact]
    public static void Should_reject_req_with_client_certificate_auth_if_PKCS12_cert_file_is_not_found()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver("{{CertificateFilePath}}", "./cert.p12");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.p12", false }
        });
        PororocaRequestAuth auth = new();
        auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, "prvkeypwd");
        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(auth);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("prvkeypwd"), Times.Once);
        Assert.Equal(TranslateRequestErrors.ClientCertificateFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_reject_req_with_client_certificate_auth_if_PEM_cert_file_is_not_found()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver("{{CertificateFilePath}}", "./cert.pem");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.pem", false },
            { "./private_key.key", true }
        });
        PororocaRequestAuth auth = new();
        auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", "./private_key.key", "prvkeypwd");
        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(auth);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("./private_key.key"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("prvkeypwd"), Times.Once);
        Assert.Equal(TranslateRequestErrors.ClientCertificateFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_reject_req_with_client_certificate_auth_if_PEM_private_key_file_is_specified_and_not_found()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver("{{PrivateKeyFilePath}}", "./private_key.key");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.pem", true },
            { "./private_key.key", false }
        });
        PororocaRequestAuth auth = new();
        auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "./cert.pem", "{{PrivateKeyFilePath}}", "prvkeypwd");
        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(auth);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("./cert.pem"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{PrivateKeyFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("prvkeypwd"), Times.Once);
        Assert.Equal(TranslateRequestErrors.ClientCertificatePrivateKeyFileNotFound, errorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public static void Should_reject_req_with_client_certificate_auth_if_PKCS12_without_password(string? filePassword)
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver("{{CertificateFilePath}}", "./cert.p12");
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.p12", true }
        });
        PororocaRequestAuth auth = new();
        auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, filePassword);
        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(auth);

        // WHEN
        Assert.False(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(null), Times.Exactly(2));
        Assert.Equal(TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank, errorCode);
    }

    [Fact]
    public static void Should_allow_req_with_client_certificate_auth_and_valid_PKCS12_params()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            {"{{CertificateFilePath}}", "./cert.p12"},
            {"{{PrivateKeyFilePassword}}", "prvkeypwd"}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.p12", true }
        });
        PororocaRequestAuth auth = new();
        auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, "{{PrivateKeyFilePassword}}");
        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(auth);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{PrivateKeyFilePassword}}"), Times.Once);
        Assert.Null(errorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("my_password")]
    public static void Should_allow_req_with_client_certificate_auth_and_valid_PEM_params_separate_private_key_file(string filePassword)
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            {"{{CertificateFilePath}}", "./cert.pem"},
            {"{{PrivateKeyFilePath}}", "./private_key.key"},
            {"{{PrivateKeyFilePassword}}", filePassword}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.pem", true },
            { "./private_key.key", true }
        });
        PororocaRequestAuth auth = new();
        auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", "{{PrivateKeyFilePath}}", "{{PrivateKeyFilePassword}}");
        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(auth);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{PrivateKeyFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{PrivateKeyFilePassword}}"), Times.Once);
        Assert.Null(errorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("my_password")]
    public static void Should_allow_req_with_client_certificate_auth_and_valid_PEM_params_conjoined_private_key_file(string filePassword)
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            {"{{CertificateFilePath}}", "./cert.pem"},
            {"{{FilePassword}}", filePassword}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>()
        {
            { "./cert.pem", true }
        });
        PororocaRequestAuth auth = new();
        auth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", null, "{{FilePassword}}");
        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(auth);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{FilePassword}}"), Times.Once);
        Assert.Null(errorCode);
    }

    #endregion

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
        var v = ResolveHttpVersion(httpVersion);

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
    public static void Should_resolve_content_headers_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{ExpiresHeader}}", "Expires" },
            { "{{ExpiresAt}}", "2021-12-02" }
        });

        var headers = new PororocaKeyValueParam[]
        {
            new(true, "Content-Type", "application/json"),
            new(true, "Content-Language", "pt-BR"),
            new(false, "Content-MD5", "md5"),
            new(false, "{{ExpiresHeader}}", "2020-05-01"),
            new(true, "{{ExpiresHeader}}", "{{ExpiresAt}}"),
            new(true, "{{ExpiresHeader}}", "{{ExpiresAt2}}"),
            new(true, "Date", "2021-12-01")
        };
        PororocaRequestAuth reqAuth = new();
        reqAuth.SetBearerAuth("tkn");

        PororocaHttpRequest req = new();
        req.UpdateHeaders(headers);
        req.UpdateCustomAuth(reqAuth);

        // WHEN
        var contentHeaders = ResolveContentHeaders(mockedVariableResolver.Object, req);

        // THEN

        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Type"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Language"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-MD5"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{ExpiresHeader}}"), Times.Exactly(2));
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Date"), Times.Once);
        // Resolve headers values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("application/json"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("pt-BR"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("md5"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("2020-05-01"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{ExpiresAt}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{ExpiresAt2}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("2021-12-01"), Times.Once);

        Assert.Equal(2, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("Content-Language"));
        Assert.Equal("pt-BR", contentHeaders["Content-Language"]);
        Assert.True(contentHeaders.ContainsKey("Expires"));
        Assert.Equal("2021-12-02", contentHeaders["Expires"]);
    }

    [Fact]
    public static void Should_resolve_non_content_headers_no_auth_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{CookieHeader}}", "Cookie" },
            { "{{TestCookie2}}", "cookie2" }
        });

        var headers = new PororocaKeyValueParam[]
        {
            new(true, "Content-Type", "application/json"),
            new(true, "Content-Language", "pt-BR"),
            new(true, "If-Modified-Since", "2021-10-03"),
            new(false, "{{CookieHeader}}", "{{TestCookie1}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie2}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie3}}")
        };

        PororocaHttpRequest req = new();
        req.UpdateHeaders(headers);

        // WHEN
        var contentHeaders = ResolveNonContentHeaders(mockedVariableResolver.Object, req);

        // THEN
        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Type"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Language"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("If-Modified-Since"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CookieHeader}}"), Times.Exactly(2));
        // Resolve headers values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("application/json"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("pt-BR"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("2021-10-03"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie1}}"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie2}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie3}}"), Times.Once);

        Assert.Equal(2, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
    }

    [Fact]
    public static void Should_resolve_non_content_headers_auth_but_not_custom_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{CookieHeader}}", "Cookie" },
            { "{{TestCookie2}}", "cookie2" },
            { "{{MyAuthHeader}}", "Authorization" },
            { "{{MyAuthToken}}", "tkn" }
        });

        var headers = new PororocaKeyValueParam[]
        {
            new(true, "Content-Type", "application/json"),
            new(true, "Content-Language", "pt-BR"),
            new(true, "If-Modified-Since", "2021-10-03"),
            new(false, "{{CookieHeader}}", "{{TestCookie1}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie2}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie3}}"),
            new(true, "{{MyAuthHeader}}", "{{MyAuthToken}}")
        };

        PororocaHttpRequest req = new();
        req.UpdateHeaders(headers);

        // WHEN
        var contentHeaders = ResolveNonContentHeaders(mockedVariableResolver.Object, req);

        // THEN
        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Type"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Language"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("If-Modified-Since"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CookieHeader}}"), Times.Exactly(2));
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{MyAuthHeader}}"), Times.Once);
        // Resolve headers values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("application/json"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("pt-BR"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("2021-10-03"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie1}}"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie2}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie3}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{MyAuthToken}}"), Times.Once);

        Assert.Equal(3, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
        Assert.True(contentHeaders.ContainsKey("Authorization"));
        Assert.Equal("tkn", contentHeaders["Authorization"]);
    }

    [Fact]
    public static void Should_resolve_non_content_headers_with_custom_basic_auth_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{CookieHeader}}", "Cookie" },
            { "{{TestCookie2}}", "cookie2" },
            { "{{Username}}", "usr" },
            { "{{Password}}", "pwd" },
        });

        var headers = new PororocaKeyValueParam[]
        {
            new(true, "Content-Type", "application/json"),
            new(true, "Content-Language", "pt-BR"),
            new(true, "If-Modified-Since", "2021-10-03"),
            new(false, "{{CookieHeader}}", "{{TestCookie1}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie2}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie3}}")
        };

        PororocaRequestAuth reqAuth = new();
        reqAuth.SetBasicAuth("{{Username}}", "{{Password}}");

        PororocaHttpRequest req = new();
        req.UpdateCustomAuth(reqAuth);
        req.UpdateHeaders(headers);

        // WHEN
        var contentHeaders = ResolveNonContentHeaders(mockedVariableResolver.Object, req);

        // THEN
        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Type"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Language"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("If-Modified-Since"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CookieHeader}}"), Times.Exactly(2));
        // Resolve headers values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("application/json"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("pt-BR"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("2021-10-03"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie1}}"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie2}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie3}}"), Times.Once);
        // Resolve basic auth params
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{Username}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{Password}}"), Times.Once);

        Assert.Equal(3, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
        Assert.True(contentHeaders.ContainsKey("Authorization"));
        Assert.Equal("Basic dXNyOnB3ZA==", contentHeaders["Authorization"]);
    }

    [Fact]
    public static void Should_resolve_non_content_headers_with_custom_bearer_auth_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{CookieHeader}}", "Cookie" },
            { "{{TestCookie2}}", "cookie2" },
            { "{{BearerToken}}", "tkn" }
        });

        var headers = new PororocaKeyValueParam[]
        {
            new(true, "Content-Type", "application/json"),
            new(true, "Content-Language", "pt-BR"),
            new(true, "If-Modified-Since", "2021-10-03"),
            new(false, "{{CookieHeader}}", "{{TestCookie1}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie2}}"),
            new(true, "{{CookieHeader}}", "{{TestCookie3}}")
        };

        PororocaRequestAuth reqAuth = new();
        reqAuth.SetBearerAuth("{{BearerToken}}");

        PororocaHttpRequest req = new();
        req.UpdateCustomAuth(reqAuth);
        req.UpdateHeaders(headers);

        // WHEN
        var contentHeaders = ResolveNonContentHeaders(mockedVariableResolver.Object, req);

        // THEN
        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Type"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("Content-Language"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("If-Modified-Since"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CookieHeader}}"), Times.Exactly(2));
        // Resolve headers values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("application/json"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("pt-BR"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("2021-10-03"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie1}}"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie2}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{TestCookie3}}"), Times.Once);
        // Resolve basic auth params
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{BearerToken}}"), Times.Once);

        Assert.Equal(3, contentHeaders.Count);
        Assert.True(contentHeaders.ContainsKey("If-Modified-Since"));
        Assert.Equal("2021-10-03", contentHeaders["If-Modified-Since"]);
        Assert.True(contentHeaders.ContainsKey("Cookie"));
        Assert.Equal("cookie2", contentHeaders["Cookie"]);
        Assert.True(contentHeaders.ContainsKey("Authorization"));
        Assert.Equal("Bearer tkn", contentHeaders["Authorization"]);
    }

    #endregion

    #region REQUEST AUTH CLIENT CERTIFICATE TRANSLATION

    [Fact]
    public static void Should_resolve_PKCS12_client_certificate_params_correctly()
    {
        // GIVEN
        var mockedHttpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{CertificateFilePath}}", "./cert.p12" },
            { "{{PrivateKeyFilePassword}}", "my_pwd" }
        });

        PororocaRequestAuth reqAuth = new();
        reqAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, "{{PrivateKeyFilePassword}}");

        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(reqAuth);

        // WHEN
        Assert.True(TryTranslateRequest(mockedHttpVersionVerifier,
                                        mockedVariableResolver.Object,
                                        req,
                                        out var reqMsg,
                                        out string? errorCode));

        // THEN
        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{PrivateKeyFilePassword}}"), Times.Once);

        var clientCertParamsKv = reqMsg?.Options.FirstOrDefault(o => o.Key == ClientCertificateOptionsKey);
        object? clientCertParamsObj = clientCertParamsKv != null ? clientCertParamsKv.Value.Value : (object?)null;

        Assert.NotNull(clientCertParamsObj);
        Assert.True(clientCertParamsObj is PororocaRequestAuthClientCertificate);
        var clientCertParams = (PororocaRequestAuthClientCertificate)clientCertParamsObj!;
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pkcs12, clientCertParams.Type);
        Assert.Equal("./cert.p12", clientCertParams.CertificateFilePath);
        Assert.Null(clientCertParams.PrivateKeyFilePath);
        Assert.Equal("my_pwd", clientCertParams.FilePassword);
    }

    [Fact]
    public static void Should_resolve_PEM_client_certificate_params_correctly_separate_private_key()
    {
        // GIVEN
        var mockedHttpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{CertificateFilePath}}", "./cert.pem" },
            { "{{PrivateKeyFilePath}}", "./private_key.key" },
            { "{{PrivateKeyFilePassword}}", "my_pwd" }
        });

        PororocaRequestAuth reqAuth = new();
        reqAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", "{{PrivateKeyFilePath}}", "{{PrivateKeyFilePassword}}");

        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(reqAuth);

        // WHEN
        Assert.True(TryTranslateRequest(mockedHttpVersionVerifier,
                                        mockedVariableResolver.Object,
                                        req,
                                        out var reqMsg,
                                        out string? errorCode));

        // THEN
        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{PrivateKeyFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{PrivateKeyFilePassword}}"), Times.Once);

        var clientCertParamsKv = reqMsg?.Options.FirstOrDefault(o => o.Key == ClientCertificateOptionsKey);
        object? clientCertParamsObj = clientCertParamsKv != null ? clientCertParamsKv.Value.Value : (object?)null;

        Assert.NotNull(clientCertParamsObj);
        Assert.True(clientCertParamsObj is PororocaRequestAuthClientCertificate);
        var clientCertParams = (PororocaRequestAuthClientCertificate)clientCertParamsObj!;
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pem, clientCertParams.Type);
        Assert.Equal("./cert.pem", clientCertParams.CertificateFilePath);
        Assert.Equal("./private_key.key", clientCertParams.PrivateKeyFilePath);
        Assert.Equal("my_pwd", clientCertParams.FilePassword);
    }

    [Fact]
    public static void Should_resolve_PEM_client_certificate_params_correctly_conjoined_private_key()
    {
        // GIVEN
        var mockedHttpVersionVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{CertificateFilePath}}", "./cert.pem" },
            { "{{FilePassword}}", "my_pwd" }
        });

        PororocaRequestAuth reqAuth = new();
        reqAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", null, "{{FilePassword}}");

        PororocaHttpRequest req = new();
        req.UpdateUrl("http://www.pudim.com.br");
        req.UpdateCustomAuth(reqAuth);

        // WHEN
        Assert.True(TryTranslateRequest(mockedHttpVersionVerifier,
                                        mockedVariableResolver.Object,
                                        req,
                                        out var reqMsg,
                                        out string? errorCode));

        // THEN
        // Resolve headers names
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{CertificateFilePath}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{FilePassword}}"), Times.Once);

        var clientCertParamsKv = reqMsg?.Options.FirstOrDefault(o => o.Key == ClientCertificateOptionsKey);
        object? clientCertParamsObj = clientCertParamsKv != null ? clientCertParamsKv.Value.Value : (object?)null;

        Assert.NotNull(clientCertParamsObj);
        Assert.True(clientCertParamsObj is PororocaRequestAuthClientCertificate);
        var clientCertParams = (PororocaRequestAuthClientCertificate)clientCertParamsObj!;
        Assert.Equal(PororocaRequestAuthClientCertificateType.Pem, clientCertParams.Type);
        Assert.Equal("./cert.pem", clientCertParams.CertificateFilePath);
        Assert.Null(clientCertParams.PrivateKeyFilePath);
        Assert.Equal("my_pwd", clientCertParams.FilePassword);
    }

    #endregion

    #region HTTP BODY

    [Fact]
    public static void Should_resolve_form_url_encoded_key_values_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{keyX}}", "key3" },
            { "{{keyXvalue}}", "value3" }
        });

        var formUrlEncodedParams = new PororocaKeyValueParam[]
        {
            new(true, "key1", "abc"),
            new(true, "key1", "def"),
            new(false, "key2", "ghi"),
            new(true, "{{keyX}}", "{{keyXvalue}}")
        };

        PororocaHttpRequest req = new();
        PororocaHttpRequestBody body = new();
        body.SetUrlEncodedContent(formUrlEncodedParams);
        req.UpdateBody(body);

        // WHEN
        var resolvedUrlEncoded = ResolveFormUrlEncodedKeyValues(mockedVariableResolver.Object, req.Body!);

        // THEN
        // Params keys
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("key1"), Times.Exactly(2));
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("key2"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{keyX}}"), Times.Once);
        // Params values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("abc"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("def"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("ghi"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{keyXvalue}}"), Times.Once);

        Assert.Equal(2, resolvedUrlEncoded.Count);
        Assert.True(resolvedUrlEncoded.ContainsKey("key1"));
        Assert.Equal("abc", resolvedUrlEncoded["key1"]);
        Assert.True(resolvedUrlEncoded.ContainsKey("key3"));
        Assert.Equal("value3", resolvedUrlEncoded["key3"]);
    }

    [Fact]
    public static void Should_resolve_no_content_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver("{{myID}}", "3162");
        Dictionary<string, string> resolvedContentHeaders = new(0);

        PororocaHttpRequest req = new();

        // WHEN
        var resolvedReqContent = ResolveRequestContent(mockedVariableResolver.Object, req.Body, resolvedContentHeaders);

        // THEN
        Assert.Null(resolvedReqContent);
    }

    [Fact]
    public static async Task Should_resolve_raw_content_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver("{\"id\":{{myID}}}", "{\"id\":3162}");
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        PororocaHttpRequest req = new();
        PororocaHttpRequestBody body = new();
        body.SetRawContent("{\"id\":{{myID}}}", "application/json");
        req.UpdateBody(body);

        // WHEN
        var resolvedReqContent = ResolveRequestContent(mockedVariableResolver.Object, req.Body, resolvedContentHeaders);

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{\"id\":{{myID}}}"), Times.Once);

        Assert.NotNull(resolvedReqContent);
        Assert.True(resolvedReqContent is StringContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/json", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", resolvedReqContent.Headers.ContentType!.CharSet);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("{\"id\":3162}", contentText);
    }

    [Fact]
    public static async Task Should_resolve_file_content_correctly()
    {
        // GIVEN
        string testFilePath = GetTestFilePath("testfilecontent1.json");
        var mockedVariableResolver = MockVariableResolver("{{MyFilePath}}", testFilePath);
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        PororocaHttpRequest req = new();
        PororocaHttpRequestBody body = new();
        body.SetFileContent("{{MyFilePath}}", "application/json");
        req.UpdateBody(body);

        // WHEN
        var resolvedReqContent = ResolveRequestContent(mockedVariableResolver.Object, req.Body, resolvedContentHeaders);

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{MyFilePath}}"), Times.Once);

        Assert.NotNull(resolvedReqContent);
        Assert.True(resolvedReqContent is StreamContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/json", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("{\"id\":1}", contentText);
    }

    [Fact]
    public static async Task Should_resolve_graphql_content_correctly()
    {
        // GIVEN
        const string qry = "myGraphQlQuery";
        const string variables = "{\"id\":{{CocoId}}}";
        const string resolvedVariables = "{\"id\":19}";
        var mockedVariableResolver = MockVariableResolver(variables, resolvedVariables);
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        PororocaHttpRequest req = new();
        PororocaHttpRequestBody body = new();
        body.SetGraphQlContent(qry, variables);
        req.UpdateBody(body);

        // WHEN
        var resolvedReqContent = ResolveRequestContent(mockedVariableResolver.Object, req.Body, resolvedContentHeaders);

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(variables), Times.Once);

        Assert.NotNull(resolvedReqContent);
        Assert.True(resolvedReqContent is StringContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/json", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("{\"query\":\"myGraphQlQuery\",\"variables\":{\"id\":19}}", contentText);
    }

    [Fact]
    public static async Task Should_resolve_form_url_encoded_content_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{keyX}}", "key3" },
            { "{{keyXvalue}}", "value3" }
        });
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        var formUrlEncodedParams = new PororocaKeyValueParam[]
        {
            new(true, "key1", "abc"),
            new(true, "key1", "def"),
            new(false, "key2", "ghi"),
            new(true, "{{keyX}}", "{{keyXvalue}}")
        };

        PororocaHttpRequest req = new();
        PororocaHttpRequestBody body = new();
        body.SetUrlEncodedContent(formUrlEncodedParams);
        req.UpdateBody(body);

        // WHEN
        var resolvedReqContent = ResolveRequestContent(mockedVariableResolver.Object, req.Body, resolvedContentHeaders);

        // THEN
        // Params keys
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("key1"), Times.Exactly(2));
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("key2"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{keyX}}"), Times.Once);
        // Params values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("abc"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("def"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("ghi"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{keyXvalue}}"), Times.Once);

        Assert.NotNull(resolvedReqContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/x-www-form-urlencoded", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("key1=abc&key3=value3", contentText);
    }


    [Fact]
    public static async Task Should_resolve_form_data_content_correctly()
    {
        // GIVEN
        var mockedVariableResolver = MockVariableResolver(new Dictionary<string, string>()
        {
            { "{{keyX}}", "key3" },
            { "{{keyXvalue}}", "value3" }
        });
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        PororocaRequestFormDataParam p1 = new(true, "key1");
        p1.SetTextValue("oi", "text/plain");
        PororocaRequestFormDataParam p2 = new(true, "key1");
        p2.SetTextValue("oi2", "text/plain");
        PororocaRequestFormDataParam p3 = new(false, "key2");
        p3.SetTextValue("oi2", "text/plain");
        PororocaRequestFormDataParam p4 = new(true, "{{keyX}}");
        p4.SetTextValue("{{keyXvalue}}", "application/json");
        string testFilePath = GetTestFilePath("testfilecontent2.json");
        PororocaRequestFormDataParam p5 = new(true, "key4");
        p5.SetFileValue(testFilePath, "application/json");

        var formDataParams = new[] { p1, p2, p3, p4, p5 };

        PororocaHttpRequest req = new();
        PororocaHttpRequestBody body = new();
        body.SetFormDataContent(formDataParams);
        req.UpdateBody(body);

        // WHEN
        var resolvedReqContent = ResolveRequestContent(mockedVariableResolver.Object, req.Body, resolvedContentHeaders);

        // THEN
        // Params keys
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("key1"), Times.Exactly(2));
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("key2"), Times.Never);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{keyX}}"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("key4"), Times.Once);
        // Params values
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("oi"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("oi2"), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(testFilePath), Times.Exactly(2));

        Assert.NotNull(resolvedReqContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("multipart/form-data", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        Assert.True(resolvedReqContent is MultipartFormDataContent);
        var castedContent = (MultipartFormDataContent)resolvedReqContent;
        Assert.Equal(4, castedContent.Count());

        var p1Content = (StringContent)castedContent.ElementAt(0);
        string? p1ContentText = await p1Content.ReadAsStringAsync();
        Assert.Equal("oi", p1ContentText);
        Assert.NotNull(p1Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p1Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key1", p1Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p1Content.Headers.ContentType);
        Assert.Equal("text/plain", p1Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", p1Content.Headers.ContentType!.CharSet);

        var p2Content = (StringContent)castedContent.ElementAt(1);
        string? p2ContentText = await p2Content.ReadAsStringAsync();
        Assert.Equal("oi2", p2ContentText);
        Assert.NotNull(p2Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p2Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key1", p2Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p2Content.Headers.ContentType);
        Assert.Equal("text/plain", p2Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", p2Content.Headers.ContentType!.CharSet);

        var p3Content = (StringContent)castedContent.ElementAt(2);
        string? p3ContentText = await p3Content.ReadAsStringAsync();
        Assert.Equal("value3", p3ContentText);
        Assert.NotNull(p3Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p3Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key3", p3Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p3Content.Headers.ContentType);
        Assert.Equal("application/json", p3Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", p3Content.Headers.ContentType!.CharSet);

        var p4Content = (StreamContent)castedContent.ElementAt(3);
        string? p4ContentText = await p4Content.ReadAsStringAsync();
        Assert.Equal("{\"id\":2}", p4ContentText);
        Assert.NotNull(p4Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p4Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key4", p4Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p4Content.Headers.ContentType);
        Assert.Equal("application/json", p4Content.Headers.ContentType!.MediaType);
    }

    #endregion

    private static string GetTestFilePath(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
    }
}