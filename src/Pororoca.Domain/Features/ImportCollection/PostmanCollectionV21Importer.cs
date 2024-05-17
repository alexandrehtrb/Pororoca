using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Postman;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;

namespace Pororoca.Domain.Features.ImportCollection;

public static class PostmanCollectionV21Importer
{
    public static bool TryImportPostmanCollection(string postmanCollectionFileContent, out PororocaCollection? pororocaCollection)
    {
        try
        {
            var postmanCollection = JsonSerializer.Deserialize(postmanCollectionFileContent, MainJsonCtx.PostmanCollectionV21);
            if (postmanCollection == null
             || postmanCollection.Info?.Name == null)
            {
                pororocaCollection = null;
                return false;
            }

            return TryConvertToPororocaCollection(postmanCollection, out pororocaCollection);
        }
        catch
        {
            pororocaCollection = null;
            return false;
        }
    }

    internal static bool TryConvertToPororocaCollection(PostmanCollectionV21 postmanCollection, out PororocaCollection? pororocaCollection)
    {
        try
        {
            // Always generating new id, in case user imports the same collection twice
            // This is to avoid overwriting when saving user collections
            var collectionScopedAuth = ConvertToPororocaAuth(postmanCollection.Auth);
            PororocaCollection myCol = new(Guid.NewGuid(), postmanCollection.Info.Name, DateTimeOffset.Now)
            {
                // if collection-scoped auth exists, then use it and inherit it in requests
                CollectionScopedAuth = collectionScopedAuth
            };
            foreach (var item in postmanCollection.Items)
            {
                object convertedItem = ConvertToPororocaCollectionItem(item);
                if (convertedItem is PororocaCollectionFolder folder)
                    myCol.Folders.Add(folder);
                else if (convertedItem is PororocaHttpRequest request)
                    myCol.Requests.Add(request);
            }
            if (postmanCollection.Variable != null)
            {
                foreach (var v in postmanCollection.Variable)
                {
                    myCol.Variables.Add(ConvertToPororocaVariable(v));
                }
            }

            pororocaCollection = myCol;
            return true;
        }
        catch
        {
            pororocaCollection = null;
            return false;
        }
    }

    internal static PororocaRequestAuth? ConvertToPororocaAuth(PostmanAuth? auth)
    {
        switch (auth?.Type)
        {
            case PostmanAuthType.basic:
                var (basicAuthLogin, basicAuthPwd) = auth.ReadBasicAuthValues();
                return PororocaRequestAuth.MakeBasicAuth(basicAuthLogin, basicAuthPwd);
            case PostmanAuthType.bearer:
                string bearerToken = auth.ReadBearerAuthValue();
                return PororocaRequestAuth.MakeBearerAuth(bearerToken);
            case PostmanAuthType.ntlm:
                var (login, pwd, dom, _) = auth.ReadNtlmAuthValues();
                return PororocaRequestAuth.MakeWindowsAuth(false, login, pwd, dom);
            case PostmanAuthType.noauth:
            default:
                return null;
        }
    }

    private static PororocaVariable ConvertToPororocaVariable(PostmanVariable v) =>
        new(IsEnabledInPostman(v.Disabled), v.Key, v.Value, false);

    private static object ConvertToPororocaCollectionItem(PostmanCollectionItem item)
    {
        if (item.Request != null)
        {
            return ConvertToPororocaHttpRequest(item.Name, item.Request);
        }
        else
        {
            PororocaCollectionFolder folder = new(item.Name);
            if (item.Items != null)
            {
                foreach (var subItem in item.Items)
                {
                    object convertedSubItem = ConvertToPororocaCollectionItem(subItem);
                    if (convertedSubItem is PororocaCollectionFolder subFolder)
                        folder.Folders.Add(subFolder);
                    else if (convertedSubItem is PororocaHttpRequest subRequest)
                        folder.Requests.Add(subRequest);
                }
            }
            return folder;
        }
    }

    internal static PororocaHttpRequest ConvertToPororocaHttpRequest(string name, PostmanRequest request)
    {
        const decimal defaultHttpVersion = 1.1m;
        var headers = ConvertToPororocaHeaders(request.Header);
        string? contentTypeHeaderValue = headers.FirstOrDefault(h => h.Key == "Content-Type")?.Value;

        return new(
            Name: name,
            HttpVersion: defaultHttpVersion,
            HttpMethod: request.Method,
            Url: ReadPostmanRequestUrl(request.Url),
            Headers: headers,
            Body: ConvertToPororocaHttpRequestBody(request.Body, contentTypeHeaderValue),
            // When Postman req auth is null, the request uses collection scoped auth
            CustomAuth: request.Auth != null ? ConvertToPororocaAuth(request.Auth) : PororocaRequestAuth.InheritedFromCollection,
            ResponseCaptures: null);
    }

