using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonValidator;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Domain.Features.TranslateRequest.Http;

public static class PororocaHttpRequestValidator
{
    public static bool IsValidRequest(IPororocaVariableResolver variableResolver, PororocaHttpRequest req, out string? errorCode) =>
        IsValidRequest(IsHttpVersionAvailableInOS, File.Exists, variableResolver, req, out errorCode);

    internal static bool IsValidRequest(HttpVersionAvailableVerifier httpVersionOSVerifier, FileExistsVerifier fileExistsVerifier, IPororocaVariableResolver variableResolver, PororocaHttpRequest req, out string? errorCode) =>
        TryResolveRequestUri(variableResolver, req.Url, out _, out errorCode)
        && httpVersionOSVerifier(req.HttpVersion, out errorCode)
        && HasValidContentTypeForReqBody(req, out errorCode)
        && CheckReqBodyFileExists(req, fileExistsVerifier, out errorCode)
        && CheckClientCertificateFilesAndPassword(variableResolver, fileExistsVerifier, req.CustomAuth, out errorCode);

    private static bool HasValidContentTypeForReqBody(PororocaHttpRequest req, out string? errorCode)
    {
        var bodyMode = req.Body?.Mode;

        if (bodyMode == PororocaHttpRequestBodyMode.File || bodyMode == PororocaHttpRequestBodyMode.Raw)
        {
            bool isBlank = string.IsNullOrWhiteSpace(req.Body?.ContentType);
            bool isInvalid = !IsValidContentType(req.Body!.ContentType);
            errorCode = isBlank ? TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRawOrFile :
                        isInvalid ? TranslateRequestErrors.InvalidContentTypeRawOrFile :
                        null;
            return errorCode == null;
        }
        else if (bodyMode == PororocaHttpRequestBodyMode.FormData)
        {
            bool anyInvalid = req.Body!.FormDataValues!.Any(fd => fd.Enabled && !IsValidContentType(fd.ContentType));
            errorCode = anyInvalid ? TranslateRequestErrors.InvalidContentTypeFormData : null;
            return errorCode == null;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }

    private static bool CheckReqBodyFileExists(PororocaHttpRequest req, FileExistsVerifier fileExistsVerifier, out string? errorCode)
    {
        var bodyMode = req.Body?.Mode;

        if (bodyMode == PororocaHttpRequestBodyMode.File)
        {
            bool fileFound = fileExistsVerifier.Invoke(req.Body!.FileSrcPath!);
            errorCode = fileFound ? null : TranslateRequestErrors.ReqBodyFileNotFound;
            return errorCode == null;
        }
        else if (bodyMode == PororocaHttpRequestBodyMode.FormData)
        {
            bool anyNotFound = req.Body!.FormDataValues!
                                         .Any(fd => fd.Enabled
                                                 && fd.Type == PororocaHttpRequestFormDataParamType.File
                                                 && !fileExistsVerifier.Invoke(fd.FileSrcPath!));
            errorCode = anyNotFound ? TranslateRequestErrors.ReqBodyFileNotFound : null;
            return errorCode == null;
        }
        else
        {
            errorCode = null;
            return true;
        }
    }
}