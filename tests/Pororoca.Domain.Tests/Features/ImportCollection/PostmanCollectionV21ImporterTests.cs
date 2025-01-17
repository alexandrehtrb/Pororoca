using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Postman;
using Xunit;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;

namespace Pororoca.Domain.Tests.Features.ImportCollection;

public static class PostmanCollectionV21ImporterTests
{
    private static readonly Guid testGuid = Guid.NewGuid();
    private const string testName = "MyCollection";

    #region FULL COLLECTION JSON IMPORT

    [Fact]
    public static void Should_import_valid_full_postman_collection_correctly()
    {
        // GIVEN
        string json = ReadTestFileText("VirusTotal API.postman_collection.json");

        // WHEN AND THEN
        Assert.True(TryImportPostmanCollection(json, out var col));

        // THEN
        Assert.NotNull(col);
        Assert.Equal("VirusTotal API", col.Name);
        Assert.Equal([
            new(true, "BaseURL", "https://www.virustotal.com/api/v3", false),
            new(true, "API-Key", string.Empty, false),
            new(true, "UploadFileURL", string.Empty, false)],
            col.Variables);

        Assert.Null(col.CollectionScopedAuth);

        Assert.Empty(col.Environments);

        var dir = Assert.Single(col.Folders);
        Assert.Equal("Upload files", dir.Name);
        Assert.Equal(2, dir.HttpRequests.Count);

        var req = dir.HttpRequests[0];
        Assert.Equal("Get URL for upload", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseURL}}/files/upload_url", req.Url);
        Assert.NotNull(req.Headers);
        Assert.Empty(req.Headers);
        Assert.Null(req.Body);
        Assert.Null(req.CustomAuth);

        req = dir.HttpRequests[1];
        Assert.Equal("Upload large file", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("{{UploadFileURL}}", req.Url);
        Assert.NotNull(req.Headers);
        Assert.Empty(req.Headers);
        Assert.NotNull(req.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, req.Body.Mode);
        Assert.Equal("application/json", req.Body.ContentType);
        Assert.Equal("{\"text\":\"this endpoint actually uses form data\"}", req.Body.RawContent);
        Assert.Null(req.CustomAuth);

        req = Assert.Single(col.HttpRequests);
        Assert.Equal("Get file analysis", req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("GET", req.HttpMethod);
        Assert.Equal("{{BaseURL}}/analyses/{{AnalysisId}}", req.Url);
        Assert.NotNull(req.Headers);
        Assert.Empty(req.Headers);
        Assert.Null(req.Body);
        Assert.Null(req.CustomAuth);
    }

    #endregion

    #region INVALID COLLECTION

    [Fact]
    public static void Should_not_convert_invalid_postman_collection()
    {
        string json = "{\"id\": \"8b34e2c4-3384-4ebd-996e-24c0e63ee256\"}";

        Assert.False(TryImportPostmanCollection(json, out var col));
        Assert.Null(col);
    }

    #endregion

    #region REQUEST HEADERS

    [Fact]
    public static void Should_convert_postman_req_header_to_pororoca_variable_correctly()
    {
        // GIVEN
        PostmanVariable p1 = new() { Disabled = null, Key = "Key1", Value = "Value1" };
        PostmanVariable p2 = new() { Disabled = true, Key = "Key2", Value = "Value2" };

        // WHEN
        var hdrs = ConvertToPororocaHeaders([p1, p2]);

        // THEN
        Assert.NotNull(hdrs);
        Assert.Equal(2, hdrs.Count);

        var h1 = hdrs[0];
        Assert.True(h1.Enabled);
        Assert.Equal("Key1", h1.Key);
        Assert.Equal("Value1", h1.Value);

        var h2 = hdrs[1];
        Assert.False(h2.Enabled);
        Assert.Equal("Key2", h2.Key);
        Assert.Equal("Value2", h2.Value);
    }

    #endregion

    #region REQUEST BODY

    [Fact]
    public static void Should_convert_postman_req_none_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        PostmanRequestBody? postmanBody = null;

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.Null(reqBody);
    }

