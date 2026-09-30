using System.Text.Json.Nodes;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;
using static Pororoca.Domain.Features.ImportCollection.OpenApiImporter;

namespace Pororoca.Domain.Tests.Features.ImportCollection;

public static class OpenApiImporterTests
{
    [Theory]
    [InlineData("3.0.0")]
    [InlineData("3.1.0")]
    [InlineData("3.1.1")]
    public static void Should_import_json_without_components(string version)
    {
        string fileContent = $$"""
            {
              "openapi": "{{version}}",
              "info": { "title": "API without components", "version": "1.0" },
              "servers": [{ "url": "https://example.com/api" }],
              "paths": {
                "/items": {
                  "get": { "summary": "List items", "responses": { "200": { "description": "OK" } } }
                }
              }
            }
            """;

        Assert.True(TryImportOpenApi(fileContent, out var col));
        Assert.NotNull(col);
        Assert.Equal("API without components", col.Name);
        Assert.Null(col.CollectionScopedAuth);
        Assert.Empty(col.CollectionScopedRequestHeaders!);
        Assert.Equal("https://example.com/api", Assert.Single(Assert.Single(col.Environments).Variables).Value);
        var req = Assert.Single(col.HttpRequests);
        Assert.Equal("List items", req.Name);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/items", req.Url);
        Assert.Empty(req.Headers!);
        Assert.Null(req.Body);
    }

    [Fact]
    public static void Should_import_openapi_31_yaml_without_optional_collections()
    {
        string fileContent = """
            openapi: 3.1.1
            info:
              title: Minimal YAML API
              version: '1.0'
            paths:
              /items:
                get:
                  responses:
                    '200':
                      description: OK
            """;

        Assert.True(TryImportOpenApi(fileContent, out var col));
        Assert.NotNull(col);
        Assert.Empty(col.Environments);
        Assert.Equal("{{BaseUrl}}/items", Assert.Single(col.HttpRequests).Url);
    }

