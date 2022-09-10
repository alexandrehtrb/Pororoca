using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using Pororoca.Domain.Features.TranslateRequest;

namespace Pororoca.Domain.Tests.Features.TranslateRequest.Common;

public static class PororocaRequestCommonValidatorTests
{
    #region MOCKERS

    private static FileExistsVerifier MockFileExistsVerifier(Dictionary<string, bool> fileList) =>
        (string s) =>
        {
            bool exists = fileList.ContainsKey(s) && fileList[s];
            return exists;
        };

    #endregion

    #region CONTENT TYPE

    [Theory]
    [InlineData("application/json")]
    [InlineData("text/plain")]
    [InlineData("application/excel")]
    public static void Should_detect_valid_http_content_types(string mimeType) =>
        Assert.True(IsValidContentType(mimeType));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("application/eggcel")]
    public static void Should_detect_invalid_http_content_types(string? mimeType) =>
        Assert.False(IsValidContentType(mimeType));

    #endregion


    #region CLIENT CERTIFICATES

    [Fact]
    public static void Should_not_check_client_certificate_files_if_no_custom_auth()
    {
        // GIVEN
        static bool mockFileExists(string _) => false;
        PororocaCollection col = new("VarResolver");
        PororocaRequestAuth? customAuth = null;

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_not_check_client_certificate_files_if_custom_auth_not_client_certificate_kind()
    {
        // GIVEN
        static bool mockFileExists(string _) => false;
        PororocaCollection col = new("VarResolver");
        PororocaRequestAuth? customAuth = new();
        customAuth.SetBasicAuth("usr", "pwd");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_reject_client_certificate_if_certificate_file_is_not_found()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.p12", false }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.p12", false));
        col.AddVariable(new(true, "CertPassword", "pwd", true));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.False(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.ClientCertificateFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_reject_client_certificate_if_PEM_certificate_private_key_file_is_external_and_is_not_found()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.crt", true },
            { "./cert_prv_key.pem", false }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.crt", false));
        col.AddVariable(new(true, "CertPrvKeyPath", "./cert_prv_key.pem", false));
        col.AddVariable(new(true, "CertPassword", "pwd", true));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", "{{CertPrvKeyPath}}", "{{CertPassword}}");

        // WHEN AND THEN
        Assert.False(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.ClientCertificatePrivateKeyFileNotFound, errorCode);
    }

    [Fact]
    public static void Should_reject_client_certificate_if_PKCS12_certificate_has_no_password_along()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.p12", true }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.p12", false));
        col.AddVariable(new(true, "CertPassword", string.Empty, true));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.False(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank, errorCode);
    }

    [Fact]
    public static void Should_allow_valid_PKCS12_certificate_with_password_along()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.p12", true }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.p12", false));
        col.AddVariable(new(true, "CertPassword", "pwd", true));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_allow_valid_PEM_certificate_with_conjoined_private_key_without_password_along()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.crt", true }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.crt", false));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", null, null);

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_allow_valid_PEM_certificate_with_separate_private_key_without_password_along()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.crt", true },
            { "./cert_prv_key.pem", true }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.crt", false));
        col.AddVariable(new(true, "CertPrvKeyPath", "./cert_prv_key.pem", false));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", "{{CertPrvKeyPath}}", null);

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_allow_valid_PEM_certificate_with_conjoined_private_key_with_password_along()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.crt", true }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.crt", false));
        col.AddVariable(new(true, "CertPassword", "pwd", true));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));        
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_allow_valid_PEM_certificate_with_separate_private_key_with_password_along()
    {
        // GIVEN
        var mockFileExists = MockFileExistsVerifier(new()
        {
            { "./cert.crt", true },
            { "./cert_prv_key.pem", true }
        });
        PororocaCollection col = new("VarResolver");
        col.AddVariable(new(true, "CertPath", "./cert.crt", false));
        col.AddVariable(new(true, "CertPrvKeyPath", "./cert_prv_key.pem", false));
        col.AddVariable(new(true, "CertPassword", "pwd", true));
        PororocaRequestAuth? customAuth = new();
        customAuth.SetClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", "{{CertPrvKeyPath}}", "{{CertPassword}}");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col, mockFileExists, customAuth, out string? errorCode));        
        Assert.Null(errorCode);
    }

    #endregion
}