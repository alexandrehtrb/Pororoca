using System.Text;
using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.ImportCollection;

public static class OpenApiImporter
{
    private static string ToPororocaTemplateStyle(this string input) =>
        input.Replace("{", "{{").Replace("}", "}}");

    private static string Untemplatize(this string s) =>
        s.Replace("{{", string.Empty).Replace("}}", string.Empty);

    public static bool TryImportOpenApi(string openApiFileContent, out PororocaCollection? pororocaCollection)
    {
        try
        {
            OpenApiStringReader reader = new();
            var doc = reader.Read(openApiFileContent, out var diag);

            var collectionScopedAuth = ReadAuth(doc.SecurityRequirements);
            bool hasCollectionScopedAuth = collectionScopedAuth is not null;

            PororocaCollection col = new(doc.Info.Title)
            {
                CollectionScopedAuth = collectionScopedAuth
            };
            ReadEnvironments(doc, col);
            AddOAuth2ToCollectionIfSpecified(col, doc.Components.SecuritySchemes);
            AddCollectionScopedAuthVariables(col);

            var collectionScopedApiKeyHeaders = ReadApiKeys(doc.SecurityRequirements, ParameterLocation.Header);
            AddCollectionScopedApiKeys(col, collectionScopedApiKeyHeaders);
            // not bothering with collection-scoped api key query parameters

            foreach (var (path, pathItem) in doc.Paths)
            {
                string reqPath = path.ToPororocaTemplateStyle();
                foreach (var (operationType, operation) in pathItem.Operations)
                {
                    string reqName = operation.Summary ?? operation.Description ?? "req";
                    string httpMethod = operationType.ToString().ToUpper();
                    var headers = ReadRequestHeaders(operation.Parameters, collectionScopedApiKeyHeaders, operation.Security);
                    string queryParameters = ReadQueryParameters(operation.Parameters, operation.Security);
                    // TODO: get cookies
                    var reqBody = ReadRequestBody(operation.RequestBody);
                    var reqAuth = ReadRequestAuth(hasCollectionScopedAuth, operation.Security);

                    PororocaHttpRequest req = new(reqName);
                    req.Update(
                        name: reqName,
                        httpVersion: 1.1m,
                        httpMethod: httpMethod,
                        url: "{{BaseUrl}}" + reqPath + queryParameters,
                        customAuth: reqAuth,
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

    private static void ReadEnvironments(OpenApiDocument doc, PororocaCollection col)
    {
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

    private static List<PororocaKeyValueParam> ReadRequestHeaders(IList<OpenApiParameter> reqParams, IDictionary<string, string>? colScopedApiKeyHeaders, IList<OpenApiSecurityRequirement> reqSecurity)
    {
        var reqHeaders = reqParams.Where(p => p.In == ParameterLocation.Header)
                                  .Select(p => new PororocaKeyValueParam(true, p.Name, ConvertOpenApiSchemaToObject(p.Schema)?.ToString()))
                                  .ToList();

        if (colScopedApiKeyHeaders is not null)
        {
            reqHeaders.AddRange(colScopedApiKeyHeaders.Select(kv => new PororocaKeyValueParam(true, kv.Key, kv.Value)));
        }

        var reqApiKeyHeaders = ReadApiKeys(reqSecurity, ParameterLocation.Header);
        if (reqApiKeyHeaders is not null)
        {
            reqHeaders.AddRange(reqApiKeyHeaders.Select(kv => new PororocaKeyValueParam(true, kv.Key, kv.Value)));
        }

        return reqHeaders;
    }

    private static string ReadQueryParameters(IList<OpenApiParameter> parameters, IList<OpenApiSecurityRequirement> reqSecurity)
    {
        List<(string name, string? defaultValue)> qryParams =
            parameters.Where(p => p.In == ParameterLocation.Query)
                      .Select(p => (p.Name, ConvertOpenApiSchemaToObject(p.Schema)?.ToString()))
                      .ToList();

        var reqApiKeyQueryParams = ReadApiKeys(reqSecurity, ParameterLocation.Query);
        if (reqApiKeyQueryParams is not null)
        {
            qryParams.AddRange(reqApiKeyQueryParams.Select(x => (x.Key, x.Value))!);
        }

        StringBuilder sb = new();
        for (int i = 0; i < qryParams.Count; i++)
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

    #region REQUEST BODY

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
            string rawStr = JsonUtils.PrettySerializeJson(obj);
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

    #endregion

    #region AUTHENTICATION

    private static void AddCollectionScopedAuthVariables(PororocaCollection col)
    {


        if (col.CollectionScopedAuth is null)
            return;

        foreach (var env in col.Environments)
        {
            if (col.CollectionScopedAuth.Mode == PororocaRequestAuthMode.Basic)
            {
                env.Variables.Add(new(true, Untemplatize(col.CollectionScopedAuth.BasicAuthLogin!), string.Empty, true));
                env.Variables.Add(new(true, Untemplatize(col.CollectionScopedAuth.BasicAuthPassword!), string.Empty, true));
            }
            else if (col.CollectionScopedAuth.Mode == PororocaRequestAuthMode.Bearer)
            {
                env.Variables.Add(new(true, Untemplatize(col.CollectionScopedAuth.BearerToken!), string.Empty, true));
            }
        }
    }

    private static void AddCollectionScopedApiKeys(PororocaCollection col, Dictionary<string, string>? collectionScopedApiKeyHeaders)
    {
        if (collectionScopedApiKeyHeaders is null)
            return;

        foreach (var env in col.Environments)
        {
            env.Variables.AddRange(collectionScopedApiKeyHeaders.Select(kv => new PororocaVariable(true, kv.Key, kv.Value, true)));
        }
    }

    private static PororocaRequestAuth? ReadRequestAuth(bool hasCollectionScopedAuth, IList<OpenApiSecurityRequirement> security) =>
        ReadAuth(security) ?? (hasCollectionScopedAuth ? PororocaRequestAuth.InheritedFromCollection : null);

    private static void AddOAuth2ToCollectionIfSpecified(PororocaCollection col, IDictionary<string, OpenApiSecurityScheme> securitySchemes)
    {
        var oauth2Schemes = securitySchemes.Where(x => x.Value.Type == SecuritySchemeType.OAuth2);
        if (oauth2Schemes.Any() == false)
        {
            return;
        }
        else
        {
            foreach (var env in col.Environments)
            {
                env.Variables.Add(new(true, "oauth2_client_id", string.Empty, true));
                env.Variables.Add(new(true, "oauth2_client_secret", string.Empty, true));
            }

            foreach (var (schemeName, scheme) in oauth2Schemes)
            {
                PororocaCollectionFolder authFolder = new(schemeName);
                if (scheme.Flows.AuthorizationCode is not null)
                {
                    authFolder.AddFolder(GenerateOAuth2AuthorizationCodeRequestsFolder(scheme.Flows.AuthorizationCode));
                }
                if (scheme.Flows.ClientCredentials is not null)
                {
                    authFolder.AddFolder(GenerateOAuth2ClientCredentialsRequestsFolder(scheme.Flows.ClientCredentials));
                }
                col.Folders.Add(authFolder);
            }
        }
    }

    private static PororocaRequestAuth? ReadAuth(IList<OpenApiSecurityRequirement>? security)
    {
        if (security is null || security.Count == 0 || security.First().Count == 0)
            return null;

        // only caring about one security requirement, others will be ignored
        var (scheme, _) = security[0].First();

        if (scheme.Type == SecuritySchemeType.Http && scheme.Scheme == "basic")
        {
            return PororocaRequestAuth.MakeBasicAuth("{{basic_auth_login}}", "{{basic_auth_password}}");
        }
        else if (scheme.Type == SecuritySchemeType.Http && scheme.Scheme == "bearer")
        {
            return PororocaRequestAuth.MakeBearerAuth("{{bearer_auth_token}}");
        }
        // API keys, OAuth2 and OIDC are handled separately
        else if (scheme.Type == SecuritySchemeType.OAuth2)
        {
            return PororocaRequestAuth.MakeBearerAuth("{{oauth2_access_token}}");
        }
        else if (scheme.Type == SecuritySchemeType.OpenIdConnect)
        {
            return PororocaRequestAuth.MakeBearerAuth("{{openidconnect_access_token}}");
        }
        else
        {
            return null;
        }
    }

    private static Dictionary<string, string>? ReadApiKeys(IList<OpenApiSecurityRequirement>? security, ParameterLocation location)
    {
        if (security is null || security.Count == 0 || security.First().Count == 0)
            return null;

        // only caring about one security requirement, others will be ignored
        return security[0]
               .Where((s, _) => s.Key.Type == SecuritySchemeType.ApiKey && s.Key.In == location)
               .Select((s, _) => s.Key.Name)
               .ToDictionary(s => s, s => "{{" + s + "}}");
    }

    private static PororocaCollectionFolder GenerateOAuth2AuthorizationCodeRequestsFolder(OpenApiOAuthFlow flow)
    {
        PororocaHttpRequest getAuthCodeReq = new("Get auth code");
        StringBuilder sbUrl = new(flow.AuthorizationUrl.ToString());
        sbUrl.Append("?client_id={{oauth2_client_id}}");
        sbUrl.Append("&redirect_uri={{oauth2_redirect_uri}}");
        sbUrl.Append("&response_type=code");
        sbUrl.Append("&response_mode=query");
        sbUrl.Append("&state=12345");
        getAuthCodeReq.UpdateMethod("GET");
        getAuthCodeReq.UpdateUrl(sbUrl.ToString());

        PororocaHttpRequest getAccessTokenReq = new("Get access token");
        getAccessTokenReq.UpdateUrl(flow.TokenUrl.ToString());
        PororocaHttpRequestBody body = new();
        body.SetUrlEncodedContent(new PororocaKeyValueParam[]
        {
            new(true, "grant_type", "authorization_code"),
            new(true, "client_id", "{{oauth2_client_id}}"),
            new(true, "client_secret", "{{oauth2_client_secret}}"),
            new(true, "scope", string.Join(' ', flow.Scopes.Select(s => s.Key))),
            new(true, "redirect_uri", "{{oauth2_redirect_uri}}")
        });
        getAccessTokenReq.UpdateBody(body);
        getAccessTokenReq.UpdateMethod("POST");
        getAccessTokenReq.UpdateResponseCaptures(new()
        {
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token", null, "$.access_token"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token_expires_in", null, "$.expires_in"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_refresh_token", null, "$.refresh_token")
        });

        PororocaHttpRequest renewAccessTokenReq = new("Renew access token");
        renewAccessTokenReq.UpdateUrl(flow.TokenUrl.ToString());
        PororocaHttpRequestBody body2 = new();
        body.SetUrlEncodedContent(new PororocaKeyValueParam[]
        {
            new(true, "grant_type", "refresh_token"),
            new(true, "client_id", "{{oauth2_client_id}}"),
            new(true, "client_secret", "{{oauth2_client_secret}}"),
            new(true, "scope", string.Join(' ', flow.Scopes.Select(s => s.Key))),
            new(true, "refresh_token", "{{oauth2_refresh_token}}")
        });
        renewAccessTokenReq.UpdateBody(body);
        renewAccessTokenReq.UpdateMethod("POST");
        renewAccessTokenReq.UpdateResponseCaptures(new()
        {
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token", null, "$.access_token"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token_expires_in", null, "$.expires_in"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_refresh_token", null, "$.refresh_token")
        });

        PororocaCollectionFolder folder = new("Authorization code");
        folder.AddRequest(getAuthCodeReq);
        folder.AddRequest(getAccessTokenReq);
        folder.AddRequest(renewAccessTokenReq);
        return folder;
    }

    private static PororocaCollectionFolder GenerateOAuth2ClientCredentialsRequestsFolder(OpenApiOAuthFlow flow)
    {
        PororocaHttpRequest getAccessTokenReq = new("Get access token");
        getAccessTokenReq.UpdateUrl(flow.TokenUrl.ToString());
        PororocaHttpRequestBody body = new();
        body.SetUrlEncodedContent(new PororocaKeyValueParam[]
        {
            new(true, "grant_type", "client_credentials"),
            new(true, "client_id", "{{oauth2_client_id}}"),
            new(true, "client_secret", "{{oauth2_client_secret}}"),
            new(true, "scope", string.Join(' ', flow.Scopes.Select(s => s.Key)))
        });
        getAccessTokenReq.UpdateBody(body);
        getAccessTokenReq.UpdateMethod("POST");
        getAccessTokenReq.UpdateResponseCaptures(new()
        {
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token", null, "$.access_token"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token_expires_in", null, "$.expires_in"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_refresh_token", null, "$.refresh_token")
        });

        PororocaCollectionFolder folder = new("Client credentials");
        folder.AddRequest(getAccessTokenReq);
        return folder;
    }

    #endregion

    #region OTHERS

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
            return innerObj is not null ? [innerObj] : Array.Empty<object>();
        }
        else if (schema.Type == "object")
        {
            return schema.Properties.ToDictionary(x => x.Key, x => ConvertOpenApiSchemaToObject(x.Value));
        }

        return null;
    }

    #endregion
}