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

    internal static bool ValidateAuthParams(IPororocaVariableResolver variableResolver, FileExistsVerifier fileExistsVerifier, PororocaRequestAuth? customAuth, out string? errorCode) =>
        CheckClientCertificateFilesAndPassword(variableResolver, fileExistsVerifier, customAuth, out errorCode)
     && CheckWindowsAuthParams(variableResolver, customAuth, out errorCode);

    internal static bool CheckClientCertificateFilesAndPassword(IPororocaVariableResolver variableResolver, FileExistsVerifier fileExistsVerifier, PororocaRequestAuth? customAuth, out string? errorCode)
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
        string resolvedCertificateFilePath = variableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.CertificateFilePath);
        string resolvedPrivateKeyFilePath = variableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.PrivateKeyFilePath);
        string resolvedFilePassword = variableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.FilePassword);

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

    internal static bool CheckWindowsAuthParams(IPororocaVariableResolver variableResolver, PororocaRequestAuth? customAuth, out string? errorCode)
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

        string resolvedLogin = variableResolver.ReplaceTemplates(customAuth.Windows.Login);
        if (string.IsNullOrWhiteSpace(resolvedLogin))
        {
            errorCode = TranslateRequestErrors.WindowsAuthLoginCannotBeBlank;
            return false;
        }

        string resolvedPassword = variableResolver.ReplaceTemplates(customAuth.Windows.Password);
        if (string.IsNullOrWhiteSpace(resolvedPassword))
        {
            errorCode = TranslateRequestErrors.WindowsAuthPasswordCannotBeBlank;
            return false;
        }

        string resolvedDomain = variableResolver.ReplaceTemplates(customAuth.Windows.Domain);
        if (string.IsNullOrWhiteSpace(resolvedDomain))
        {
            errorCode = TranslateRequestErrors.WindowsAuthDomainCannotBeBlank;
            return false;
        }

        errorCode = null;
        return true;
    }
}