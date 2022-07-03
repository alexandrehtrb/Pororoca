using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Pororoca.Domain.Features.Entities.Pororoca;
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
            Items = ConvertToPostmanItems(col.Folders, col.Requests),
            Variable = col.Variables
                          .Select(v => new PostmanVariable()
                          {
                              Key = v.Key,
                              Value = shouldHideSecrets && v.IsSecret ? string.Empty : v.Value,
                              Disabled = v.Enabled ? null : true
                          })
                          .ToArray()
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

    private static PostmanCollectionItem[] ConvertToPostmanItems(IReadOnlyList<PororocaCollectionFolder> folders, IReadOnlyList<PororocaRequest> requests)
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
        folderItems.AddRange(colFolder.Requests.Select(ConvertToPostmanItem));

        return new()
        {
            Name = colFolder.Name,
            Request = null,
            Items = folderItems.ToArray()
        };
    }

    internal static PostmanCollectionItem ConvertToPostmanItem(PororocaRequest req) =>
        new()
        {
            Name = req.Name,
            Items = null,
            Request = new()
            {
                Auth = ConvertToPostmanAuth(req.CustomAuth),
                Method = req.HttpMethod,
                Url = ConvertToPostmanRequestUrl(req.Url),
                Header = ConvertToPostmanHeaders(req.Headers),
                Body = ConvertToPostmanRequestBody(req.Body)
            },
            Response = Array.Empty<object>()
        };

    internal static PostmanRequestUrl ConvertToPostmanRequestUrl(string rawUrl)
    {
        if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var absoluteUri))
        {
            return new PostmanRequestUrl()
            {
                Raw = rawUrl,
                Protocol = absoluteUri.Scheme,
                Host = absoluteUri.Host.Split('.', StringSplitOptions.RemoveEmptyEntries),
                Path = absoluteUri.Segments.Select(s => s.TrimStart('/').TrimEnd('/')).Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                Port = absoluteUri.IsDefaultPort || absoluteUri.Port <= 0 ? null : absoluteUri.Port.ToString(),
                Query = absoluteUri.Query.TrimStart('?')
                            .Split('&', StringSplitOptions.RemoveEmptyEntries)
                            .Select(kvStr =>
                            {
                                string[] kv = kvStr.Split('=');
                                if (kv.Length >= 2)
                                    return new PostmanVariable() { Key = kv[0], Value = kv[1] };
                                else
                                    return new PostmanVariable() { Key = kv.FirstOrDefault() ?? string.Empty, Value = null };
                            })
                            .ToArray()
            };
        }
        else
        {
            const string protocolRgxPart = @"(?<protocol>[_\d\w\{\}]+://)";
            const string hostRgxPart = @"(?<host>[_\d\w\.\{\}]+)";
            const string portRgxPart = @"(?<port>:[_\d\w\{\}]+)";
            const string segmentsRgxPart = @"(?<segments>(/[_\d\w\{\}#\.]*)*)";
            const string queryParamsRgxPart = @"\?(?<queryparams>.*)";

            string[] regexes = new[]
            {
                protocolRgxPart+hostRgxPart+portRgxPart+segmentsRgxPart+queryParamsRgxPart,
                protocolRgxPart+hostRgxPart+segmentsRgxPart+queryParamsRgxPart,
                hostRgxPart+segmentsRgxPart+queryParamsRgxPart,
                hostRgxPart+segmentsRgxPart,
                hostRgxPart+queryParamsRgxPart,
                hostRgxPart
            };

            for (int i = 0; i < regexes.Length; i++)
            {
                var mc = new Regex(regexes[i]).Matches(rawUrl);
                if (mc.Count == 0)
                {
                    continue;
                }
                else
                {
                    var matchGroups = mc.First().Groups.Values;
                    return new PostmanRequestUrl()
                    {
                        Raw = rawUrl,
                        Protocol = matchGroups.FirstOrDefault(g => g.Name == "protocol")?.Value?.Replace("://", string.Empty),
                        Host = matchGroups.First(g => g.Name == "host").Value.Split('.', StringSplitOptions.RemoveEmptyEntries),
                        Path = matchGroups.FirstOrDefault(g => g.Name == "segments")?.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries),
                        Port = matchGroups.FirstOrDefault(g => g.Name == "port")?.Value?.Replace(":", string.Empty),
                        Query = matchGroups.FirstOrDefault(g => g.Name == "queryparams")
                                           ?.Value
                                           ?.Split('&', StringSplitOptions.RemoveEmptyEntries)
                                           ?.Select(kvStr =>
                                           {
                                               string[] kv = kvStr.Split('=');
                                               if (kv.Length >= 2)
                                                   return new PostmanVariable() { Key = kv[0], Value = kv[1] };
                                               else
                                                   return new PostmanVariable() { Key = kv.FirstOrDefault() ?? string.Empty, Value = null };
                                           })
                                           ?.ToArray()
                    };
                }
            }

            return new PostmanRequestUrl() { Raw = rawUrl };
        }
    }

    internal static PostmanVariable[] ConvertToPostmanHeaders(IEnumerable<PororocaKeyValueParam>? hdrs) =>
        hdrs == null ?
        Array.Empty<PostmanVariable>() :
        hdrs.Select(v => new PostmanVariable() { Key = v.Key, Value = v.Value, Type = "text", Disabled = (v.Enabled == false ? true : null) })
            .ToArray();

    internal static PostmanRequestBody? ConvertToPostmanRequestBody(PororocaRequestBody? reqBody)
    {
        PostmanRequestBody body = new();

        switch (reqBody?.Mode)
        {
            case PororocaRequestBodyMode.Raw:
                body.Mode = PostmanRequestBodyMode.Raw;
                body.Raw = reqBody.RawContent;
                body.Options = FindPostmanBodyOptionsForContentType(reqBody.ContentType);
                break;
            case PororocaRequestBodyMode.UrlEncoded:
                body.Mode = PostmanRequestBodyMode.Urlencoded;
                body.Urlencoded = ConvertToPostmanUrlEncodedValues(reqBody.UrlEncodedValues);
                break;
            case PororocaRequestBodyMode.File:
                body.Mode = PostmanRequestBodyMode.File;
                body.File = new() { Src = reqBody.FileSrcPath };
                break;
            case PororocaRequestBodyMode.FormData:
                body.Mode = PostmanRequestBodyMode.Formdata;
                body.Formdata = ConvertToPostmanFormDataParams(reqBody.FormDataValues);
                break;
            case PororocaRequestBodyMode.GraphQl:
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

    private static PostmanRequestBodyFormDataParam[]? ConvertToPostmanFormDataParams(IEnumerable<PororocaRequestFormDataParam>? formDataValues) =>
        formDataValues
        ?.Select(p => new PostmanRequestBodyFormDataParam()
        {
            Type = p.Type switch
            {
                PororocaRequestFormDataParamType.File => PostmanRequestBodyFormDataParamType.File,
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