using Moq;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;
using static Pororoca.Domain.Features.TranslateRequest.Http.PororocaHttpRequestValidator;

namespace Pororoca.Domain.Tests.Features.TranslateRequest.Http;

public static class PororocaHttpRequestValidatorTests
{
    #region MOCKERS

    private static HttpVersionAvailableVerifier MockHttpVersionOSVerifier(bool valid, string? errorCode) =>
        (decimal x, out string? z) =>
        {
            z = errorCode;
            return valid;
        };

    private static Mock<IPororocaVariableResolver> MockVariableResolver(string key, string value) =>
        MockVariableResolver(new() { { key, value } });

    private static Mock<IPororocaVariableResolver> MockVariableResolver(Dictionary<string, string> kvs)
    {
        Mock<IPororocaVariableResolver> mockedVariableResolver = new();

        string f(string? k) => k == null ? string.Empty : kvs.ContainsKey(k) ? kvs[k] : k;

        mockedVariableResolver.Setup(x => x.ReplaceTemplates(It.IsAny<string?>()))
                              .Returns((Func<string?, string>)f);

        return mockedVariableResolver;
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
        PororocaHttpRequestFormDataParam p1 = new(true, "p1");
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
        PororocaHttpRequestFormDataParam p1 = new(false, "p1");
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
        body.SetFormDataContent(Array.Empty<PororocaHttpRequestFormDataParam>());
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
    public static void Should_accept_file_body_if_file_is_found()
    {
        // GIVEN
        const string url = "http://www.url.br";
        const string fileName = "testfilecontent1.json";
        string testFilePath = GetTestFilePath(fileName);
        var mockedVariableResolver = MockVariableResolver("{{MyFilePath}}", testFilePath);
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(new Dictionary<string, bool>() { { testFilePath, true } });
        PororocaHttpRequest req = new();
        req.UpdateUrl(url);
        req.UpdateHttpVersion(1.1m);
        var body = new PororocaHttpRequestBody();
        body.SetFileContent("{{MyFilePath}}", "text/plain");
        req.UpdateBody(body);

        // WHEN
        Assert.True(IsValidRequest(mockedHttpVersionOSVerifier, mockedFileExistsVerifier, mockedVariableResolver.Object, req, out string? errorCode));

        // THEN
        mockedVariableResolver.Verify(x => x.ReplaceTemplates(url), Times.Once);
        mockedVariableResolver.Verify(x => x.ReplaceTemplates("{{MyFilePath}}"), Times.Once);
        Assert.Null(errorCode);
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
        PororocaHttpRequestFormDataParam p1 = new(true, "p1");
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
        PororocaHttpRequestFormDataParam p1 = new(false, "p1");
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
        var mockedFileExistsVerifier = MockFileExistsVerifier(false);
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
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
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
        var mockedVariableResolver = MockVariableResolver(new()
        {
            {"{{CertificateFilePath}}", "./cert.p12"},
            {"{{PrivateKeyFilePassword}}", "prvkeypwd"}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
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
        var mockedVariableResolver = MockVariableResolver(new()
        {
            {"{{CertificateFilePath}}", "./cert.pem"},
            {"{{PrivateKeyFilePath}}", "./private_key.key"},
            {"{{PrivateKeyFilePassword}}", filePassword}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
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
        var mockedVariableResolver = MockVariableResolver(new()
        {
            {"{{CertificateFilePath}}", "./cert.pem"},
            {"{{FilePassword}}", filePassword}
        });
        var mockedHttpVersionOSVerifier = MockHttpVersionOSVerifier(true, null);
        var mockedFileExistsVerifier = MockFileExistsVerifier(true);
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

    private static string GetTestFilePath(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
    }
}