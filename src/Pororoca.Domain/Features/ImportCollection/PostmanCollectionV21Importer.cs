using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Postman;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;

namespace Pororoca.Domain.Features.ImportCollection;

public static class PostmanCollectionV21Importer
{
    public static bool TryImportPostmanCollection(string postmanCollectionFileContent, out PororocaCollection? pororocaCollection)
    {
        try
        {
            var postmanCollection = JsonSerializer.Deserialize<PostmanCollectionV21>(postmanCollectionFileContent, options: ExporterImporterJsonOptions);
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
            PororocaCollection myCol = new(Guid.NewGuid(), postmanCollection.Info.Name, DateTimeOffset.Now);
            var collectionScopedAuth = ConvertToPororocaAuth(postmanCollection.Auth);
            foreach (var item in postmanCollection.Items)
            {
                var convertedItem = ConvertToPororocaCollectionItem(item, collectionScopedAuth);
                if (convertedItem is PororocaCollectionFolder folder)
                    myCol.AddFolder(folder);
                else if (convertedItem is PororocaHttpRequest request)
                    myCol.AddRequest(request);
            }
            if (postmanCollection.Variable != null)
            {
                foreach (var v in postmanCollection.Variable)
                {
                    myCol.AddVariable(ConvertToPororocaVariable(v));
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
        PororocaRequestAuth myAuth;
        switch (auth?.Type)
        {
            case PostmanAuthType.basic:
                myAuth = new(PororocaRequestAuthMode.Basic);
                myAuth.SetBasicAuth(auth.Basic?.FirstOrDefault(p => p.Key == "username")?.Value ?? string.Empty,
                                    auth.Basic?.FirstOrDefault(p => p.Key == "password")?.Value ?? string.Empty);
                break;
            case PostmanAuthType.bearer:
                myAuth = new(PororocaRequestAuthMode.Bearer);
                myAuth.SetBearerAuth(auth.Bearer?.FirstOrDefault(p => p.Key == "token")?.Value ?? string.Empty);
                break;
            case PostmanAuthType.noauth:
            default:
                return null;
        }
        return myAuth;
    }

    private static PororocaVariable ConvertToPororocaVariable(PostmanVariable v) =>
        new(IsEnabledInPostman(v.Disabled), v.Key, v.Value, false);

    private static PororocaCollectionItem ConvertToPororocaCollectionItem(PostmanCollectionItem item, PororocaRequestAuth? collectionScopedAuth)
    {
        if (item.Request != null)
        {
            return ConvertToPororocaHttpRequest(item.Name, item.Request, collectionScopedAuth);
        }
        else
        {
            PororocaCollectionFolder folder = new(item.Name);
            if (item.Items != null)
            {
                foreach (var subItem in item.Items)
                {
                    var convertedSubItem = ConvertToPororocaCollectionItem(subItem, collectionScopedAuth);
                    if (convertedSubItem is PororocaCollectionFolder subFolder)
                        folder.AddFolder(subFolder);
                    else if (convertedSubItem is PororocaHttpRequest subRequest)
                        folder.AddRequest(subRequest);
                }
            }
            return folder;
        }
    }

    internal static PororocaHttpRequest ConvertToPororocaHttpRequest(string name, PostmanRequest request, PororocaRequestAuth? collectionScopedAuth)
    {
        const decimal defaultHttpVersion = 1.1m;
        PororocaHttpRequest myReq = new();
        myReq.Update(
            name: name,
            httpVersion: defaultHttpVersion,
            httpMethod: request.Method,
            url: request.Url.Raw,
            // When Postman req auth is null, the request uses collection scoped auth
            customAuth: request.Auth != null ? ConvertToPororocaAuth(request.Auth) : (PororocaRequestAuth?)collectionScopedAuth?.Clone(),
            headers: ConvertToPororocaHeaders(request.Header),
            body: ConvertToPororocaHttpRequestBody(request.Body));
        return myReq;
    }

    internal static List<PororocaKeyValueParam> ConvertToPororocaHeaders(PostmanVariable[]? headersParams) =>
        ConvertToPororocaKeyValueParams(headersParams);

    internal static PororocaHttpRequestBody? ConvertToPororocaHttpRequestBody(PostmanRequestBody? body)
    {
        static string FindFileBodyMimeType(PostmanRequestBody? body)
        {
            string fileSrcPath = body?.File?.Src ?? string.Empty;
            TryFindMimeTypeForFile(fileSrcPath, out string? mimeType);
            return mimeType ?? DefaultMimeTypeForBinary;
        }

        PororocaHttpRequestBody myBody = new();
        switch (body?.Mode)
        {
            case PostmanRequestBodyMode.Raw:
                string rawContentType = FindContentTypeForPostmanRawBodyLanguage(body.Options?.Raw?.Language);
                myBody.SetRawContent(body.Raw ?? string.Empty, rawContentType);
                break;
            case PostmanRequestBodyMode.File:
                string fileContentType = FindFileBodyMimeType(body);
                myBody.SetFileContent(body.File?.Src ?? string.Empty, fileContentType);
                break;
            case PostmanRequestBodyMode.Urlencoded:
                myBody.SetUrlEncodedContent(ConvertToPororocaKeyValueParams(body.Urlencoded));
                break;
            case PostmanRequestBodyMode.Formdata:
                myBody.SetFormDataContent(ConvertToPororocaHttpFormDataValues(body.Formdata));
                break;
            case PostmanRequestBodyMode.Graphql:
                string? query = string.IsNullOrWhiteSpace(body.Graphql?.Query) ? null : body.Graphql.Query;
                string? variables = string.IsNullOrWhiteSpace(body.Graphql?.Variables) ? null : body.Graphql.Variables;
                myBody.SetGraphQlContent(query, variables);
                break;
            default:
                return null;
        }
        return myBody;
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

    private static List<PororocaKeyValueParam> ConvertToPororocaKeyValueParams(PostmanVariable[]? variables) =>
        (variables ?? Array.Empty<PostmanVariable>())
            .Select(v => new PororocaKeyValueParam(IsEnabledInPostman(v.Disabled), v.Key, v.Value))
            .ToList();

    private static List<PororocaHttpRequestFormDataParam> ConvertToPororocaHttpFormDataValues(PostmanRequestBodyFormDataParam[]? formDatas) =>
        (formDatas ?? Array.Empty<PostmanRequestBodyFormDataParam>())
            .Select(ConvertToPororocaHttpFormDataParam).ToList();

    private static PororocaHttpRequestFormDataParam ConvertToPororocaHttpFormDataParam(PostmanRequestBodyFormDataParam f)
    {
        PororocaHttpRequestFormDataParam p = new(IsEnabledInPostman(f.Disabled), f.Key);
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
                    fileSrcPath = je.Deserialize<string[]>()?.FirstOrDefault();
                }
                else
                {
                    fileSrcPath = je.Deserialize<string>();
                }
            }
            p.SetFileValue(fileSrcPath ?? string.Empty, f.ContentType ?? string.Empty);
        }
        else if (f.Type == PostmanRequestBodyFormDataParamType.Text)
        {
            p.SetTextValue(f.Value ?? string.Empty, f.ContentType ?? string.Empty);
        }
        return p;
    }

    private static bool IsEnabledInPostman(bool? postmanDisabledBool) =>
        postmanDisabledBool == null || postmanDisabledBool == false;
}