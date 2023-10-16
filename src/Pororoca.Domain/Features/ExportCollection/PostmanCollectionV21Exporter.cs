using System.Text.Json;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Postman;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ExportCollection;

public static class PostmanCollectionV21Exporter
{
    public static string ExportAsPostmanCollectionV21(PororocaCollection col, bool shouldHideSecrets)
    {
        var postmanCollection = ConvertToPostmanCollectionV21(col, shouldHideSecrets);
        return JsonSerializer.Serialize(postmanCollection!, options: ExporterImporterJsonOptions);
    }

    internal static PostmanCollectionV21 ConvertToPostmanCollectionV21(PororocaCollection col, bool shouldHideSecrets) =>
        new()
        {
            Info = new()
            {
                Id = col.Id,
                Name = col.Name,
                Schema = PostmanCollectionV21.SchemaUrl
            },
            Items = ConvertToPostmanItems(col.Folders, col.HttpRequests),
            Variable = col.Variables
                          .Select(v => new PostmanVariable()
                          {
                              Key = v.Key,
                              Value = shouldHideSecrets && v.IsSecret ? string.Empty : v.Value,
                              Disabled = v.Enabled ? null : true
                          })
                          .ToArray(),
            Auth = ConvertToPostmanAuth(col.CollectionScopedAuth)
        };

    internal static PostmanAuth ConvertToPostmanAuth(PororocaRequestAuth? auth)
    {
        PostmanAuth postmanAuth = new();
        switch (auth?.Mode)
        {
            case PororocaRequestAuthMode.Basic:
                postmanAuth.Type = PostmanAuthType.basic;
                postmanAuth.Basic = new PostmanVariable[]
                {
                    new() { Key = "username", Value = auth.BasicAuthLogin, Type = "string" },
                    new() { Key = "password", Value = auth.BasicAuthPassword, Type = "string" }
                };
                break;
            case PororocaRequestAuthMode.Bearer:
                postmanAuth.Type = PostmanAuthType.bearer;
                postmanAuth.Bearer = new PostmanVariable[]
                {
                    new() { Key = "token", Value = auth.BearerToken, Type = "string" }
                };
                break;
            default:
                postmanAuth.Type = PostmanAuthType.noauth;
                break;
        }

        return postmanAuth;
    }

    private static PostmanCollectionItem[] ConvertToPostmanItems(IReadOnlyList<PororocaCollectionFolder> folders, IReadOnlyList<PororocaHttpRequest> requests)
    {
        List<PostmanCollectionItem> postmanItems = new();
        postmanItems.AddRange(requests.Select(ConvertToPostmanItem));
        postmanItems.AddRange(folders.Select(ConvertToPostmanItem));

        return postmanItems.ToArray();
    }

    private static PostmanCollectionItem ConvertToPostmanItem(PororocaCollectionFolder colFolder)
    {
        List<PostmanCollectionItem> folderItems = new();
        folderItems.AddRange(colFolder.Folders.Select(ConvertToPostmanItem));
        folderItems.AddRange(colFolder.HttpRequests.Select(ConvertToPostmanItem));

        return new()
        {
            Name = colFolder.Name,
            Request = null,
            Items = folderItems.ToArray()
        };
    }

    internal static PostmanCollectionItem ConvertToPostmanItem(PororocaHttpRequest req) =>
        new()
        {
            Name = req.Name,
            Items = null,
            Request = new()
            {
                Auth = ConvertToPostmanAuth(req.CustomAuth),
                Method = req.HttpMethod,
                Url = req.Url,
                Header = ConvertToPostmanHeaders(req.Headers),
                Body = ConvertToPostmanRequestBody(req.Body)
            },
            Response = Array.Empty<object>()
        };

    internal static PostmanVariable[] ConvertToPostmanHeaders(IEnumerable<PororocaKeyValueParam>? hdrs) =>
        hdrs == null ?
        Array.Empty<PostmanVariable>() :
        hdrs.Select(v => new PostmanVariable() { Key = v.Key, Value = v.Value, Type = "text", Disabled = (v.Enabled == false ? true : null) })
            .ToArray();

    internal static PostmanRequestBody? ConvertToPostmanRequestBody(PororocaHttpRequestBody? reqBody)
    {
        PostmanRequestBody body = new();

        switch (reqBody?.Mode)
        {
            case PororocaHttpRequestBodyMode.Raw:
                body.Mode = PostmanRequestBodyMode.Raw;
                body.Raw = reqBody.RawContent;
                body.Options = FindPostmanBodyOptionsForContentType(reqBody.ContentType);
                break;
            case PororocaHttpRequestBodyMode.UrlEncoded:
                body.Mode = PostmanRequestBodyMode.Urlencoded;
                body.Urlencoded = ConvertToPostmanUrlEncodedValues(reqBody.UrlEncodedValues);
                break;
            case PororocaHttpRequestBodyMode.File:
                body.Mode = PostmanRequestBodyMode.File;
                body.File = new() { Src = reqBody.FileSrcPath };
                break;
            case PororocaHttpRequestBodyMode.FormData:
                body.Mode = PostmanRequestBodyMode.Formdata;
                body.Formdata = ConvertToPostmanFormDataParams(reqBody.FormDataValues);
                break;
            case PororocaHttpRequestBodyMode.GraphQl:
                body.Mode = PostmanRequestBodyMode.Graphql;
                body.Graphql = new()
                {
                    Query = reqBody.GraphQlValues?.Query ?? string.Empty,
                    Variables = reqBody.GraphQlValues?.Variables ?? string.Empty
                };
                break;
            default:
                return null;
        }

        return body;
    }

    private static PostmanRequestBodyOptions? FindPostmanBodyOptionsForContentType(string? contentType)
    {
        foreach (string lang in PostmanRequestBodyRawOptions.PostmanRequestBodyRawLanguages)
        {
            if (contentType?.Contains(lang) == true)
            {
                return new() { Raw = new() { Language = lang } };
            }
        }
        return null;
    }

    private static PostmanVariable[]? ConvertToPostmanUrlEncodedValues(IReadOnlyList<PororocaKeyValueParam>? urlEncoded) =>
        urlEncoded?.Select(v => new PostmanVariable() { Key = v.Key, Value = v.Value, Type = "text", Disabled = (v.Enabled == false ? true : null) })
                  ?.ToArray();

    private static PostmanRequestBodyFormDataParam[]? ConvertToPostmanFormDataParams(IEnumerable<PororocaHttpRequestFormDataParam>? formDataValues) =>
        formDataValues
        ?.Select(p => new PostmanRequestBodyFormDataParam()
        {
            Type = p.Type switch
            {
                PororocaHttpRequestFormDataParamType.File => PostmanRequestBodyFormDataParamType.File,
                _ => PostmanRequestBodyFormDataParamType.Text
            },
            Key = p.Key,
            Value = p.TextValue,
            ContentType = p.ContentType,
            Disabled = p.Enabled == false ? true : null,
            Src = p.FileSrcPath
        })
        ?.ToArray();
}