    private static string ReadPostmanRequestUrl(object? postmanRequestUrl)
    {
        // TODO: Use custom JSON converter instead of handling JSON here
        if (postmanRequestUrl is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Object)
            {
                return je.Deserialize(MainJsonCtx.PostmanRequestUrl)?.Raw ?? string.Empty;
            }
            else
            {
                return je.Deserialize(MainJsonCtx.String) ?? string.Empty;
            }
        }
        else if (postmanRequestUrl is PostmanRequestUrl reqUrl)
        {
            return reqUrl.Raw;
        }
        else
        {
            return string.Empty;
        }
    }

    internal static List<PororocaKeyValueParam> ConvertToPororocaHeaders(PostmanVariable[]? headersParams) =>
        ConvertToPororocaKeyValueParams(headersParams);

    internal static PororocaHttpRequestBody? ConvertToPororocaHttpRequestBody(PostmanRequestBody? body, string? contentTypeHeaderValue)
    {
        static string FindFileBodyMimeType(PostmanRequestBody? body)
        {
            string fileSrcPath = body?.File?.Src ?? string.Empty;
            TryFindMimeTypeForFile(fileSrcPath, out string? mimeType);
            return mimeType ?? DefaultMimeTypeForBinary;
        }

        switch (body?.Mode)
        {
            case PostmanRequestBodyMode.Raw:
                string? contentTypeFromHeader = FindContentTypeForPostmanContentTypeHeader(contentTypeHeaderValue);
                string contentTypeFromRaw = FindContentTypeForPostmanRawBodyLanguage(body.Options?.Raw?.Language);
                string contentType = contentTypeFromHeader ?? contentTypeFromRaw;
                return MakeRawContent(body.Raw ?? string.Empty, contentTypeFromHeader ?? contentType);
            case PostmanRequestBodyMode.File:
                string fileContentType = FindFileBodyMimeType(body);
                return MakeFileContent(body.File?.Src ?? string.Empty, fileContentType);
            case PostmanRequestBodyMode.Urlencoded:
                return MakeUrlEncodedContent(ConvertToPororocaKeyValueParams(body.Urlencoded));
            case PostmanRequestBodyMode.Formdata:
                return MakeFormDataContent(ConvertToPororocaHttpFormDataValues(body.Formdata));
            case PostmanRequestBodyMode.Graphql:
                string? query = string.IsNullOrWhiteSpace(body.Graphql?.Query) ? null : body.Graphql.Query;
                string? variables = string.IsNullOrWhiteSpace(body.Graphql?.Variables) ? null : body.Graphql.Variables;
                return MakeGraphQlContent(query, variables);
            default:
                return null;
        }
    }

    private static string FindContentTypeForPostmanRawBodyLanguage(string? postmanReqRawBodyLanguage) =>
        postmanReqRawBodyLanguage switch
        {
            "json" => DefaultMimeTypeForJson,
            "javascript" => DefaultMimeTypeForJavascript,
            "html" => DefaultMimeTypeForHtml,
            "xml" => DefaultMimeTypeForXml,
            "text" => DefaultMimeTypeForText,
            _ => DefaultMimeTypeForText
        };

    private static string? FindContentTypeForPostmanContentTypeHeader(string? contentTypeHeaderValue)
    {
        if (string.IsNullOrWhiteSpace(contentTypeHeaderValue))
            return null;
        else if (contentTypeHeaderValue.Contains(DefaultMimeTypeForJson))
            return DefaultMimeTypeForJson;
        else if (contentTypeHeaderValue.Contains(DefaultMimeTypeForJavascript))
            return DefaultMimeTypeForJavascript;
        else if (contentTypeHeaderValue.Contains(DefaultMimeTypeForHtml))
            return DefaultMimeTypeForHtml;
        else if (contentTypeHeaderValue.Contains(DefaultMimeTypeForXml))
            return DefaultMimeTypeForXml;
        else if (contentTypeHeaderValue.Contains(DefaultMimeTypeForText))
            return DefaultMimeTypeForText;
        else
            return null;
    }

    private static List<PororocaKeyValueParam> ConvertToPororocaKeyValueParams(PostmanVariable[]? variables) =>
        (variables ?? Array.Empty<PostmanVariable>())
            .Select(v => new PororocaKeyValueParam(IsEnabledInPostman(v.Disabled), v.Key, v.Value))
            .ToList();

    private static List<PororocaHttpRequestFormDataParam> ConvertToPororocaHttpFormDataValues(PostmanRequestBodyFormDataParam[]? formDatas) =>
        (formDatas ?? Array.Empty<PostmanRequestBodyFormDataParam>())
            .Select(ConvertToPororocaHttpFormDataParam).ToList();

    private static PororocaHttpRequestFormDataParam ConvertToPororocaHttpFormDataParam(PostmanRequestBodyFormDataParam f)
    {
        PororocaHttpRequestFormDataParam p = new();
        if (f.Type == PostmanRequestBodyFormDataParamType.File)
        {
            string? fileSrcPath = null;
            // TODO: Use custom JSON converter instead of handling JSON here
            if (f.Src is string srcStr)
            {
                fileSrcPath = srcStr;
            }
            else if (f.Src is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Array)
                {
                    fileSrcPath = je.Deserialize(MainJsonCtx.StringArray)?.FirstOrDefault();
                }
                else
                {
                    fileSrcPath = je.Deserialize(MainJsonCtx.String);
                }
            }
            return PororocaHttpRequestFormDataParam.MakeFileParam(IsEnabledInPostman(f.Disabled), f.Key, fileSrcPath ?? string.Empty, f.ContentType ?? string.Empty);
        }
        else if (f.Type == PostmanRequestBodyFormDataParamType.Text)
        {
            return PororocaHttpRequestFormDataParam.MakeTextParam(IsEnabledInPostman(f.Disabled), f.Key, f.Value ?? string.Empty, f.ContentType ?? string.Empty);
        }
        return p;
    }

    private static bool IsEnabledInPostman(bool? postmanDisabledBool) =>
        postmanDisabledBool == null || postmanDisabledBool == false;
}