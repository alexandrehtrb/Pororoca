using System.Text;
using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportCollection;

public static class OpenApiImporter
{
    private static string ToPororocaTemplateStyle(this string input) =>
        input.Replace("{", "{{").Replace("}", "}}");

    public static bool TryImportOpenApi(string openApiFileContent, out PororocaCollection? pororocaCollection)
    {
        try
        {
            OpenApiStringReader reader = new();
            var doc = reader.Read(openApiFileContent, out var diag);

            PororocaCollection col = new(doc.Info.Title);
            for (int i = 0; i < doc.Servers.Count; i++)
            {
                var server = doc.Servers[i];
                PororocaEnvironment env = new(server.Description ?? ("env" + (i + 1)));
                env.Variables.Add(new(true, "BaseUrl", server.Url.ToPororocaTemplateStyle().TrimEnd('/'), false));
                foreach (var (serverVarName, serverVar) in server.Variables)
                {
                    foreach (string serverVarEnumValue in serverVar.Enum)
                    {
                        env.Variables.Add(new(true, serverVarName, serverVarEnumValue, false));
                    }
                }
                col.Environments.Add(env);
            }

            // TODO: Get OAuth authentication

            foreach (var (path, pathItem) in doc.Paths)
            {
                string reqPath = path.ToPororocaTemplateStyle();
                foreach (var (operationType, operation) in pathItem.Operations)
                {
                    string reqName = operation.Summary;
                    string httpMethod = operationType.ToString().ToUpper();
                    var headers = ReadHeaders(operation.Parameters);
                    string queryParameters = ReadQueryParameters(operation.Parameters);
                    // TODO: get cookies
                    var reqBody = ReadRequestBody(operation.RequestBody);

                    PororocaHttpRequest req = new(reqName);
                    req.Update(
                        name: reqName,
                        httpVersion: 1.1m,
                        httpMethod: httpMethod,
                        url: "{{BaseUrl}}" + reqPath + queryParameters,
                        customAuth: null,
                        headers: headers,
                        body: reqBody,
                        captures: null);

                    PlaceRequestInCollection(col, operation, req);
                }
            }

            pororocaCollection = col;
            return true;
        }
        catch
        {
            pororocaCollection = null;
            return false;
        }
    }

    private static void PlaceRequestInCollection(PororocaCollection col, OpenApiOperation operation, PororocaHttpRequest req)
    {
        string? folderName = operation.Tags.FirstOrDefault()?.Name;
        if (folderName is null)
        {
            col.Requests.Add(req);
        }
        else
        {
            var folder = col.Folders.FirstOrDefault(f => f.Name == folderName);
            if (folder is not null)
            {
                folder.AddRequest(req);
            }
            else
            {
                col.Folders.Add(folder = new(folderName));
                folder.AddRequest(req);
            }
        }
    }

    private static List<PororocaKeyValueParam> ReadHeaders(IList<OpenApiParameter> parameters) =>
        parameters
        .Where(p => p.In == ParameterLocation.Header)
        .Select(p => new PororocaKeyValueParam(true, p.Name, ConvertOpenApiSchemaToObject(p.Schema)?.ToString()))
        .ToList();

    private static string ReadQueryParameters(IList<OpenApiParameter> parameters)
    {
        (string name, string? defaultValue)[] qryParams =
            parameters.Where(p => p.In == ParameterLocation.Query)
                      .Select(p => (p.Name, ConvertOpenApiSchemaToObject(p.Schema)?.ToString()))
                      .ToArray();

        StringBuilder sb = new();
        for (int i = 0; i < qryParams.Length; i++)
        {
            var (p, defaultValue) = qryParams[i];
            sb.Append(i == 0 ? '?' : '&');
            if (defaultValue is not null)
            {
                sb.Append(p + '=' + defaultValue);
            }
            else
            {
                sb.Append(p + "={{" + p + "}}");
            }
        }

        return sb.ToString();
    }