    [Fact]
    public static void Should_generate_body_from_openapi_31_schema()
    {
        string fileContent = """
            {
              "openapi": "3.1.1",
              "info": { "title": "Schema API", "version": "1.0" },
              "paths": {
                "/items": {
                  "post": {
                    "requestBody": {
                      "content": {
                        "application/json": {
                          "schema": {
                            "type": "object",
                            "properties": {
                              "kind": { "const": "item" },
                              "nullableName": { "type": ["string", "null"] },
                              "value": { "anyOf": [{ "type": "number" }, { "type": "null" }] },
                              "state": { "enum": ["active", "inactive"] },
                              "flags": { "type": "array", "items": { "type": "boolean" } },
                              "untypedItems": { "type": "array" }
                            }
                          }
                        }
                      }
                    },
                    "responses": { "200": { "description": "OK" } }
                  }
                }
              }
            }
            """;

        Assert.True(TryImportOpenApi(fileContent, out var col));
        Assert.NotNull(col);
        var body = Assert.Single(col.HttpRequests).Body;
        Assert.NotNull(body);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, body.Mode);
        Assert.Equal("application/json", body.ContentType);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"kind":"item","nullableName":"","value":0,"state":"active","flags":[false],"untypedItems":[]}"""),
            JsonNode.Parse(body.RawContent!)));
    }

    [Fact]
    public static void Should_limit_recursive_array_schema_depth()
    {
        string fileContent = """
            {
              "openapi": "3.1.1",
              "info": { "title": "Recursive array API", "version": "1.0" },
              "components": {
                "schemas": {
                  "NestedArray": { "type": "array", "items": { "$ref": "#/components/schemas/NestedArray" } }
                }
              },
              "paths": {
                "/items": {
                  "post": {
                    "requestBody": {
                      "content": { "application/json": { "schema": { "$ref": "#/components/schemas/NestedArray" } } }
                    },
                    "responses": { "200": { "description": "OK" } }
                  }
                }
              }
            }
            """;

        Assert.True(TryImportOpenApi(fileContent, out var col));
        Assert.NotNull(col);
        var body = Assert.Single(col.HttpRequests).Body;
        Assert.NotNull(body);
        Assert.Equal("[[[[[]]]]]", MinifyJsonString(body.RawContent!));
    }

    [Theory]
    [InlineData("not an OpenAPI document")]
    [InlineData("{}")]
    [InlineData("{\"openapi\":\"9.0.0\",\"info\":{\"title\":\"Unsupported\",\"version\":\"1.0\"}}")]
    public static void Should_reject_invalid_or_unsupported_documents(string fileContent)
    {
        Assert.False(TryImportOpenApi(fileContent, out var col));
        Assert.Null(col);
    }

    [Fact]
    public static void Should_import_all_requests_from_valid_openapi_without_tags_correctly()
    {
        // GIVEN
        string fileContent = ReadTestFileText("OpenAPI", "openapi_imgflip.yml");

        // WHEN AND THEN
        Assert.True(TryImportOpenApi(fileContent, out var col));

        // THEN
        Assert.NotNull(col);
        Assert.Equal("Imgflip API", col.Name);

        #region ENVIRONMENTS

        var env = Assert.Single(col.Environments);

        Assert.Equal("env1", env.Name);
        var baseUrlVar = Assert.Single(env.Variables);
        Assert.True(baseUrlVar.Enabled);
        Assert.Equal("BaseUrl", baseUrlVar.Key);
        Assert.Equal("https://api.imgflip.com", baseUrlVar.Value);

        #endregion

        #region TOTAL REQUESTS, URLS, NAMES AND HTTP METHODS

        Assert.Empty(col.Folders);
        Assert.Equal(4, col.Requests.Count);

        Assert.Equal("Get popular memes", col.HttpRequests[0].Name);
        Assert.Equal("GET", col.HttpRequests[0].HttpMethod);
        Assert.Equal("{{BaseUrl}}/get_memes", col.HttpRequests[0].Url);

        Assert.Equal("Add a caption to an Imgflip meme template", col.HttpRequests[1].Name);
        Assert.Equal("POST", col.HttpRequests[1].HttpMethod);
        Assert.Equal("{{BaseUrl}}/caption_image", col.HttpRequests[1].Url);

        Assert.Equal("Search for meme templates", col.HttpRequests[2].Name);
        Assert.Equal("POST", col.HttpRequests[2].HttpMethod);
        Assert.Equal("{{BaseUrl}}/search_memes", col.HttpRequests[2].Url);

        Assert.Equal("Create a custom meme", col.HttpRequests[3].Name);
        Assert.Equal("POST", col.HttpRequests[3].HttpMethod);
        Assert.Equal("{{BaseUrl}}/create_meme", col.HttpRequests[3].Url);

        #endregion
    }

    [Fact]
    public static void Should_import_all_requests_from_valid_openapi_with_tags_correctly()
    {
        // GIVEN
        string fileContent = ReadTestFileText("OpenAPI", "openapi_pix.yaml");

        // WHEN AND THEN
        Assert.True(TryImportOpenApi(fileContent, out var col));

        // THEN
        Assert.NotNull(col);
        Assert.Equal("API Pix", col.Name);

        Assert.NotNull(col.CollectionScopedRequestHeaders);
        Assert.Empty(col.CollectionScopedRequestHeaders);

        Assert.NotNull(col.CollectionScopedAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, col.CollectionScopedAuth.Mode);
        Assert.Equal("{{oauth2_access_token}}", col.CollectionScopedAuth.BearerToken);

        #region ENVIRONMENTS

        Assert.Equal(2, col.Environments.Count);

        Assert.Equal("Servidor de Produção", col.Environments[0].Name);
        var prodVars = col.Environments[0].Variables;
        Assert.Equal(4, prodVars.Count);

        var v = prodVars[0];
        Assert.True(v.Enabled);
        Assert.Equal("BaseUrl", v.Key);
        Assert.Equal("https://pix.example.com/api", v.Value);
        Assert.False(v.IsSecret);

        v = prodVars[1];
        Assert.True(v.Enabled);
        Assert.Equal("oauth2_client_id", v.Key);
        Assert.Equal(string.Empty, v.Value);
        Assert.True(v.IsSecret);

        v = prodVars[2];
        Assert.True(v.Enabled);
        Assert.Equal("oauth2_client_secret", v.Key);
        Assert.Equal(string.Empty, v.Value);
        Assert.True(v.IsSecret);

        v = prodVars[3];
        Assert.True(v.Enabled);
        Assert.Equal("oauth2_access_token", v.Key);
        Assert.Equal(string.Empty, v.Value);
        Assert.True(v.IsSecret);

        Assert.Equal("Servidor de Homologação", col.Environments[1].Name);
        var hmlVars = col.Environments[1].Variables;
        Assert.Equal(4, hmlVars.Count);

        v = hmlVars[0];
        Assert.True(v.Enabled);
        Assert.Equal("BaseUrl", v.Key);
        Assert.Equal("https://pix-h.example.com/api", v.Value);
        Assert.False(v.IsSecret);

        v = hmlVars[1];
        Assert.True(v.Enabled);
        Assert.Equal("oauth2_client_id", v.Key);
        Assert.Equal(string.Empty, v.Value);
        Assert.True(v.IsSecret);

        v = hmlVars[2];
        Assert.True(v.Enabled);
        Assert.Equal("oauth2_client_secret", v.Key);
        Assert.Equal(string.Empty, v.Value);
        Assert.True(v.IsSecret);

        v = hmlVars[3];
        Assert.True(v.Enabled);
        Assert.Equal("oauth2_access_token", v.Key);
        Assert.Equal(string.Empty, v.Value);
        Assert.True(v.IsSecret);

        #endregion

        #region TOTAL REQUESTS, URLS, NAMES AND HTTP METHODS

        Assert.Equal(8, col.Folders.Count);

        //------------------------
        Assert.Equal("OAuth2", col.Folders[0].Name);

        Assert.Equal("Cob", col.Folders[1].Name);

        var r = col.Folders[1].HttpRequests[0];
        Assert.Equal("Criar cobrança imediata.", r.Name);
        Assert.Equal("PUT", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cob/{{txid}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[1].HttpRequests[1];
        Assert.Equal("Revisar cobrança imediata.", r.Name);
        Assert.Equal("PATCH", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cob/{{txid}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[1].HttpRequests[2];
        Assert.Equal("Consultar cobrança imediata.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cob/{{txid}}?revisao=0", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[1].HttpRequests[3];
        Assert.Equal("Criar cobrança imediata.", r.Name);
        Assert.Equal("POST", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cob", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[1].HttpRequests[4];
        Assert.Equal("Consultar lista de cobranças imediatas.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Contains("{{BaseUrl}}/cob?inicio=", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);
        //------------------------
        Assert.Equal("CobV", col.Folders[2].Name);

        r = col.Folders[2].HttpRequests[0];
        Assert.Equal("Criar cobrança com vencimento.", r.Name);
        Assert.Equal("PUT", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cobv/{{txid}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[2].HttpRequests[1];
        Assert.Equal("Revisar cobrança com vencimento.", r.Name);
        Assert.Equal("PATCH", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cobv/{{txid}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[2].HttpRequests[2];
        Assert.Equal("Consultar cobrança com vencimento.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cobv/{{txid}}?revisao=0", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[2].HttpRequests[3];
        Assert.Equal("Consultar lista de cobranças com vencimento.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Contains("{{BaseUrl}}/cobv?inicio=", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);
        //------------------------
        Assert.Equal("LoteCobV", col.Folders[3].Name);

        r = col.Folders[3].HttpRequests[0];
        Assert.Equal("Criar/Alterar lote de cobranças com vencimento.", r.Name);
        Assert.Equal("PUT", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/lotecobv/{{id}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[3].HttpRequests[1];
        Assert.Equal("Utilizado para revisar cobranças específicas dentro de um lote de cobranças com vencimento.", r.Name);
        Assert.Equal("PATCH", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/lotecobv/{{id}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[3].HttpRequests[2];
        Assert.Equal("Consultar um lote específico de cobranças com vencimento.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/lotecobv/{{id}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[3].HttpRequests[3];
        Assert.Equal("Consultar lotes de cobranças com vencimento.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Contains("{{BaseUrl}}/lotecobv?inicio=", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);
        //------------------------
        Assert.Equal("PayloadLocation", col.Folders[4].Name);

        r = col.Folders[4].HttpRequests[0];
        Assert.Equal("Criar location do payload.", r.Name);
        Assert.Equal("POST", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/loc", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[4].HttpRequests[1];
        Assert.Equal("Consultar locations cadastradas.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Contains("{{BaseUrl}}/loc?inicio=", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[4].HttpRequests[2];
        Assert.Equal("Recuperar location do payload.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/loc/{{id}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[4].HttpRequests[3];
        Assert.Equal("Desvincular uma cobrança de uma location.", r.Name);
        Assert.Equal("DELETE", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/loc/{{id}}/txid", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);
        //------------------------
        Assert.Equal("Pix", col.Folders[5].Name);

        r = col.Folders[5].HttpRequests[0];
        Assert.Equal("Consultar Pix.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/pix/{{e2eid}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[5].HttpRequests[1];
        Assert.Equal("Consultar Pix recebidos.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Contains("{{BaseUrl}}/pix?inicio=", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[5].HttpRequests[2];
        Assert.Equal("Solicitar devolução.", r.Name);
        Assert.Equal("PUT", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/pix/{{e2eid}}/devolucao/{{id}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[5].HttpRequests[3];
        Assert.Equal("Consultar devolução.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/pix/{{e2eid}}/devolucao/{{id}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);
        //------------------------
        Assert.Equal("CobPayload", col.Folders[6].Name);

        r = col.Folders[6].HttpRequests[0];
        Assert.Equal("Recuperar o payload JSON que representa a cobrança imediata.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/{{pixUrlAccessToken}}", r.Url);
        Assert.Null(r.CustomAuth);

        r = col.Folders[6].HttpRequests[1];
        Assert.Equal("Recuperar o payload JSON que representa a cobrança com vencimento.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cobv/{{pixUrlAccessToken}}", r.Url);
        Assert.Null(r.CustomAuth);
        //------------------------
        Assert.Equal("Webhook", col.Folders[7].Name);

        r = col.Folders[7].HttpRequests[0];
        Assert.Equal("Configurar o Webhook Pix.", r.Name);
        Assert.Equal("PUT", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/webhook/{{chave}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[7].HttpRequests[1];
        Assert.Equal("Exibir informações acerca do Webhook Pix.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/webhook/{{chave}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[7].HttpRequests[2];
        Assert.Equal("Cancelar o webhook Pix.", r.Name);
        Assert.Equal("DELETE", r.HttpMethod);
        Assert.Equal("{{BaseUrl}}/webhook/{{chave}}", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);

        r = col.Folders[7].HttpRequests[3];
        Assert.Equal("Consultar webhooks cadastrados.", r.Name);
        Assert.Equal("GET", r.HttpMethod);
        Assert.Contains("{{BaseUrl}}/webhook?inicio=", r.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, r.CustomAuth);
        //------------------------

        #endregion
    }

    [Fact]
    public static void Should_read_request_with_raw_json_body_correctly()
    {
        // GIVEN
        string fileContent = ReadTestFileText("OpenAPI", "openapi_pix.yaml");

        // WHEN AND THEN
        Assert.True(TryImportOpenApi(fileContent, out var col));

        // THEN
        Assert.NotNull(col);

        #region REQUEST WITH RAW JSON BODY

        var req = col.Folders[2].HttpRequests[0];

        Assert.Equal("Criar cobrança com vencimento.", req.Name);
        Assert.Equal("PUT", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/cobv/{{txid}}", req.Url);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, req.Body.Mode);
        Assert.Equal("application/json", req.Body.ContentType);
        Assert.Equal(
            "{\"calendario\":{\"dataDeVencimento\":\"2020-12-31\",\"validadeAposVencimento\":30},\"loc\":{\"id\":789},\"devedor\":{\"logradouro\":\"Alameda Souza, Numero 80, Bairro Braz\",\"cidade\":\"Recife\",\"uf\":\"PE\",\"cep\":\"70011750\",\"cpf\":\"12345678909\",\"nome\":\"Francisco da Silva\"},\"valor\":{\"original\":\"123.45\",\"multa\":{\"modalidade\":\"2\",\"valorPerc\":\"15.00\"},\"juros\":{\"modalidade\":\"2\",\"valorPerc\":\"2.00\"},\"desconto\":{\"modalidade\":\"1\",\"descontoDataFixa\":[{\"data\":\"2020-11-30\",\"valorPerc\":\"30.00\"}]}},\"chave\":\"5f84a4c5-c5cb-4599-9f13-7eb4d419dacc\",\"solicitacaoPagador\":\"Cobrança dos serviços prestados.\"}",
            MinifyJsonString(req.Body.RawContent!));

        #endregion
    }

    [Fact]
    public static void Should_read_request_with_query_parameters_and_empty_body_correctly()
    {
        // GIVEN
        string fileContent = ReadTestFileText("OpenAPI", "openapi_petstore.json");

        // WHEN AND THEN
        Assert.True(TryImportOpenApi(fileContent, out var col));

        // THEN
        Assert.NotNull(col);

        #region REQUEST WITH QUERY PARAMETERS AND EMPTY BODY

        var req = col.Folders[1].HttpRequests[5];

        Assert.Equal("Updates a pet in the store with form data", req.Name);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/pet/{{petId}}?name=&status=", req.Url);
        Assert.Null(req.Body);

        #endregion
    }

    [Fact]
    public static void Should_read_request_with_url_encoded_body_correctly()
    {
        // GIVEN
        string fileContent = ReadTestFileText("OpenAPI", "openapi_imgflip.yml");

        // WHEN AND THEN
        Assert.True(TryImportOpenApi(fileContent, out var col));

        // THEN
        Assert.NotNull(col);

        #region REQUEST WITH URL ENCODED BODY

        var req = col.HttpRequests[1];

        Assert.Equal("Add a caption to an Imgflip meme template", req.Name);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/caption_image", req.Url);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.UrlEncoded, req.Body.Mode);
        Assert.Null(req.Body.ContentType);
        Assert.NotNull(req.Body.UrlEncodedValues);
        Assert.Equal(8, req.Body.UrlEncodedValues.Count);
        Assert.Equal(new(true, "template_id", ""), req.Body.UrlEncodedValues[0]);
        Assert.Equal(new(true, "username", ""), req.Body.UrlEncodedValues[1]);
        Assert.Equal(new(true, "password", ""), req.Body.UrlEncodedValues[2]);
        Assert.Equal(new(true, "text0", ""), req.Body.UrlEncodedValues[3]);
        Assert.Equal(new(true, "text1", ""), req.Body.UrlEncodedValues[4]);
        Assert.Equal(new(true, "font", "impact"), req.Body.UrlEncodedValues[5]);
        Assert.Equal(new(true, "max_font_size", "0"), req.Body.UrlEncodedValues[6]);
        Assert.Equal(new(true, "no_watermark", "false"), req.Body.UrlEncodedValues[7]);

        #endregion
    }

    [Fact]
    public static void Should_read_request_with_OAuth2_client_credentials_correctly()
    {
        // GIVEN
        string fileContent = ReadTestFileText("OpenAPI", "openapi_pix.yaml");

        // WHEN AND THEN
        Assert.True(TryImportOpenApi(fileContent, out var col));

        // THEN
        Assert.NotNull(col);

        #region COLLECTION WITH OAUTH2 CLIENT CREDENTIALS

        var oauth2folder = col.Folders[0];
        Assert.NotNull(oauth2folder);
        var clientCredentialsFolder = Assert.Single(oauth2folder.Folders);
        Assert.Equal("Client credentials", clientCredentialsFolder.Name);
        var req = Assert.Single(clientCredentialsFolder.HttpRequests);

        Assert.Equal("Get access token", req.Name);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("https://pix.example.com/oauth/token", req.Url);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.UrlEncoded, req.Body.Mode);
        Assert.Null(req.Body.ContentType);
        Assert.NotNull(req.Body.UrlEncodedValues);
        Assert.Equal(4, req.Body.UrlEncodedValues.Count);
        Assert.Equal(new(true, "grant_type", "client_credentials"), req.Body.UrlEncodedValues[0]);
        Assert.Equal(new(true, "client_id", "{{oauth2_client_id}}"), req.Body.UrlEncodedValues[1]);
        Assert.Equal(new(true, "client_secret", "{{oauth2_client_secret}}"), req.Body.UrlEncodedValues[2]);
        Assert.Equal(new(true, "scope", "cob.write cob.read cobv.write cobv.read lotecobv.write lotecobv.read pix.write pix.read webhook.read webhook.write payloadlocation.write payloadlocation.read"), req.Body.UrlEncodedValues[3]);
        Assert.NotNull(req.ResponseCaptures);
        Assert.Equal([
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token", null, "$.access_token"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_access_token_expires_in", null, "$.expires_in"),
            new(PororocaHttpResponseValueCaptureType.Body, "oauth2_refresh_token", null, "$.refresh_token"),
        ], req.ResponseCaptures);


        #endregion
    }

    [Fact]
    public static void Should_read_request_with_API_keys_correctly()
    {
        // GIVEN
        // the original document had API keys in query params,
        // changed to headers for this test
        string fileContent = ReadTestFileText("OpenAPI", "SHODAN-OPENAPI.json");

        // WHEN AND THEN
        Assert.True(TryImportOpenApi(fileContent, out var col));

        // THEN
        Assert.NotNull(col);
        Assert.Equal("Shodan REST API Documentation", col.Name);

        Assert.NotNull(col.CollectionScopedRequestHeaders);
        Assert.Equal(new(true, "SHODAN-KEY", "{{SHODAN-KEY}}"), Assert.Single(col.CollectionScopedRequestHeaders));

        var env = Assert.Single(col.Environments);

        Assert.Equal("env1", env.Name);

        Assert.Equal(2, env.Variables.Count);
        var v = env.Variables[0];
        Assert.True(v.Enabled);
        Assert.Equal("BaseUrl", v.Key);
        Assert.Equal("https://api.shodan.io", v.Value);
        Assert.False(v.IsSecret);

        v = env.Variables[1];
        Assert.True(v.Enabled);
        Assert.Equal("SHODAN-KEY", v.Key);
        Assert.Equal(string.Empty, v.Value);
        Assert.True(v.IsSecret);

        var folder = Assert.Single(col.Folders);
        Assert.Equal("Search Methods", folder.Name);
        var req = Assert.Single(folder.HttpRequests);

        Assert.Equal("req", req.Name);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseUrl}}/shodan/host/{{ip}}?history=false&minify=false", req.Url);
        Assert.NotNull(req.Headers);
        Assert.Empty(req.Headers);
        Assert.Null(req.Body);
    }
}
