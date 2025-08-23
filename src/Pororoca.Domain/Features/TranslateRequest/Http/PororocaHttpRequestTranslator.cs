using System.Text;
using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.VariableResolution;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;
using static Pororoca.Domain.Features.TranslateRequest.Common.PororocaRequestCommonTranslator;

namespace Pororoca.Domain.Features.TranslateRequest.Http;

public static class PororocaHttpRequestTranslator
{
    public const string AuthOptionsKey = nameof(AuthOptionsKey);

    private static readonly JsonDocumentOptions GraphQlJsonOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip
    };

    #region TRANSLATE REQUEST

    public static bool TryTranslateRequest(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, List<PororocaKeyValueParam>? collectionScopedReqHeaders, PororocaHttpRequest req, out PororocaHttpRequest? resolvedReq, out HttpRequestMessage? reqMsg, out string? errorCode)
    {
        if (!TryResolveAndMakeRequestUri(effectiveVars, req.Url, out var uri, out errorCode)
         || !IsHttpVersionAvailableInOS(req.HttpVersion, out errorCode))
        {
            reqMsg = null;
            resolvedReq = null;
            return false;
        }
        else
        {
            try
            {
                resolvedReq = ResolveRequest(effectiveVars, collectionScopedAuth, collectionScopedReqHeaders, req);
                HttpMethod method = new(resolvedReq.HttpMethod);
                var resolvedContentHeaders = MakeContentHeaders(resolvedReq.Headers);
                reqMsg = new(method, uri)
                {
                    Version = MakeHttpVersion(resolvedReq.HttpVersion),
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                    Content = MakeRequestContent(resolvedReq.Body, resolvedContentHeaders)
                };

                var resolvedNonContentHeaders = MakeNonContentHeaders(resolvedReq.CustomAuth, resolvedReq.Headers);
                if (resolvedNonContentHeaders != null)
                {
                    foreach (var header in resolvedNonContentHeaders)
                    {
                        reqMsg.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                IncludeAuthInOptions(resolvedReq.CustomAuth, reqMsg);

                return true;
            }
            catch
            {
                reqMsg = null;
                resolvedReq = null;
                errorCode = TranslateRequestErrors.UnknownRequestTranslationError;
                return false;
            }
        }
    }

    #endregion

    #region RESOLUTION / REPLACE VARIABLE TEMPLATES

    internal static PororocaHttpRequest ResolveRequest(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, List<PororocaKeyValueParam>? collectionScopedReqHeaders, PororocaHttpRequest req) => new()
    {
        HttpVersion = req.HttpVersion,
        HttpMethod = req.HttpMethod,
        Url = IPororocaVariableResolver.ReplaceTemplates(req.Url, effectiveVars),
        Headers = ResolveRequestHeaders(effectiveVars, collectionScopedReqHeaders, req.Headers),
        Body = ResolveRequestBody(effectiveVars, req.Body),
        CustomAuth = ResolveRequestAuth(effectiveVars, collectionScopedAuth, req.CustomAuth),
        ResponseCaptures = req.ResponseCaptures
    };

    internal static PororocaHttpRequestBody? ResolveRequestBody(IEnumerable<PororocaVariable> effectiveVars, PororocaHttpRequestBody? input)
    {
        string ReplaceTemplates(string? s) => IPororocaVariableResolver.ReplaceTemplates(s, effectiveVars);

        if (input is null) return null;

        return input.Mode switch
        {
            PororocaHttpRequestBodyMode.Raw => MakeRawContent(ReplaceTemplates(input.RawContent!), input.ContentType!),
            PororocaHttpRequestBodyMode.File => MakeFileContent(ReplaceTemplates(input.FileSrcPath!), input.ContentType!),
            PororocaHttpRequestBodyMode.UrlEncoded => MakeUrlEncodedContent(ResolveKVParams(effectiveVars, input.UrlEncodedValues!)!),
            PororocaHttpRequestBodyMode.FormData => MakeFormDataContent(
                                input.FormDataValues!
                                    .Where(x => x.Enabled)
                                    .Select(x => new PororocaHttpRequestFormDataParam(true, x.Type, ReplaceTemplates(x.Key), ReplaceTemplates(x.TextValue), x.ContentType, ReplaceTemplates(x.FileSrcPath)))),
            PororocaHttpRequestBodyMode.GraphQl => MakeGraphQlContent(input.GraphQlValues!.Query, ReplaceTemplates(input.GraphQlValues!.Variables)),
            _ => null,
        };
    }

    #endregion

    #region AUTH

    private static void IncludeAuthInOptions(PororocaRequestAuth? resolvedAuth, HttpRequestMessage reqMsg)
    {
        if (resolvedAuth is not null)
        {
            reqMsg.Options.TryAdd(AuthOptionsKey, resolvedAuth);
        }
    }

    #endregion

    #region HTTP BODY

    internal static HttpContent? MakeRequestContent(PororocaHttpRequestBody? resolvedBody, Dictionary<string, string>? resolvedContentHeaders)
    {
        // TODO: Fix bug that charset cannot be passed in contentType below
        StringContent MakeRawContent() =>
            new(resolvedBody.RawContent!, Encoding.UTF8, resolvedBody.ContentType!);

        FormUrlEncodedContent MakeFormUrlEncodedContent() =>
            new(resolvedBody.UrlEncodedValues!.ToDictionary(p => p.Key, p => p.Value!));

        StreamContent MakeFileContent(string resolvedFileSrcPath, string? contentType)
        {
            // DO NOT USE "USING" FOR FILESTREAM HERE --> it will be disposed later, by the PororocaRequester
            FileStream fs = File.OpenRead(resolvedFileSrcPath);
            StreamContent content = new(fs);
            content.Headers.ContentType = new(contentType ?? MimeTypesDetector.DefaultMimeTypeForBinary);
            return content;
        }

        MultipartFormDataContent MakeFormDataContent()
        {
            MultipartFormDataContent formDataContent = new();
            foreach (var param in resolvedBody!.FormDataValues!)
            {
                if (param.Type == PororocaHttpRequestFormDataParamType.Text)
                {
                    formDataContent.Add(content: new StringContent(param.TextValue!, Encoding.UTF8, param.ContentType ?? MimeTypesDetector.DefaultMimeTypeForText),
                                        name: param.Key);
                }
                else if (param.Type == PororocaHttpRequestFormDataParamType.File)
                {
                    string fileName = new FileInfo(param.FileSrcPath!).Name;
                    formDataContent.Add(content: MakeFileContent(param.FileSrcPath!, param.ContentType),
                                        name: param.Key,
                                        fileName: fileName);
                }
            }
            // TODO: Should Content headers be added in parent FormDataContent, or in each child?
            return formDataContent;
        }

        StringContent MakeGraphQlContent()
        {
            string? variables = resolvedBody!.GraphQlValues!.Variables;
            JsonDocument? variablesJsonDoc = null;
            if (!string.IsNullOrWhiteSpace(variables))
            {
                try
                {
                    // this deserailize-serialize is solely
                    // to allow comments in GraphQL variables textbox
                    variablesJsonDoc = JsonDocument.Parse(variables, GraphQlJsonOptions);
                }
                catch
                {
                    variablesJsonDoc = JsonDocument.Parse("{}");
                }
            }
            string variablesJsonStr =
                variablesJsonDoc is null ?
                "null" :
                JsonSerializer.Serialize(variablesJsonDoc.RootElement, MinifyingJsonCtx.JsonElement);
            string json = "{\"query\":\"" + resolvedBody!.GraphQlValues!.Query! + "\",\"variables\":" + variablesJsonStr + "}";

            return new(json, Encoding.UTF8, MimeTypesDetector.DefaultMimeTypeForJson);
        }

        HttpContent? content = resolvedBody?.Mode switch
        {
            PororocaHttpRequestBodyMode.Raw => MakeRawContent(),
            PororocaHttpRequestBodyMode.File => MakeFileContent(resolvedBody.FileSrcPath!, resolvedBody.ContentType),
            PororocaHttpRequestBodyMode.UrlEncoded => MakeFormUrlEncodedContent(),
            PororocaHttpRequestBodyMode.FormData => MakeFormDataContent(),
            PororocaHttpRequestBodyMode.GraphQl => MakeGraphQlContent(),
            _ => null
        };

        if (content != null && resolvedContentHeaders != null)
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