namespace Pororoca.Domain.Features.TranslateRequest;

public static class TranslateRequestErrors
{
    public const string ClientCertificateFileNotFound = "TranslateRequest_ClientCertificateFileNotFound";
    public const string ClientCertificatePkcs12PasswordCannotBeBlank = "TranslateRequest_ClientCertificatePkcs12PasswordCannotBeBlank";
    public const string ClientCertificatePrivateKeyFileNotFound = "TranslateRequest_ClientCertificatePrivateKeyFileNotFound";
    public const string ContentTypeCannotBeBlankReqBodyRawOrFile = "TranslateRequest_ContentTypeCannotBeBlankReqBodyRawOrFile";
    public const string Http2UnavailableInOSVersion = "TranslateRequest_Http2UnavailableInOSVersion";
    public const string Http3UnavailableInOSVersion = "TranslateRequest_Http3UnavailableInOSVersion";
    public const string InvalidContentTypeFormData = "TranslateRequest_InvalidContentTypeFormData";
    public const string InvalidContentTypeRawOrFile = "TranslateRequest_InvalidContentTypeRawOrFile";
    public const string InvalidUrl = "TranslateRequest_InvalidUrl";
    public const string ReqBodyFileNotFound = "TranslateRequest_ReqBodyFileNotFound";
    public const string UnknownRequestTranslationError = "TranslateRequest_UnknownRequestTranslationError";
}