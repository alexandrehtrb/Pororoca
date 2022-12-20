using System.Text;
using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Domain.Features.TranslateRequest.Http;

public static class PororocaHttpRequestTranslator
{
    public const string ClientCertificateOptionsKey = nameof(ClientCertificateOptionsKey);

    #region TRANSLATE REQUEST

    public static bool TryTranslateRequest(IPororocaVariableResolver variableResolver, PororocaHttpRequest req, out HttpRequestMessage? reqMsg, out string? errorCode)
    {
        if (!TryResolveRequestUri(variableResolver, req.Url, out var uri, out errorCode)
         || !IsHttpVersionAvailableInOS(req.HttpVersion, out errorCode))
        {
            reqMsg = null;
            return false;
        }
        else
        {
            try
            {
                HttpMethod method = new(req.HttpMethod);
                var resolvedContentHeaders = ResolveContentHeaders(variableResolver, req.Headers);
                reqMsg = new(method, uri)
                {
                    Version = ResolveHttpVersion(req.HttpVersion),
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                    Content = ResolveRequestContent(variableResolver, req.Body, resolvedContentHeaders)
                };

                var resolvedNonContentHeaders = ResolveNonContentHeaders(variableResolver, req.Headers, req.CustomAuth);
                foreach (var header in resolvedNonContentHeaders)
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

    #region CLIENT CERTIFICATES

    private static void IncludeClientCertificateIfSet(IPororocaVariableResolver variableResolver, PororocaHttpRequest req, HttpRequestMessage reqMsg)
    {
        if (req.CustomAuth?.Mode == PororocaRequestAuthMode.ClientCertificate
         && req.CustomAuth?.ClientCertificate != null)
        {
            var resolvedClientCert = ResolveClientCertificate(variableResolver, req.CustomAuth!.ClientCertificate!);

            reqMsg.Options.TryAdd(ClientCertificateOptionsKey, resolvedClientCert);
        }
    }

    #endregion

    #region HTTP BODY

    internal static IDictionary<string, string> ResolveFormUrlEncodedKeyValues(IPororocaVariableResolver variableResolver, PororocaHttpRequestBody reqBody) =>
        variableResolver.ResolveKeyValueParams(reqBody.UrlEncodedValues!);

    internal static HttpContent? ResolveRequestContent(IPororocaVariableResolver variableResolver, PororocaHttpRequestBody? reqBody, IDictionary<string, string> resolvedContentHeaders)
    {
        StringContent MakeRawContent()
        {
            // TODO: Fix bug that charset cannot be passed in contentType below
            string resolvedRawContent = variableResolver.ReplaceTemplates(reqBody.RawContent!);
            return new(resolvedRawContent, Encoding.UTF8, reqBody.ContentType!);
        }

        FormUrlEncodedContent MakeFormUrlEncodedContent()
        {
            var resolvedFormValues = ResolveFormUrlEncodedKeyValues(variableResolver, reqBody);
            return new(resolvedFormValues);
        }

        StreamContent MakeFileContent(string fileSrcPath, string? contentType)
        {
            const int fileStreamBufferSize = 4096;
            string resolvedFileSrcPath = variableResolver.ReplaceTemplates(fileSrcPath);
            FileStream fs = new(resolvedFileSrcPath, FileMode.Open, FileAccess.Read, FileShare.Read, fileStreamBufferSize, useAsync: true);
            StreamContent content = new(fs);
            content.Headers.ContentType = new(contentType ?? MimeTypesDetector.DefaultMimeTypeForBinary);
            return content;
        }

        MultipartFormDataContent MakeFormDataContent()
        {
            MultipartFormDataContent formDataContent = new();
            var resolvedFormDataParams = reqBody!.FormDataValues!.Where(x => x.Enabled);
            foreach (var param in resolvedFormDataParams)
            {
                string resolvedKey = variableResolver.ReplaceTemplates(param.Key);
                if (param.Type == PororocaHttpRequestFormDataParamType.Text)
                {
                    string resolvedTextValue = variableResolver.ReplaceTemplates(param.TextValue!);
                    formDataContent.Add(content: new StringContent(resolvedTextValue, Encoding.UTF8, param.ContentType ?? MimeTypesDetector.DefaultMimeTypeForText),
                                        name: resolvedKey);
                }
                else if (param.Type == PororocaHttpRequestFormDataParamType.File)
                {
                    string resolvedFileSrcPath = variableResolver.ReplaceTemplates(param.FileSrcPath!);
                    string fileName = new FileInfo(param.FileSrcPath!).Name;
                    formDataContent.Add(content: MakeFileContent(resolvedFileSrcPath, param.ContentType),
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
            PororocaHttpRequestBodyMode.Raw => MakeRawContent(),
            PororocaHttpRequestBodyMode.File => MakeFileContent(reqBody.FileSrcPath!, reqBody.ContentType),
            PororocaHttpRequestBodyMode.UrlEncoded => MakeFormUrlEncodedContent(),
            PororocaHttpRequestBodyMode.FormData => MakeFormDataContent(),
            PororocaHttpRequestBodyMode.GraphQl => MakeGraphQlContent(),
            _ => null
        };

        if (content != null)
        {
            foreach (var header in resolvedContentHeaders)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return content;
    }

    #endregion

}