    [Theory]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForJson, "application/json; charset=utf-8")]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForHtml, "text/html")]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForXml, "text/xml")]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForText, "text/plain")]
    public static void Should_convert_postman_req_raw_json_body_to_pororoca_req_body_correctly_taking_content_type_from_header(string expectedPororocaContentType, string contentTypeFromHeader)
    {
        // GIVEN
        PostmanRequestBody? postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Raw,
            Raw = "[]"
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, contentTypeFromHeader);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, reqBody!.Mode);
        Assert.Equal("[]", reqBody.RawContent);
        Assert.Equal(expectedPororocaContentType, reqBody.ContentType);
    }

    [Theory]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForJson, "json")]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForJavascript, "javascript")]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForHtml, "html")]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForXml, "xml")]
    [InlineData(MimeTypesDetector.DefaultMimeTypeForText, "text")]
    public static void Should_convert_postman_req_raw_json_body_to_pororoca_req_body_correctly(string expectedPororocaContentType, string postmanRawBodyLang)
    {
        // GIVEN
        PostmanRequestBody? postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Raw,
            Options = new() { Raw = new() { Language = postmanRawBodyLang } },
            Raw = "[]"
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, reqBody!.Mode);
        Assert.Equal("[]", reqBody.RawContent);
        Assert.Equal(expectedPororocaContentType, reqBody.ContentType);
    }

    [Fact]
    public static void Should_convert_postman_req_raw_text_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        PostmanRequestBody? postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Raw,
            Options = new() { Raw = new() { Language = "text" } },
            Raw = "aeiou"
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, reqBody!.Mode);
        Assert.Equal("aeiou", reqBody.RawContent);
        Assert.Equal(MimeTypesDetector.DefaultMimeTypeForText, reqBody.ContentType);
    }

    [Fact]
    public static void Should_convert_postman_req_raw_xml_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        PostmanRequestBody postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Raw,
            Options = new() { Raw = new() { Language = "xml" } },
            Raw = "<a k=\"1\"/>"
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, reqBody!.Mode);
        Assert.Equal("<a k=\"1\"/>", reqBody.RawContent);
        Assert.Equal(MimeTypesDetector.DefaultMimeTypeForXml, reqBody.ContentType);
    }

    [Fact]
    public static void Should_convert_postman_req_url_encoded_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        PostmanVariable p1 = new() { Disabled = null, Key = "Key1", Value = "Value1" };
        PostmanVariable p2 = new() { Disabled = true, Key = "Key2", Value = "Value2" };
        PostmanRequestBody postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Urlencoded,
            Urlencoded = [p1, p2]
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.UrlEncoded, reqBody!.Mode);

        Assert.Equal(2, reqBody.UrlEncodedValues!.Count);

        var h1 = reqBody.UrlEncodedValues[0];
        Assert.True(h1.Enabled);
        Assert.Equal("Key1", h1.Key);
        Assert.Equal("Value1", h1.Value);

        var h2 = reqBody.UrlEncodedValues[1];
        Assert.False(h2.Enabled);
        Assert.Equal("Key2", h2.Key);
        Assert.Equal("Value2", h2.Value);
    }

    [Fact]
    public static void Should_convert_postman_req_file_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        PostmanRequestBody postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.File,
            File = new() { Src = @"/C:/MyFolder/image.png" }
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.File, reqBody!.Mode);
        Assert.Equal("image/png", reqBody.ContentType);
        Assert.Equal(@"/C:/MyFolder/image.png", reqBody.FileSrcPath);
    }

    [Fact]
    public static void Should_convert_postman_req_form_data_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        PostmanRequestBodyFormDataParam p1t = new()
        {
            Key = "Key1Text",
            Value = "Value1Text",
            ContentType = "text/plain",
            Type = PostmanRequestBodyFormDataParamType.Text
        };

        PostmanRequestBodyFormDataParam p2t = new()
        {
            Key = "Key2Text",
            Value = "Value2Text",
            ContentType = "application/json; charset=utf-8",
            Type = PostmanRequestBodyFormDataParamType.Text,
            Disabled = true
        };

        PostmanRequestBodyFormDataParam p1f = new()
        {
            Key = "Key1File",
            Src = @"C:\Pasta1\arq.txt",
            ContentType = null,
            Type = PostmanRequestBodyFormDataParamType.File
        };

        PostmanRequestBodyFormDataParam p2f = new()
        {
            Key = "Key2File",
            Src = JsonDocument.Parse("[\"C:/Pasta1/arq2.jpg\"]").RootElement,
            ContentType = "image/jpeg",
            Type = PostmanRequestBodyFormDataParamType.File,
            Disabled = true
        };

        PostmanRequestBodyFormDataParam p1d = new()
        {
            Key = "Key1Default",
            Value = "Value1Default",
            ContentType = null,
            Type = PostmanRequestBodyFormDataParamType.Default,
            Disabled = true
        };

        PostmanRequestBody postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Formdata,
            Formdata = [p1t, p2t, p1f, p2f, p1d]
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.FormData, reqBody!.Mode);

        Assert.Equal(5, reqBody.FormDataValues!.Count);

        var f1t = reqBody.FormDataValues[0];
        Assert.True(f1t.Enabled);
        Assert.Equal(PororocaHttpRequestFormDataParamType.Text, f1t.Type);
        Assert.Equal("Key1Text", f1t.Key);
        Assert.Equal("Value1Text", f1t.TextValue);
        Assert.Equal("text/plain", f1t.ContentType);

        var f2t = reqBody.FormDataValues[1];
        Assert.False(f2t.Enabled);
        Assert.Equal(PororocaHttpRequestFormDataParamType.Text, f2t.Type);
        Assert.Equal("Key2Text", f2t.Key);
        Assert.Equal("Value2Text", f2t.TextValue);
        Assert.Equal("application/json; charset=utf-8", f2t.ContentType);

        var f1f = reqBody.FormDataValues[2];
        Assert.True(f1f.Enabled);
        Assert.Equal(PororocaHttpRequestFormDataParamType.File, f1f.Type);
        Assert.Equal("Key1File", f1f.Key);
        Assert.Equal(@"C:\Pasta1\arq.txt", f1f.FileSrcPath);
        Assert.Equal("application/octet-stream", f1f.ContentType);

        var f2f = reqBody.FormDataValues[3];
        Assert.False(f2f.Enabled);
        Assert.Equal(PororocaHttpRequestFormDataParamType.File, f2f.Type);
        Assert.Equal("Key2File", f2f.Key);
        Assert.Equal(@"C:/Pasta1/arq2.jpg", f2f.FileSrcPath);
        Assert.Equal("image/jpeg", f2f.ContentType);

        var f1d = reqBody.FormDataValues[4];
        Assert.False(f1d.Enabled);
        Assert.Equal(PororocaHttpRequestFormDataParamType.Text, f1d.Type);
        Assert.Equal("Key1Default", f1d.Key);
        Assert.Equal("Value1Default", f1d.TextValue);
        Assert.Equal("text/plain", f1d.ContentType);
    }

    [Fact]
    public static void Should_convert_postman_req_graphql_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        const string qry = "query allFruits { fruits { fruit_name } }";
        const string variables = "{\"id\":{{CocoId}}}";
        PostmanRequestBody postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Graphql,
            Graphql = new() { Query = qry, Variables = variables }
        };

        // WHEN
        var reqBody = ConvertToPororocaHttpRequestBody(postmanBody, null);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaHttpRequestBodyMode.GraphQl, reqBody!.Mode);
        Assert.NotNull(reqBody.GraphQlValues);
        Assert.Equal(qry, reqBody.GraphQlValues!.Query);
        Assert.Equal(variables, reqBody.GraphQlValues!.Variables);
    }

    #endregion

    #region REQUEST AUTH

    [Fact]
    public static void Should_convert_postman_req_none_auth_to_pororoca_req_auth_correctly()
    {
        // GIVEN
        PostmanAuth? postmanAuth = null;

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.Null(reqAuth);
    }

    [Fact]
    public static void Should_convert_postman_req_basic_auth_to_pororoca_req_auth_correctly_basic_is_postmanvararray()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.basic,
            Basic = new PostmanVariable[]
            {
                new() { Key = "username", Value = "usr", Type = "string" },
                new() { Key = "password", Value = "pwd", Type = "string" }
            }
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Basic, reqAuth!.Mode);
        Assert.Equal("usr", reqAuth.BasicAuthLogin);
        Assert.Equal("pwd", reqAuth.BasicAuthPassword);
        Assert.Null(reqAuth.BearerToken);
    }

    [Fact]
    public static void Should_convert_postman_req_basic_auth_to_pororoca_req_auth_correctly_basic_is_postmanauthbasicjsonobj()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.basic,
            Basic = JsonDocument.Parse("{\"username\":\"usr\",\"password\":\"pwd\"}").RootElement
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Basic, reqAuth!.Mode);
        Assert.Equal("usr", reqAuth.BasicAuthLogin);
        Assert.Equal("pwd", reqAuth.BasicAuthPassword);
        Assert.Null(reqAuth.BearerToken);
    }

    [Fact]
    public static void Should_convert_postman_req_basic_auth_to_pororoca_req_auth_correctly_basic_is_postmanauthbasicjsonarray()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.basic,
            Basic = JsonDocument.Parse("[ {\"key\":\"username\", \"value\":\"usr\"}, {\"key\":\"password\",\"value\":\"pwd\"} ]").RootElement
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Basic, reqAuth!.Mode);
        Assert.Equal("usr", reqAuth.BasicAuthLogin);
        Assert.Equal("pwd", reqAuth.BasicAuthPassword);
        Assert.Null(reqAuth.BearerToken);
    }

    [Fact]
    public static void Should_convert_postman_req_bearer_auth_to_pororoca_req_auth_correctly_bearer_is_postmanvararray()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.bearer,
            Bearer = new PostmanVariable[]
            {
                new() { Key = "token", Value = "tkn", Type = "string" }
            }
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, reqAuth!.Mode);
        Assert.Null(reqAuth.BasicAuthLogin);
        Assert.Null(reqAuth.BasicAuthPassword);
        Assert.Equal("tkn", reqAuth.BearerToken);
    }

    [Fact]
    public static void Should_convert_postman_req_bearer_auth_to_pororoca_req_auth_correctly_bearer_is_postmanauthbearerjsonobj()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.bearer,
            Bearer = JsonDocument.Parse("{\"token\":\"tkn\"}").RootElement
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, reqAuth!.Mode);
        Assert.Null(reqAuth.BasicAuthLogin);
        Assert.Null(reqAuth.BasicAuthPassword);
        Assert.Equal("tkn", reqAuth.BearerToken);
    }

    [Fact]
    public static void Should_convert_postman_req_bearer_auth_to_pororoca_req_auth_correctly_bearer_is_postmanauthbearerjsonarray()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.bearer,
            Bearer = JsonDocument.Parse("[ {\"key\":\"token\",\"value\":\"tkn\"} ]").RootElement
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Bearer, reqAuth!.Mode);
        Assert.Null(reqAuth.BasicAuthLogin);
        Assert.Null(reqAuth.BasicAuthPassword);
        Assert.Equal("tkn", reqAuth.BearerToken);
    }

    [Fact]
    public static void Should_convert_postman_req_ntlm_auth_to_pororoca_req_auth_correctly_ntlm_is_postmanvararray()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.ntlm,
            Ntlm = new PostmanVariable[]
            {
                new() { Key = "username", Value = "usr", Type = "string" },
                new() { Key = "password", Value = "pwd", Type = "string" },
                new() { Key = "domain", Value = "dom", Type = "string" }
            }
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Windows, reqAuth!.Mode);
        Assert.Null(reqAuth.BasicAuthLogin);
        Assert.Null(reqAuth.BasicAuthPassword);
        Assert.Null(reqAuth.BearerToken);
        Assert.Null(reqAuth.ClientCertificate);
        Assert.NotNull(reqAuth.Windows);
        Assert.False(reqAuth.Windows.UseCurrentUser);
        Assert.Equal("usr", reqAuth.Windows.Login);
        Assert.Equal("pwd", reqAuth.Windows.Password);
        Assert.Equal("dom", reqAuth.Windows.Domain);
    }

    [Fact]
    public static void Should_convert_postman_req_ntlm_auth_to_pororoca_req_auth_correctly_ntlm_is_postmanauthntlmjsonobj()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.ntlm,
            Ntlm = JsonDocument.Parse("{\"username\":\"usr\",\"password\":\"pwd\",\"domain\":\"dom\"}").RootElement
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Windows, reqAuth!.Mode);
        Assert.Null(reqAuth.BasicAuthLogin);
        Assert.Null(reqAuth.BasicAuthPassword);
        Assert.Null(reqAuth.BearerToken);
        Assert.Null(reqAuth.ClientCertificate);
        Assert.NotNull(reqAuth.Windows);
        Assert.False(reqAuth.Windows.UseCurrentUser);
        Assert.Equal("usr", reqAuth.Windows.Login);
        Assert.Equal("pwd", reqAuth.Windows.Password);
        Assert.Equal("dom", reqAuth.Windows.Domain);
    }

    [Fact]
    public static void Should_convert_postman_req_ntlm_auth_to_pororoca_req_auth_correctly_ntlm_is_postmanauthntlmjsonarray()
    {
        // GIVEN
        PostmanAuth? postmanAuth = new()
        {
            Type = PostmanAuthType.ntlm,
            Ntlm = JsonDocument.Parse("[ {\"key\":\"username\",\"value\":\"usr\"},{\"key\":\"password\",\"value\":\"pwd\"},{\"key\":\"domain\",\"value\":\"dom\"} ]").RootElement
        };

        // WHEN
        var reqAuth = ConvertToPororocaAuth(postmanAuth);

        // THEN
        Assert.NotNull(reqAuth);
        Assert.Equal(PororocaRequestAuthMode.Windows, reqAuth!.Mode);
        Assert.Null(reqAuth.BasicAuthLogin);
        Assert.Null(reqAuth.BasicAuthPassword);
        Assert.Null(reqAuth.BearerToken);
        Assert.Null(reqAuth.ClientCertificate);
        Assert.NotNull(reqAuth.Windows);
        Assert.False(reqAuth.Windows.UseCurrentUser);
        Assert.Equal("usr", reqAuth.Windows.Login);
        Assert.Equal("pwd", reqAuth.Windows.Password);
        Assert.Equal("dom", reqAuth.Windows.Domain);
    }

    #endregion

    #region REQUEST URL

    [Fact]
    public static void Should_convert_postman_req_url_correctly_when_url_is_jsonobject()
    {
        // GIVEN
        var postmanRequest = CreateTestRequestWithAuth();
        postmanRequest.Url = JsonDocument.Parse("{\"raw\": \"http://www.abc.com.br\"}").RootElement;

        // WHEN
        var req = ConvertToPororocaHttpRequest(string.Empty, postmanRequest);

        // THEN
        Assert.NotNull(req);
        Assert.Equal("http://www.abc.com.br", req.Url);
    }

    [Fact]
    public static void Should_convert_postman_req_url_correctly_when_url_is_jsonstring()
    {
        // GIVEN
        var postmanRequest = CreateTestRequestWithAuth();
        postmanRequest.Url = JsonDocument.Parse("\"http://www.abc.com.br\"").RootElement;

        // WHEN
        var req = ConvertToPororocaHttpRequest(string.Empty, postmanRequest);

        // THEN
        Assert.NotNull(req);
        Assert.Equal("http://www.abc.com.br", req.Url);
    }

    [Fact]
    public static void Should_convert_postman_req_url_correctly_when_url_is_postmanrequrl_parsedobj()
    {
        // GIVEN
        var postmanRequest = CreateTestRequestWithAuth();
        postmanRequest.Url = new PostmanRequestUrl() { Raw = "http://www.abc.com.br" };

        // WHEN
        var req = ConvertToPororocaHttpRequest(string.Empty, postmanRequest);

        // THEN
        Assert.NotNull(req);
        Assert.Equal("http://www.abc.com.br", req.Url);
    }

    #endregion

    #region REQUEST

    private static PostmanRequest CreateTestRequestWithAuth() =>
        new()
        {
            Auth = new()
            {
                Type = PostmanAuthType.basic,
                Basic = new PostmanVariable[]
                {
                    new() { Key = "username", Value = "usr", Type = "string" },
                    new() { Key = "password", Value = "pwd", Type = "string" }
                }
            },
            Method = "POST",
            Header =
            [
                new() { Disabled = null, Key = "Key1", Value = "Value1" },
                new() { Disabled = true, Key = "Key2", Value = "Value2" }
            ],
            Body = new()
            {
                Mode = PostmanRequestBodyMode.Raw,
                Options = new() { Raw = new() { Language = "json" } },
                Raw = "[]"
            },
            Url = new PostmanRequestUrl() { Raw = "http://www.abc.com.br" }
        };

    [Fact]
    public static void Should_convert_postman_req_with_no_inherited_auth_to_pororoca_req_correctly()
    {
        // GIVEN
        string reqName = "MyRequest";
        var postmanRequest = CreateTestRequestWithAuth();

        // WHEN
        var req = ConvertToPororocaHttpRequest(reqName, postmanRequest);

        // THEN
        Assert.NotNull(req);
        Assert.Equal(reqName, req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("http://www.abc.com.br", req.Url);

        Assert.Equal(PororocaRequestAuthMode.Basic, req.CustomAuth!.Mode);
        Assert.Equal("usr", req.CustomAuth.BasicAuthLogin);
        Assert.Equal("pwd", req.CustomAuth.BasicAuthPassword);

        Assert.Equal(2, req.Headers!.Count);
        var hdr1 = req.Headers[0];
        Assert.True(hdr1.Enabled);
        Assert.Equal("Key1", hdr1.Key);
        Assert.Equal("Value1", hdr1.Value);
        var hdr2 = req.Headers[1];
        Assert.False(hdr2.Enabled);
        Assert.Equal("Key2", hdr2.Key);
        Assert.Equal("Value2", hdr2.Value);

        Assert.Equal(PororocaHttpRequestBodyMode.Raw, req.Body!.Mode);
        Assert.Equal("[]", req.Body.RawContent);
        Assert.Equal(MimeTypesDetector.DefaultMimeTypeForJson, req.Body.ContentType);
    }

    [Fact]
    public static void Should_convert_postman_req_with_inherited_auth_to_pororoca_req_correctly()
    {
        // GIVEN
        string reqName = "MyRequest";
        var postmanRequest = CreateTestRequestWithAuth();
        postmanRequest.Auth = null;

        // WHEN
        var req = ConvertToPororocaHttpRequest(reqName, postmanRequest);

        // THEN
        Assert.NotNull(req);
        Assert.Equal(reqName, req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("http://www.abc.com.br", req.Url);

        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.InheritFromCollection, req.CustomAuth!.Mode);

        Assert.Equal(2, req.Headers!.Count);
        var hdr1 = req.Headers[0];
        Assert.True(hdr1.Enabled);
        Assert.Equal("Key1", hdr1.Key);
        Assert.Equal("Value1", hdr1.Value);
        var hdr2 = req.Headers[1];
        Assert.False(hdr2.Enabled);
        Assert.Equal("Key2", hdr2.Key);
        Assert.Equal("Value2", hdr2.Value);

        Assert.Equal(PororocaHttpRequestBodyMode.Raw, req.Body!.Mode);
        Assert.Equal("[]", req.Body.RawContent);
        Assert.Equal(MimeTypesDetector.DefaultMimeTypeForJson, req.Body.ContentType);
    }

    #endregion

    #region COLLECTION, FOLDERS AND COLLECTION VARIABLES

    [Fact]
    public static void Should_convert_postman_collection_to_pororoca_collection_correctly()
    {
        // GIVEN
        var postmanCollection = CreateTestCollection();

        // WHEN
        Assert.True(TryConvertToPororocaCollection(postmanCollection, out var pororocaCollection));

        // THEN
        AssertConvertedCollection(pororocaCollection);
    }

    private static PostmanCollectionV21 CreateTestCollection() =>
        new()
        {
            Info = new()
            {
                Id = testGuid,
                Name = testName,
                Schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
            },
            Variable =
            [
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2", Disabled = true }
            ],
            Auth = new()
            {
                Type = PostmanAuthType.bearer,
                Bearer = new PostmanVariable[]
                {
                    new() { Key = "token", Value = "tkn", Type = "string" }
                }
            },
            Items =
            [
                new()
                {
                    Name = "Folder1",
                    Items =
                    [
                        new()
                        {
                            Name = "Req1",
                            Request = new()
                            {
                                Method = "GET",
                                Header = Array.Empty<PostmanVariable>(),
                                Url = new PostmanRequestUrl() { Raw = "http://www.abc.com.br" }
                            }
                        }
                    ]
                },
                new()
                {
                    Name = "Req2",
                    Request = new()
                    {
                        Method = "GET",
                        Header = Array.Empty<PostmanVariable>(),
                        Url = new PostmanRequestUrl() { Raw = "http://www.def.com.br" }
                    }
                }
            ]
        };

    private static void AssertConvertedCollection(PororocaCollection? pororocaCollection)
    {
        Assert.NotNull(pororocaCollection);
        Assert.NotEqual(testGuid, pororocaCollection!.Id);
        Assert.Equal(testName, pororocaCollection.Name);
        Assert.Single(pororocaCollection.Folders);
        Assert.Single(pororocaCollection.Requests);
        Assert.Single(pororocaCollection.HttpRequests);

        var folder1 = pororocaCollection.Folders[0];
        Assert.Equal("Folder1", folder1.Name);
        Assert.Empty(folder1.Folders);
        Assert.Single(folder1.Requests);

        var req1 = folder1.HttpRequests[0];
        Assert.Equal("Req1", req1.Name);
        Assert.Equal("GET", req1.HttpMethod);
        Assert.Equal("http://www.abc.com.br", req1.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req1.CustomAuth);

        var req2 = pororocaCollection.HttpRequests[0];
        Assert.Equal("Req2", req2.Name);
        Assert.Equal("GET", req2.HttpMethod);
        Assert.Equal("http://www.def.com.br", req2.Url);
        Assert.Equal(PororocaRequestAuth.InheritedFromCollection, req2.CustomAuth);

        Assert.Equal(2, pororocaCollection.Variables.Count);
        var var1 = pororocaCollection.Variables[0];
        Assert.True(var1.Enabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);
        var var2 = pororocaCollection.Variables[1];
        Assert.False(var2.Enabled);
        Assert.Equal("Key2", var2.Key);
        Assert.Equal("Value2", var2.Value);
    }

    #endregion

}