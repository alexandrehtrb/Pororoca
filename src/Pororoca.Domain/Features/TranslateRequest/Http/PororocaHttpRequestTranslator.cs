using System.Text;
using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;

namespace Pororoca.Domain.Features.TranslateRequest.Http;

public static class PororocaHttpRequestTranslator
{
    public const string AuthOptionsKey = nameof(AuthOptionsKey);

    #region TRANSLATE REQUEST

    public static bool TryTranslateRequest(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaHttpRequest req, out HttpRequestMessage? reqMsg, out string? errorCode)
    {
        if (!TryResolveRequestUri(effectiveVars, req.Url, out var uri, out errorCode)
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
                var resolvedContentHeaders = ResolveContentHeaders(effectiveVars, req.Headers);
                reqMsg = new(method, uri)
                {
                    Version = ResolveHttpVersion(req.HttpVersion),
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                    Content = ResolveRequestContent(effectiveVars, req.Body, resolvedContentHeaders)
                };

                var resolvedNonContentHeaders = ResolveNonContentHeaders(effectiveVars, collectionScopedAuth, req.CustomAuth, req.Headers);
                foreach (var header in resolvedNonContentHeaders)
                {
                    reqMsg.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                IncludeAuthInOptions(effectiveVars, collectionScopedAuth, req, reqMsg);

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

    #region AUTH

    private static void IncludeAuthInOptions(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaHttpRequest req, HttpRequestMessage reqMsg)
    {
        var resolvedAuth = ResolveRequestAuth(effectiveVars, collectionScopedAuth, req.CustomAuth);

        if (resolvedAuth is not null)
        {
            reqMsg.Options.TryAdd(AuthOptionsKey, resolvedAuth);
        }
    }

    #endregion

    #region HTTP BODY

    internal static IDictionary<string, string> ResolveFormUrlEncodedKeyValues(IEnumerable<PororocaVariable> effectiveVars, PororocaHttpRequestBody reqBody) =>
        IPororocaVariableResolver.ResolveKeyValueParams(reqBody.UrlEncodedValues!, effectiveVars);

    internal static HttpContent? ResolveRequestContent(IEnumerable<PororocaVariable> effectiveVars, PororocaHttpRequestBody? reqBody, IDictionary<string, string> resolvedContentHeaders)
    {
        StringContent MakeRawContent()
        {
            // TODO: Fix bug that charset cannot be passed in contentType below
            string resolvedRawContent = IPororocaVariableResolver.ReplaceTemplates(reqBody.RawContent!, effectiveVars);
            return new(resolvedRawContent, Encoding.UTF8, reqBody.ContentType!);
        }

        FormUrlEncodedContent MakeFormUrlEncodedContent()
        {
            var resolvedFormValues = ResolveFormUrlEncodedKeyValues(effectiveVars, reqBody);
            return new(resolvedFormValues);
        }

        StreamContent MakeFileContent(string fileSrcPath, string? contentType)
        {
            const int fileStreamBufferSize = 4096;
            string resolvedFileSrcPath = IPororocaVariableResolver.ReplaceTemplates(fileSrcPath, effectiveVars);
            // DO NOT USE "USING" FOR FILESTREAM HERE --> it will be disposed later, by the PororocaRequester
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
                string resolvedKey = IPororocaVariableResolver.ReplaceTemplates(param.Key, effectiveVars);
                if (param.Type == PororocaHttpRequestFormDataParamType.Text)
                {
                    string resolvedTextValue = IPororocaVariableResolver.ReplaceTemplates(param.TextValue!, effectiveVars);
                    formDataContent.Add(content: new StringContent(resolvedTextValue, Encoding.UTF8, param.ContentType ?? MimeTypesDetector.DefaultMimeTypeForText),
                                        name: resolvedKey);
                }
                else if (param.Type == PororocaHttpRequestFormDataParamType.File)
                {
                    string resolvedFileSrcPath = IPororocaVariableResolver.ReplaceTemplates(param.FileSrcPath!, effectiveVars);
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
                variables = IPororocaVariableResolver.ReplaceTemplates(variables, effectiveVars);
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