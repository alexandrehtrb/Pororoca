using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.TranslateRequest.Common;

internal static class PororocaRequestCommonValidator
{
    internal delegate bool HttpVersionAvailableVerifier(decimal httpVersion, out string? errorCode);
    internal delegate bool FileExistsVerifier(string filePath);

    internal static bool IsValidContentType(string? contentType) =>
        !string.IsNullOrWhiteSpace(contentType) && MimeTypesDetector.AllMimeTypes.Contains(contentType);

    internal static bool ValidateAuthParams(IEnumerable<PororocaVariable> effectiveVars, FileExistsVerifier fileExistsVerifier, PororocaRequestAuth? customAuth, out string? errorCode) =>
        CheckClientCertificateFilesAndPassword(effectiveVars, fileExistsVerifier, customAuth, out errorCode)
     && CheckWindowsAuthParams(effectiveVars, customAuth, out errorCode);

    internal static bool CheckClientCertificateFilesAndPassword(IEnumerable<PororocaVariable> effectiveVars, FileExistsVerifier fileExistsVerifier, PororocaRequestAuth? customAuth, out string? errorCode)
    {
        if (customAuth?.Mode == PororocaRequestAuthMode.ClientCertificate && customAuth?.ClientCertificate is not null)
        {
            goto validation;
        }
        else
        {
            errorCode = null;
            return true;
        }

        validation:
        string resolvedCertificateFilePath = IPororocaVariableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.CertificateFilePath, effectiveVars);
        string resolvedPrivateKeyFilePath = IPororocaVariableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.PrivateKeyFilePath, effectiveVars);
        string resolvedFilePassword = IPororocaVariableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.FilePassword, effectiveVars);

        bool certFileExists = fileExistsVerifier.Invoke(resolvedCertificateFilePath);
        bool prvKeyFileSpecified = !string.IsNullOrWhiteSpace(resolvedPrivateKeyFilePath);
        bool prvKeyFileExists = fileExistsVerifier.Invoke(resolvedPrivateKeyFilePath);
        bool filePasswordIsBlank = string.IsNullOrWhiteSpace(resolvedFilePassword);

        var certType = customAuth.ClientCertificate.Type;
        if (!certFileExists)
        {
            errorCode = certType switch
            {
                PororocaRequestAuthClientCertificateType.Pkcs12 => TranslateRequestErrors.ClientCertificatePkcs12CertificateFileNotFound,
                PororocaRequestAuthClientCertificateType.Pem => TranslateRequestErrors.ClientCertificatePemCertificateFileNotFound,
                _ => string.Empty
            };
            return false;
        }
        else if (certType == PororocaRequestAuthClientCertificateType.Pem && prvKeyFileSpecified && !prvKeyFileExists)
        {
            errorCode = TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound;
            return false;
        }
        else if (certType == PororocaRequestAuthClientCertificateType.Pkcs12 && filePasswordIsBlank)
        {
            errorCode = TranslateRequestErrors.ClientCertificatePkcs12PasswordCannotBeBlank;
            return false;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }

    internal static bool CheckWindowsAuthParams(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? customAuth, out string? errorCode)
    {
        if (customAuth?.Mode == PororocaRequestAuthMode.Windows && customAuth?.Windows is not null)
        {
            goto validation;
        }
        else
        {
            errorCode = null;
            return true;
        }

        validation:
        if (customAuth.Windows.UseCurrentUser)
        {
            errorCode = null;
            return true;
        }

        string resolvedLogin = IPororocaVariableResolver.ReplaceTemplates(customAuth.Windows.Login, effectiveVars);
        if (string.IsNullOrWhiteSpace(resolvedLogin))
        {
            errorCode = TranslateRequestErrors.WindowsAuthLoginCannotBeBlank;
            return false;
        }

        string resolvedPassword = IPororocaVariableResolver.ReplaceTemplates(customAuth.Windows.Password, effectiveVars);
        if (string.IsNullOrWhiteSpace(resolvedPassword))
        {
            errorCode = TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank;
            return false;
        }

        string resolvedDomain = IPororocaVariableResolver.ReplaceTemplates(customAuth.Windows.Domain, effectiveVars);
        if (string.IsNullOrWhiteSpace(resolvedDomain))
        {
            errorCode = TranslateRequestErrors.WindowsAuthDomainCannotBeBlank;
            return false;
        }

        errorCode = null;
        return true;
    }
}