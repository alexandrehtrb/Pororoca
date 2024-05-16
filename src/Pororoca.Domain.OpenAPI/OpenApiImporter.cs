using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;

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

            var collectionScopedAuth = ReadAuth(doc.Components.SecuritySchemes);
            bool hasCollectionScopedAuth = collectionScopedAuth is not null;

            PororocaCollection col = new(doc.Info.Title)
            {
                CollectionScopedAuth = collectionScopedAuth,
                CollectionScopedRequestHeaders = new()
            };
            ReadEnvironments(doc, col);
            AddOAuth2ToCollectionIfSpecified(col, doc.Components.SecuritySchemes);
            AddCollectionScopedAuthVariables(col);

            var colScopedSecHeaders = ReadCollectionScopedSecurityHeaders(doc.Components.SecuritySchemes);
            AddCollectionScopedSecurityHeaders(col, colScopedSecHeaders);
            // not bothering with collection-scoped api key query parameters

            foreach (var (path, pathItem) in doc.Paths)
            {
                string reqPath = path.ToPororocaTemplateStyle();
                foreach (var (operationType, operation) in pathItem.Operations)
                {
                    string reqName = operation.Summary ?? operation.Description ?? "req";
                    string httpMethod = operationType.ToString().ToUpper();
                    var headers = ReadRequestHeaders(operation.Parameters, colScopedSecHeaders, operation.Security);
                    string queryParameters = ReadQueryParameters(operation.Parameters, operation.Security);
                    // TODO: get cookies
                    var reqBody = ReadRequestBody(operation.RequestBody);
                    var reqAuth = ReadRequestAuth(hasCollectionScopedAuth, operation.Security, doc.Components.SecuritySchemes);

                    PororocaHttpRequest req = new(
                        Name: reqName,
                        HttpVersion: 1.1m,
                        HttpMethod: httpMethod,
                        Url: "{{BaseUrl}}" + reqPath + queryParameters,
                        CustomAuth: reqAuth,
                        Headers: headers,
                        Body: reqBody,
                        ResponseCaptures: null);

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
                folder.Requests.Add(req);
            }
            else
            {
                col.Folders.Add(folder = new(folderName));
                folder.Requests.Add(req);
            }
        }
    }

    private static List<PororocaKeyValueParam> ReadRequestHeaders(IList<OpenApiParameter> reqParams, IDictionary<string, string>? colScopedSecHeaders, IList<OpenApiSecurityRequirement> reqSecurity)
    {
        var reqHeaders = reqParams.Where(p => p.In == ParameterLocation.Header)
                                  .Select(p => new PororocaKeyValueParam(true, p.Name, ConvertOpenApiSchemaToObject(p.Schema)?.ToString()))
                                  .ToList();

        var reqSecHeaders = ReadRequestScopedSecurity(reqSecurity, ParameterLocation.Header);
        if (reqSecHeaders is not null)
        {
            foreach (var rh in reqSecHeaders)
            {
                if (colScopedSecHeaders is null || !colScopedSecHeaders.Any(ch => ch.Key == rh.Key))
                {
                    reqHeaders.Add(new PororocaKeyValueParam(true, rh.Key, rh.Value));
                }
            }
        }

        return reqHeaders;
    }

    private static string ReadQueryParameters(IList<OpenApiParameter> parameters, IList<OpenApiSecurityRequirement> reqSecurity)
    {
        List<(string name, string? defaultValue)> qryParams =
            parameters.Where(p => p.In == ParameterLocation.Query)
                      .Select(p => (p.Name, ConvertOpenApiSchemaToObject(p.Schema)?.ToString()))
                      .ToList();

        var reqApiKeyQueryParams = ReadRequestScopedSecurity(reqSecurity, ParameterLocation.Query);
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
        JsonNode? node = null;
        if (input is OpenApiSchema schema)
        {
            node = ConvertOpenApiSchemaToObject(schema);
        }
        else if (input is IOpenApiAny any)
        {
            node = ConvertOpenApiAnyToObject(any);
        }

        if (contentType?.Contains("json") == true)
        {
            string rawStr = PrettySerializeJson(node);
            return MakeRawContent(rawStr, contentType);
        }
        else if (contentType?.Contains("octet-stream") == true)
        {
            // probably a file input
            return MakeFileContent(string.Empty, contentType);
        }
        else if (contentType?.Contains("urlencoded") == true)
        {
            var urlEncodedParams = node is JsonObject obj ?
                                   obj.Select(kv => new PororocaKeyValueParam(true, kv.Key, kv.Value?.ToString())) :
                                   Array.Empty<PororocaKeyValueParam>();

            return MakeUrlEncodedContent(urlEncodedParams);
        }
        else if (contentType?.Contains("form-data") == true)
        {
            return MakeFormDataContent(Array.Empty<PororocaHttpRequestFormDataParam>());
        }
        else
        {
            // only JSON and URL encoded will be supported for now
            return MakeRawContent(string.Empty, contentType ?? string.Empty);
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

    private static void AddCollectionScopedSecurityHeaders(PororocaCollection col, Dictionary<string, string>? collectionScopedSecurityHeaders)
    {
        if (collectionScopedSecurityHeaders is null)
            return;

        foreach (var env in col.Environments)
        {
            env.Variables.AddRange(collectionScopedSecurityHeaders.Select(kv => new PororocaVariable(true, kv.Key, string.Empty, true)));
        }

        col.CollectionScopedRequestHeaders!.AddRange(
            collectionScopedSecurityHeaders.Select(x => new PororocaKeyValueParam(true, x.Key, x.Value)));
    }

    private static PororocaRequestAuth? ReadRequestAuth(bool hasCollectionScopedAuth, IList<OpenApiSecurityRequirement> securityRequirements, IDictionary<string, OpenApiSecurityScheme> securitySchemes)
    {
        var reqPrimarySecRequirement = securityRequirements.FirstOrDefault();
        if (reqPrimarySecRequirement is null)
        {
            return null;
        }
        else
        {
            var colScopedAuthScheme = securitySchemes.Values.FirstOrDefault();
            var reqPrimaryAuthScheme = reqPrimarySecRequirement.Keys.FirstOrDefault();

            if (hasCollectionScopedAuth && reqPrimaryAuthScheme?.Name == colScopedAuthScheme!.Name)
            {
                return PororocaRequestAuth.InheritedFromCollection;
            }
            else
            {
                var dict = securitySchemes.Where(x => x.Key == reqPrimaryAuthScheme?.Name).ToDictionary();
                return ReadAuth(dict);
            }
        }
    }

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
                    authFolder.Folders.Add(GenerateOAuth2AuthorizationCodeRequestsFolder(scheme.Flows.AuthorizationCode));
                }
                if (scheme.Flows.ClientCredentials is not null)
                {
                    authFolder.Folders.Add(GenerateOAuth2ClientCredentialsRequestsFolder(scheme.Flows.ClientCredentials));
                }
                col.Folders.Add(authFolder);
            }
        }
    }

    private static PororocaRequestAuth? ReadAuth(IDictionary<string, OpenApiSecurityScheme> securitySchemes)
    {
        if (securitySchemes is null || securitySchemes.Count == 0)
            return null;

        // only caring about one security requirement, others will be ignored
        var scheme = securitySchemes.First().Value;

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

    private static Dictionary<string, string>? ReadCollectionScopedSecurityHeaders(IDictionary<string, OpenApiSecurityScheme> securitySchemes)
    {
        if (securitySchemes is null || securitySchemes.Count == 0)
            return null;

        // only caring about one security requirement, others will be ignored
        return securitySchemes
               .Where((s, _) => (s.Value.Type == SecuritySchemeType.ApiKey || s.Value.Type == SecuritySchemeType.Http) && s.Value.In == ParameterLocation.Header)
               .Select((s, _) => s.Value.Name)
               .ToDictionary(s => s, s => "{{" + s + "}}");
    }

    private static Dictionary<string, string>? ReadRequestScopedSecurity(IList<OpenApiSecurityRequirement>? security, ParameterLocation location)
    {
        if (security is null || security.Count == 0 || security.First().Count == 0)
            return null;

        // only caring about one security requirement, others will be ignored
        return security[0]
               .Where((s, _) => (s.Key.Type == SecuritySchemeType.ApiKey || s.Key.Type == SecuritySchemeType.Http) && s.Key.In == location)
               .Select((s, _) => s.Key.Name)
               .ToDictionary(s => s, s => "{{" + s + "}}");
    }

    private static PororocaCollectionFolder GenerateOAuth2AuthorizationCodeRequestsFolder(OpenApiOAuthFlow flow)
    {
        StringBuilder sbUrl = new(flow.AuthorizationUrl.ToString());
        sbUrl.Append("?client_id={{oauth2_client_id}}");
        sbUrl.Append("&redirect_uri={{oauth2_redirect_uri}}");
        sbUrl.Append("&response_type=code");
        sbUrl.Append("&response_mode=query");
        sbUrl.Append("&state=12345");
        PororocaHttpRequest getAuthCodeReq = new(
            Name: "Get auth code",
            HttpMethod: "GET",
            Url: sbUrl.ToString());

        PororocaHttpRequest getAccessTokenReq = new(
            Name: "Get access token",
            HttpMethod: "POST",
            Url: flow.TokenUrl.ToString(),
            Body: MakeUrlEncodedContent(
            [
                new(true, "grant_type", "authorization_code"),
                new(true, "client_id", "{{oauth2_client_id}}"),
                new(true, "client_secret", "{{oauth2_client_secret}}"),
                new(true, "scope", string.Join(' ', flow.Scopes.Select(s => s.Key))),
                new(true, "redirect_uri", "{{oauth2_redirect_uri}}")
            ]),
            ResponseCaptures:
            [
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token", null, "$.access_token"),
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token_expires_in", null, "$.expires_in"),
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_refresh_token", null, "$.refresh_token")
            ]);

        PororocaHttpRequest renewAccessTokenReq = new(
            Name: "Renew access token",
            HttpMethod: "POST",
            Url: flow.TokenUrl.ToString(),
            Body: MakeUrlEncodedContent(
            [
                new(true, "grant_type", "refresh_token"),
                new(true, "client_id", "{{oauth2_client_id}}"),
                new(true, "client_secret", "{{oauth2_client_secret}}"),
                new(true, "scope", string.Join(' ', flow.Scopes.Select(s => s.Key))),
                new(true, "refresh_token", "{{oauth2_refresh_token}}")
            ]),
            ResponseCaptures:
            [
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token", null, "$.access_token"),
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token_expires_in", null, "$.expires_in"),
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_refresh_token", null, "$.refresh_token")
            ]);

        return new(
            Name: "Authorization code",
            Folders: [],
            Requests: [ getAuthCodeReq, getAccessTokenReq, renewAccessTokenReq ]);
    }

    private static PororocaCollectionFolder GenerateOAuth2ClientCredentialsRequestsFolder(OpenApiOAuthFlow flow)
    {
        PororocaHttpRequest getAccessTokenReq = new(
            Name: "Get access token",
            HttpMethod: "POST",
            Url: flow.TokenUrl.ToString(),
            Body: MakeUrlEncodedContent(
            [
                new(true, "grant_type", "client_credentials"),
                new(true, "client_id", "{{oauth2_client_id}}"),
                new(true, "client_secret", "{{oauth2_client_secret}}"),
                new(true, "scope", string.Join(' ', flow.Scopes.Select(s => s.Key)))
            ]),
            ResponseCaptures:
            [
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token", null, "$.access_token"),
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token_expires_in", null, "$.expires_in"),
                new(PororocaHttpResponseValueCaptureType.Body, "oauth2_refresh_token", null, "$.refresh_token")
            ]);

        return new(
            Name: "Client credentials",
            Folders: [],
            Requests: [ getAccessTokenReq ]);
    }

    #endregion

    #region OTHERS

    private static string PrettySerializeJson(JsonNode? node)
    {
        if (node is null) return string.Empty;
        else return JsonSerializer.Serialize(node, PrettifyJsonCtx.JsonNode);
    }

    private static JsonNode? ConvertOpenApiAnyToObject(IOpenApiAny? val)
    {
        if (val is OpenApiObject o)
        {
            JsonObject obj = [];
            foreach (var (k,v) in o)
            {
                obj.Add(k, ConvertOpenApiAnyToObject(v));
            }
            return obj;
        }
        if (val is OpenApiArray a)
        {
            return new JsonArray(a.Select(ConvertOpenApiAnyToObject).ToArray());
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

    private static JsonNode? ConvertOpenApiSchemaToObject(OpenApiSchema schema)
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
            var innerObj = ConvertOpenApiSchemaToObject(schema.Items);
            return innerObj is not null ? new JsonArray(innerObj) : [];
        }
        else if (schema.Type == "object")
        {
            JsonObject obj = [];
            foreach (var (k,v) in schema.Properties)
            {
                obj.Add(k, ConvertOpenApiSchemaToObject(v));
            }
            return obj;
        }

        return null;
    }

    #endregion
}