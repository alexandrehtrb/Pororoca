using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;
using System.Text;
using System.Text.Json;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.TranslateRequest;

public static class PororocaRequestTranslator
{
    public const string ClientCertificateOptionsKey = nameof(ClientCertificateOptionsKey);

    internal delegate bool HttpVersionAvailableInOSVerifier(decimal httpVersion, out string? errorCode);

    #region IS VALID REQUEST

    public static bool IsValidRequest(IPororocaVariableResolver variableResolver, PororocaRequest req, out string? errorCode) =>
        IsValidRequest(AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS, File.Exists, variableResolver, req, out errorCode);
    
    internal static bool IsValidRequest(HttpVersionAvailableInOSVerifier httpVersionOSVerifier, Func<string, bool> fileExistsVerifier, IPororocaVariableResolver variableResolver, PororocaRequest req, out string? errorCode) =>
        TryResolveRequestUri(variableResolver, req.Url, out _, out errorCode)
        && httpVersionOSVerifier(req.HttpVersion, out errorCode)
        && HasValidContentTypeForReqBody(req, out errorCode)
        && CheckReqBodyFileExists(req, fileExistsVerifier, out errorCode)
        && CheckClientCertificateFilesExist(variableResolver, fileExistsVerifier, req, out errorCode);

    internal static bool HasValidContentTypeForReqBody(PororocaRequest req, out string? errorCode)
    {
        PororocaRequestBodyMode? bodyMode = req.Body?.Mode;
        
        if (bodyMode == PororocaRequestBodyMode.File || bodyMode == PororocaRequestBodyMode.Raw)
        {
            bool isBlank = string.IsNullOrWhiteSpace(req.Body?.ContentType);
            bool isInvalid = !IsValidContentType(req.Body!.ContentType);
            errorCode = isBlank ? TranslateRequestErrors.ContentTypeCannotBeBlankReqBodyRawOrFile : 
                        isInvalid ? TranslateRequestErrors.InvalidContentTypeRawOrFile :
                        null;
            return errorCode == null;
        }
        else if (bodyMode == PororocaRequestBodyMode.FormData)
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

    private static bool IsValidContentType(string? contentType) =>
        !string.IsNullOrWhiteSpace(contentType) && MimeTypesDetector.AllMimeTypes.Contains(contentType);

    private static bool CheckReqBodyFileExists(PororocaRequest req, Func<string, bool> fileExistsVerifier, out string? errorCode)
    {
        PororocaRequestBodyMode? bodyMode = req.Body?.Mode;
        
        if (bodyMode == PororocaRequestBodyMode.File)
        {
            bool fileFound = fileExistsVerifier.Invoke(req.Body!.FileSrcPath!);
            errorCode = fileFound ? null : TranslateRequestErrors.ReqBodyFileNotFound;
            return errorCode == null;
        }
        else if (bodyMode == PororocaRequestBodyMode.FormData)
        {
            bool anyNotFound = req.Body!.FormDataValues!
                                         .Any(fd => fd.Enabled
                                                 && fd.Type == PororocaRequestFormDataParamType.File
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

    private static bool CheckClientCertificateFilesExist(IPororocaVariableResolver variableResolver, Func<string, bool> fileExistsVerifier, PororocaRequest req, out string? errorCode)
    {
        if (req.CustomAuth?.Mode != PororocaRequestAuthMode.ClientCertificate || req.CustomAuth?.ClientCertificate == null)
        {
            errorCode = null;
            return true;
        }
        else
        {
            string resolvedCertificateFilePath = variableResolver.ReplaceTemplates(req.CustomAuth!.ClientCertificate!.CertificateFilePath);
            string resolvedPrivateKeyFilePath = variableResolver.ReplaceTemplates(req.CustomAuth!.ClientCertificate!.PrivateKeyFilePath);
            string resolvedFilePassword = variableResolver.ReplaceTemplates(req.CustomAuth!.ClientCertificate!.FilePassword);

            bool certFileExists = fileExistsVerifier.Invoke(resolvedCertificateFilePath);
            bool prvKeyFileSpecified = !string.IsNullOrWhiteSpace(resolvedPrivateKeyFilePath);
            bool prvKeyFileExists = fileExistsVerifier.Invoke(resolvedPrivateKeyFilePath);
            bool filePasswordIsBlank = string.IsNullOrWhiteSpace(resolvedFilePassword);

            if (!certFileExists)
            {
                errorCode = TranslateRequestErrors.ClientCertificateFileNotFound;
                return false;
            }
            else if (req.CustomAuth.ClientCertificate.Type == PororocaRequestAuthClientCertificateType.Pem && prvKeyFileSpecified && !prvKeyFileExists)
            {
                errorCode = TranslateRequestErrors.ClientCertificatePrivateKeyFileNotFound;
                return false;
            }
            else if (req.CustomAuth.ClientCertificate.Type == PororocaRequestAuthClientCertificateType.Pkcs12 && filePasswordIsBlank)
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

    #endregion

    #region TRANSLATE REQUEST

    public static bool TryTranslateRequest(IPororocaVariableResolver variableResolver, PororocaRequest req, out HttpRequestMessage? reqMsg, out string? errorCode) =>
        TryTranslateRequest(AvailablePororocaRequestSelectionOptions.IsHttpVersionAvailableInOS, variableResolver, req, out reqMsg, out errorCode);

    internal static bool TryTranslateRequest(HttpVersionAvailableInOSVerifier httpVersionOSVerifier, IPororocaVariableResolver variableResolver, PororocaRequest req, out HttpRequestMessage? reqMsg, out string? errorCode)
    {
        if (!TryResolveRequestUri(variableResolver, req.Url, out Uri? uri, out errorCode)
         || !httpVersionOSVerifier(req.HttpVersion, out errorCode))
        {
            reqMsg = null;
            return false;
        }
        else
        {
            try
            {
                HttpMethod method = new(req.HttpMethod);
                IDictionary<string, string> resolvedContentHeaders = ResolveContentHeaders(variableResolver, req);
                reqMsg = new(method, uri)
                {
                    Version = ResolveHttpVersion(req.HttpVersion),
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                    Content = ResolveRequestContent(variableResolver, req.Body, resolvedContentHeaders)
                };

                IDictionary<string, string> resolvedNonContentHeaders = ResolveNonContentHeaders(variableResolver, req);
                foreach (KeyValuePair<string, string> header in resolvedNonContentHeaders)
                {
                    reqMsg.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                IncludeClientCertificateIfSet(variableResolver, req, reqMsg);

                return true;
            }
            catch
            {
                reqMsg = null;
                errorCode = TranslateRequestErrors.UnknownRequestTranslationError;
                return false;
            }
        }
    }

    #endregion

    #region REQUEST URL

    private static bool TryResolveRequestUri(IPororocaVariableResolver variableResolver, string rawUrl, out Uri? uri, out string? errorCode)
    {
        string? resolvedRawUri = variableResolver.ReplaceTemplates(rawUrl);
        bool success = Uri.TryCreate(resolvedRawUri, UriKind.Absolute, out uri);
        errorCode = success ? null : TranslateRequestErrors.InvalidUrl;
        return success;
    }

    #endregion

    #region HTTP VERSION

    internal static Version ResolveHttpVersion(decimal httpVersion) =>
        new((int) httpVersion, (int) (httpVersion * 10) % 10);

    #endregion

    #region HTTP HEADERS

    internal static bool IsContentHeader(string headerName) =>
        headerName == "Allow"
     || headerName == "Content-Disposition"
     || headerName == "Content-Encoding"
     || headerName == "Content-Language"
     || headerName == "Content-Length"
     || headerName == "Content-Location"
     || headerName == "Content-MD5"
     || headerName == "Content-Range"
     || headerName == "Content-Type"
     || headerName == "Expires"
     || headerName == "Last-Modified";

    internal static IDictionary<string, string> ResolveContentHeaders(IPororocaVariableResolver variableResolver, PororocaRequest req) =>
        ResolveHeaders(variableResolver, req, IsContentHeader);
    
    internal static IDictionary<string, string> ResolveNonContentHeaders(IPororocaVariableResolver variableResolver, PororocaRequest req)
    {
        IDictionary<string, string> nonContentHeaders = ResolveHeaders(variableResolver, req, headerName => !IsContentHeader(headerName));
        
        string? customAuthHeaderValue = ResolveCustomAuthHeaderValue(variableResolver, req.CustomAuth);
        if (customAuthHeaderValue != null)
        {
            // Custom auth overrides declared authorization header
            nonContentHeaders["Authorization"] = customAuthHeaderValue;
        }

        return nonContentHeaders;
    }

    private static IDictionary<string, string> ResolveHeaders(IPororocaVariableResolver variableResolver, PororocaRequest req, Func<string, bool> headerNameCriteria) =>
        req.Headers == null ?
        new() :
        req.Headers
           .Where(h => h.Enabled)
           .Select(h => new KeyValuePair<string, string>(
               variableResolver.ReplaceTemplates(h.Key),
               variableResolver.ReplaceTemplates(h.Value)
           ))
           .Where(h => h.Key != "Content-Type" && headerNameCriteria(h.Key)) // Content-Type header is set by the content, later
           .DistinctBy(h => h.Key) // Avoid duplicated headers
           .ToDictionary(h => h.Key, h => h.Value);

    private static string? ResolveCustomAuthHeaderValue(IPororocaVariableResolver variableResolver, PororocaRequestAuth? customAuth)
    {
        string? MakeBasicAuthToken()
        {
            string? resolvedLogin = variableResolver.ReplaceTemplates(customAuth.BasicAuthLogin!);
            string? resolvedPassword = variableResolver.ReplaceTemplates(customAuth.BasicAuthPassword!);
            string basicAuthTkn = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resolvedLogin}:{resolvedPassword}"));
            return basicAuthTkn;
        }
        
        string? MakeBearerAuthToken()
        {
            string? resolvedBearerToken = variableResolver.ReplaceTemplates(customAuth.BearerToken!);
            return resolvedBearerToken;
        }

        return customAuth?.Mode switch
        {
            PororocaRequestAuthMode.Bearer => $"Bearer {MakeBearerAuthToken()}",
            PororocaRequestAuthMode.Basic => $"Basic {MakeBasicAuthToken()}",
            _ => null
        };
    }

    private static void IncludeClientCertificateIfSet(IPororocaVariableResolver variableResolver, PororocaRequest req, HttpRequestMessage reqMsg)
    {
        if (req.CustomAuth?.Mode == PororocaRequestAuthMode.ClientCertificate
         && req.CustomAuth?.ClientCertificate != null)
        {
            var clientCert = req.CustomAuth?.ClientCertificate!;

            string resolvedPrivateKeyFilePath = variableResolver.ReplaceTemplates(clientCert.PrivateKeyFilePath);            
            string resolvedFilePassword = variableResolver.ReplaceTemplates(clientCert.FilePassword);

            string? nulledPrivateKeyFilePath = string.IsNullOrWhiteSpace(resolvedPrivateKeyFilePath) ?
                                               null : resolvedPrivateKeyFilePath;
            string? nulledFilePassword = string.IsNullOrWhiteSpace(resolvedFilePassword) ?
                                         null : resolvedFilePassword;

            PororocaRequestAuthClientCertificate resolvedClientCert = new(
                type: clientCert.Type,
                certificateFilePath: variableResolver.ReplaceTemplates(clientCert.CertificateFilePath),
                privateKeyFilePath: nulledPrivateKeyFilePath,
                filePassword: nulledFilePassword
            );

            reqMsg.Options.TryAdd(ClientCertificateOptionsKey, resolvedClientCert);
        }
    }

    #endregion

    #region HTTP BODY
    
    internal static IDictionary<string, string> ResolveFormUrlEncodedKeyValues(IPororocaVariableResolver variableResolver, PororocaRequestBody reqBody) =>
        reqBody.UrlEncodedValues!
               .Where(x => x.Enabled)
               .Select(x => new KeyValuePair<string, string>(
                   variableResolver.ReplaceTemplates(x.Key),
                   variableResolver.ReplaceTemplates(x.Value)))
               .DistinctBy(x => x.Key)
               .ToDictionary(x => x.Key, x => x.Value);

    internal static HttpContent? ResolveRequestContent(IPororocaVariableResolver variableResolver, PororocaRequestBody? reqBody, IDictionary<string, string> resolvedContentHeaders)
    {
        StringContent MakeRawContent()
        {
            // TODO: Fix bug that charset cannot be passed in contentType below
            string resolvedRawContent = variableResolver.ReplaceTemplates(reqBody.RawContent!);
            return new(resolvedRawContent, Encoding.UTF8, reqBody.ContentType);
        }

        FormUrlEncodedContent MakeFormUrlEncodedContent()
        {
            IDictionary<string, string> resolvedFormValues = ResolveFormUrlEncodedKeyValues(variableResolver, reqBody);
            return new(resolvedFormValues);
        }

        StreamContent MakeFileContent(string fileSrcPath, string? contentType)
        {
            string resolvedFileSrcPath = variableResolver.ReplaceTemplates(fileSrcPath);
            StreamContent content = new(File.OpenRead(resolvedFileSrcPath));
            content.Headers.ContentType = new(contentType ?? MimeTypesDetector.DefaultMimeTypeForBinary);
            return content;
        }

        MultipartFormDataContent MakeFormDataContent()
        {
            MultipartFormDataContent formDataContent = new();
            var resolvedFormDataParams = reqBody!.FormDataValues!.Where(x => x.Enabled);
            foreach (PororocaRequestFormDataParam param in resolvedFormDataParams)
            {
                string resolvedKey = variableResolver.ReplaceTemplates(param.Key);
                if (param.Type == PororocaRequestFormDataParamType.Text)
                {
                    string resolvedTextValue = variableResolver.ReplaceTemplates(param.TextValue!);
                    formDataContent.Add(content: new StringContent(resolvedTextValue, Encoding.UTF8, param.ContentType ?? MimeTypesDetector.DefaultMimeTypeForText),
                                        name: resolvedKey);
                }
                else if (param.Type == PororocaRequestFormDataParamType.File)
                {
                    string resolvedFileSrcPath = variableResolver.ReplaceTemplates(param.FileSrcPath!);
                    string fileName = new FileInfo(param.FileSrcPath!).Name;
                    formDataContent.Add(content: MakeFileContent(param.FileSrcPath!, param.ContentType),
                                        name: resolvedKey,
                                        fileName: fileName);
                }
            }

            return formDataContent;
        }

        StringContent MakeGraphQlContent()
        {
            string? variables = reqBody!.GraphQlValues!.Variables;
            dynamic? variablesJsonObj = null;
            if (!string.IsNullOrWhiteSpace(variables))
            {
                variables = variableResolver.ReplaceTemplates(variables);
                try
                {
                    variablesJsonObj = JsonSerializer.Deserialize<dynamic?>(variables, ExporterImporterJsonOptions);
                }
                catch
                {                    
                }
            }
            dynamic reqObj = new
            {
                reqBody!.GraphQlValues!.Query,
                Variables = variablesJsonObj
            };
            string json = JsonSerializer.Serialize(reqObj, MinifyingOptions);

            return new(json, Encoding.UTF8, MimeTypesDetector.DefaultMimeTypeForJson);
        }

        HttpContent? content = reqBody?.Mode switch
        {
            PororocaRequestBodyMode.Raw => MakeRawContent(),
            PororocaRequestBodyMode.File => MakeFileContent(reqBody.FileSrcPath!, reqBody.ContentType),
            PororocaRequestBodyMode.UrlEncoded => MakeFormUrlEncodedContent(),
            PororocaRequestBodyMode.FormData => MakeFormDataContent(),
            PororocaRequestBodyMode.GraphQl => MakeGraphQlContent(),
            _ => null
        };

        if (content != null)
        {
            foreach (KeyValuePair<string, string> header in resolvedContentHeaders)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return content;
    }

    #endregion

}