    private static PororocaHttpRequestBody? ReadRequestBody(OpenApiRequestBody? reqBody)
    {
        // 1) Prefer content that has example;
        // 2) Prefer content that is application/json;
        // 3) Prefer content that is other type;
        // 4) Handle content-types XML and URL encoded
        // 5) If content has no example, then generate example from component schema

        if (reqBody is null)
            return null;

        try
        {
            var reqBodiesWithExample = reqBody.Content.Where(x => x.Value.Examples.Any() || x.Value.Example is not null);

            KeyValuePair<string?, OpenApiMediaType?>? x;

            x = reqBodiesWithExample.FirstOrDefault(kv => kv.Key.Contains("json"));
            if (x.Value.Value is not null)
                return ReadRequestBodyFromExample(x.Value.Key!, x.Value.Value!);

            x = reqBodiesWithExample.FirstOrDefault();
            if (x.Value.Value is not null)
                return ReadRequestBodyFromExample(x.Value.Key!, x.Value.Value!);

            x = reqBody.Content.FirstOrDefault(kv => kv.Key.Contains("json"));
            if (x.Value.Value is not null)
                return ReadRequestBodyFromSchema(x.Value.Key!, x.Value.Value!);

            x = reqBody.Content.FirstOrDefault();
            if (x.Value.Value is not null)
                return ReadRequestBodyFromSchema(x.Value.Key!, x.Value.Value!);

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static PororocaHttpRequestBody? ReadRequestBodyFromExample(string? contentType, OpenApiMediaType content)
    {
        var openApiExample = content.Example ?? content.Examples?.FirstOrDefault().Value.Value;
        return ReadRequestBodyFromInput(contentType, openApiExample);
    }

    private static PororocaHttpRequestBody? ReadRequestBodyFromSchema(string? contentType, OpenApiMediaType content)
    {
        var schema = content.Schema;
        return ReadRequestBodyFromInput(contentType, schema);
    }

    private static PororocaHttpRequestBody? ReadRequestBodyFromInput(string? contentType, object? input)
    {
        object? obj = null;
        if (input is OpenApiSchema schema)
        {
            obj = ConvertOpenApiSchemaToObject(schema);
        }
        else if (input is IOpenApiAny any)
        {
            obj = ConvertOpenApiAnyToObject(any);
        }

        if (contentType?.Contains("json") == true)
        {
            string rawStr = JsonSerializer.Serialize(obj, options: ViewJsonResponseOptions);
            PororocaHttpRequestBody body = new();
            body.SetRawContent(rawStr, contentType);
            return body;
        }
        else if (contentType?.Contains("octet-stream") == true)
        {
            // probably a file input
            PororocaHttpRequestBody reqBody = new();
            reqBody.SetFileContent(string.Empty, contentType);
            return reqBody;
        }
        else if (contentType?.Contains("urlencoded") == true)
        {
            var urlEncodedParams = obj is IDictionary<string, object?> dict ?
                                   dict.Select(kv => new PororocaKeyValueParam(true, kv.Key, kv.Value?.ToString())) :
                                   Array.Empty<PororocaKeyValueParam>();

            PororocaHttpRequestBody reqBody = new();
            reqBody.SetUrlEncodedContent(urlEncodedParams);
            return reqBody;
        }
        else if (contentType?.Contains("form-data") == true)
        {
            PororocaHttpRequestBody reqBody = new();
            reqBody.SetFormDataContent(Array.Empty<PororocaHttpRequestFormDataParam>());
            return reqBody;
        }
        else
        {
            // only JSON and URL encoded will be supported for now
            PororocaHttpRequestBody reqBody = new();
            reqBody.SetRawContent(string.Empty, contentType ?? string.Empty);
            return reqBody;
        }
    }

    private static object? ConvertOpenApiAnyToObject(IOpenApiAny? val)
    {
        if (val is OpenApiObject o)
        {
            return o.ToDictionary(x => x.Key, x => ConvertOpenApiAnyToObject(x.Value));
        }
        if (val is OpenApiArray a)
        {
            return a.Select(ConvertOpenApiAnyToObject).ToArray();
        }
        if (val is OpenApiBoolean b)
        {
            return b.Value;
        }
        if (val is OpenApiString s)
        {
            return s.Value;
        }
        if (val is OpenApiInteger i)
        {
            return i.Value;
        }
        if (val is OpenApiDate d)
        {
            return d.Value;
        }
        if (val is OpenApiDateTime dt)
        {
            return dt.Value;
        }
        if (val is OpenApiDouble db)
        {
            return db.Value;
        }
        if (val is OpenApiFloat f)
        {
            return f.Value;
        }
        if (val is OpenApiLong l)
        {
            return l.Value;
        }
        return null;
    }

    private static object? ConvertOpenApiSchemaToObject(OpenApiSchema schema)
    {
        if (schema.Example is not null)
        {
            return ConvertOpenApiAnyToObject(schema.Example);
        }
        else if (schema.Default is not null)
        {
            return ConvertOpenApiAnyToObject(schema.Default);
        }
        else if (schema.Enum?.Any() == true)
        {
            var firstEnumExample = schema.Enum.First();
            return ConvertOpenApiAnyToObject(firstEnumExample);
        }
        else if (schema.Type == "integer")
        {
            return 0;
        }
        else if (schema.Type == "string")
        {
            if (schema.Format == "date-time")
            {
                return DateTimeOffset.Now.ToString("O");
            }
            else
            {
                return string.Empty;
            }
        }
        else if (schema.Type == "boolean")
        {
            return false;
        }
        else if (schema.Type == "array")
        {
            object? innerObj = ConvertOpenApiSchemaToObject(schema.Items);
            return innerObj is not null ? new[] { innerObj } : Array.Empty<object>();
        }
        else if (schema.Type == "object")
        {
            return schema.Properties.ToDictionary(x => x.Key, x => ConvertOpenApiSchemaToObject(x.Value));
        }
        
        return null;
    }
}