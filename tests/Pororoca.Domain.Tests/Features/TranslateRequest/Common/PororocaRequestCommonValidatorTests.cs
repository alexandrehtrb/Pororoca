using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;

namespace Pororoca.Domain.Tests.Features.TranslateRequest.Common;

public static class PororocaRequestCommonValidatorTests
{
    private static IEnumerable<PororocaVariable> GetEffectiveVariables(this PororocaCollection col) =>
        ((IPororocaVariableResolver)col).GetEffectiveVariables();

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
        Assert.True(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    [Fact]
    public static void Should_not_check_client_certificate_files_if_custom_auth_not_client_certificate_kind()
    {
        // GIVEN
        static bool mockFileExists(string _) => false;
        PororocaCollection col = new("VarResolver");
        var customAuth = PororocaRequestAuth.MakeBasicAuth("usr", "pwd");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
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
        col.Variables.Add(new(true, "CertPath", "./cert.p12", false));
        col.Variables.Add(new(true, "CertPassword", "pwd", true));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.False(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.ClientCertificatePkcs12CertificateFileNotFound, errorCode);
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
        col.Variables.Add(new(true, "CertPath", "./cert.crt", false));
        col.Variables.Add(new(true, "CertPrvKeyPath", "./cert_prv_key.pem", false));
        col.Variables.Add(new(true, "CertPassword", "pwd", true));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", "{{CertPrvKeyPath}}", "{{CertPassword}}");

        // WHEN AND THEN
        Assert.False(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound, errorCode);
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
        col.Variables.Add(new(true, "CertPath", "./cert.p12", false));
        col.Variables.Add(new(true, "CertPassword", string.Empty, true));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.False(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
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
        col.Variables.Add(new(true, "CertPath", "./cert.p12", false));
        col.Variables.Add(new(true, "CertPassword", "pwd", true));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
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
        col.Variables.Add(new(true, "CertPath", "./cert.crt", false));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", null, null);

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
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
        col.Variables.Add(new(true, "CertPath", "./cert.crt", false));
        col.Variables.Add(new(true, "CertPrvKeyPath", "./cert_prv_key.pem", false));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", "{{CertPrvKeyPath}}", null);

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
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
        col.Variables.Add(new(true, "CertPath", "./cert.crt", false));
        col.Variables.Add(new(true, "CertPassword", "pwd", true));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", null, "{{CertPassword}}");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
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
        col.Variables.Add(new(true, "CertPath", "./cert.crt", false));
        col.Variables.Add(new(true, "CertPrvKeyPath", "./cert_prv_key.pem", false));
        col.Variables.Add(new(true, "CertPassword", "pwd", true));
        var customAuth = PororocaRequestAuth.MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertPath}}", "{{CertPrvKeyPath}}", "{{CertPassword}}");

        // WHEN AND THEN
        Assert.True(CheckClientCertificateFilesAndPassword(col.GetEffectiveVariables(), mockFileExists, customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    #endregion

    #region WINDOWS AUTH

    [Fact]
    public static void Should_allow_windows_auth_with_current_user()
    {
        // GIVEN
        PororocaCollection col = new("VarResolver");
        var customAuth = PororocaRequestAuth.MakeWindowsAuth(true, null, null, null);

        // WHEN AND THEN
        Assert.True(CheckWindowsAuthParams(col.GetEffectiveVariables(), customAuth, out string? errorCode));
        Assert.Null(errorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public static void Should_reject_windows_auth_with_other_user_blank_login(string resolvedLogin)
    {
        // GIVEN
        PororocaCollection col = new("VarResolver");
        col.Variables.Add(new(true, "win_login", resolvedLogin, false));
        var customAuth = PororocaRequestAuth.MakeWindowsAuth(false, "{{win_login}}", "win_pwd", "win_domain");

        // WHEN AND THEN
        Assert.False(CheckWindowsAuthParams(col.GetEffectiveVariables(), customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.WindowsAuthLoginCannotBeBlank, errorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public static void Should_reject_windows_auth_with_other_user_blank_password(string resolvedPwd)
    {
        // GIVEN
        PororocaCollection col = new("VarResolver");
        col.Variables.Add(new(true, "win_pwd", resolvedPwd, false));
        var customAuth = PororocaRequestAuth.MakeWindowsAuth(false, "win_login", "{{win_pwd}}", "win_domain");

        // WHEN AND THEN
        Assert.False(CheckWindowsAuthParams(col.GetEffectiveVariables(), customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank, errorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public static void Should_reject_windows_auth_with_other_user_blank_domain(string resolvedDomain)
    {
        // GIVEN
        PororocaCollection col = new("VarResolver");
        col.Variables.Add(new(true, "win_domain", resolvedDomain, false));
        var customAuth = PororocaRequestAuth.MakeWindowsAuth(false, "win_login", "win_pwd", "{{win_domain}}");

        // WHEN AND THEN
        Assert.False(CheckWindowsAuthParams(col.GetEffectiveVariables(), customAuth, out string? errorCode));
        Assert.Equal(TranslateRequestErrors.WindowsAuthDomainCannotBeBlank, errorCode);
    }

    #endregion
}