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

    internal static bool CheckClientCertificateFilesAndPassword(IPororocaVariableResolver variableResolver, FileExistsVerifier fileExistsVerifier, PororocaRequestAuth? customAuth, out string? errorCode)
    {
        if (customAuth?.Mode != PororocaRequestAuthMode.ClientCertificate || customAuth?.ClientCertificate == null)
        {
            errorCode = null;
            return true;
        }
        else
        {
            string resolvedCertificateFilePath = variableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.CertificateFilePath);
            string resolvedPrivateKeyFilePath = variableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.PrivateKeyFilePath);
            string resolvedFilePassword = variableResolver.ReplaceTemplates(customAuth!.ClientCertificate!.FilePassword);

            bool certFileExists = fileExistsVerifier.Invoke(resolvedCertificateFilePath);
            bool prvKeyFileSpecified = !string.IsNullOrWhiteSpace(resolvedPrivateKeyFilePath);
            bool prvKeyFileExists = fileExistsVerifier.Invoke(resolvedPrivateKeyFilePath);
            bool filePasswordIsBlank = string.IsNullOrWhiteSpace(resolvedFilePassword);

            if (!certFileExists)
            {
                errorCode = TranslateRequestErrors.ClientCertificateFileNotFound;
                return false;
            }
            else if (customAuth.ClientCertificate.Type == PororocaRequestAuthClientCertificateType.Pem && prvKeyFileSpecified && !prvKeyFileExists)
            {
                errorCode = TranslateRequestErrors.ClientCertificatePemPrivateKeyFileNotFound;
                return false;
            }
            else if (customAuth.ClientCertificate.Type == PororocaRequestAuthClientCertificateType.Pkcs12 && filePasswordIsBlank)
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
    }
}