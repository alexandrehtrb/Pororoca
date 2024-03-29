namespace Pororoca.Domain.Features.TranslateRequest;

public static class TranslateRequestErrors
{
    public const string ClientCertificatePkcs12CertificateFileNotFound = "TranslateRequest_ClientCertificatePkcs12CertificateFileNotFound";
    public const string ClientCertificatePemCertificateFileNotFound = "TranslateRequest_ClientCertificatePemCertificateFileNotFound";
    public const string ClientCertificatePkcs12PasswordCannotBeBlank = "TranslateRequest_ClientCertificatePkcs12PasswordCannotBeBlank";
    public const string ClientCertificatePemPrivateKeyFileNotFound = "TranslateRequest_ClientCertificatePemPrivateKeyFileNotFound";
    public const string ContentTypeCannotBeBlankReqBodyRaw = "TranslateRequest_ContentTypeCannotBeBlankReqBodyRaw";
    public const string ContentTypeCannotBeBlankReqBodyFile = "TranslateRequest_ContentTypeCannotBeBlankReqBodyFile";
    public const string Http2UnavailableInOSVersion = "TranslateRequest_Http2UnavailableInOSVersion";
    public const string Http3UnavailableInOSVersion = "TranslateRequest_Http3UnavailableInOSVersion";
    public const string InvalidContentTypeFormData = "TranslateRequest_InvalidContentTypeFormData";
    public const string InvalidContentTypeRaw = "TranslateRequest_InvalidContentTypeRaw";
    public const string InvalidContentTypeFile = "TranslateRequest_InvalidContentTypeFile";
    public const string InvalidUrl = "TranslateRequest_InvalidUrl";
    public const string ReqBodyFileNotFound = "TranslateRequest_ReqBodyFileNotFound";
    public const string UnknownRequestTranslationError = "TranslateRequest_UnknownRequestTranslationError";
    public const string WebSocketHttpVersionUnavailable = "TranslateRequest_WebSocketHttpVersionUnavailable";
    public const string WebSocketCompressionMaxWindowBitsOutOfRange = "TranslateRequest_WebSocketCompressionClientMaxWindowBitsOutOfRange";
    public const string WebSocketUnknownConnectionTranslationError = "TranslateRequest_WebSocketUnknownConnectionTranslationError";
    public const string WebSocketNotConnected = "TranslateRequest_WebSocketNotConnected";
    public const string WebSocketClientMessageContentFileNotFound = "TranslateRequest_WebSocketClientMessageContentFileNotFound";
    public const string WebSocketUnknownClientMessageTranslationError = "TranslateRequest_WebSocketUnknownClientMessageTranslationError";
    public const string WindowsAuthLoginCannotBeBlank = "TranslateRequest_WindowsAuthLoginCannotBeBlank";
    public const string WindowsAuthPasswordCannotBeBlank = "TranslateRequest_WindowsAuthPasswordCannotBeBlank";
    public const string WindowsAuthDomainCannotBeBlank = "TranslateRequest_WindowsAuthDomainCannotBeBlank";
}