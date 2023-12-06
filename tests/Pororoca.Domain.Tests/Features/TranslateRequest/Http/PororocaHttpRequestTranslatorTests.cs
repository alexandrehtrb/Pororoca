using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.TranslateRequest;
using Xunit;
using static Pororoca.Domain.Features.TranslateRequest.Http.PororocaHttpRequestTranslator;

namespace Pororoca.Domain.Tests.Features.TranslateRequest.Http;

public static class PororocaHttpRequestTranslatorTests
{
    #region TRANSLATE REQUEST

    [Fact]
    public static void Should_not_translate_request_if_it_has_invalid_URL()
    {
        // GIVEN
        PororocaVariable[] effectiveVars = [];
        PororocaHttpRequest unresolvedReq = new(string.Empty)
        {
            HttpVersion = 3.0m,
            HttpMethod = "PUT",
            Url = "{{BaseUrl}}/index.html",
            Headers = null,
            Body = null,
            CustomAuth = null,
            ResponseCaptures = null
        };

        // WHEN, THEN
        Assert.False(TryTranslateRequest(effectiveVars, null, unresolvedReq, out var resolvedReq, out var reqMsg, out string? errorCode));
        Assert.Null(resolvedReq);
        Assert.Null(reqMsg);
        Assert.Equal(TranslateRequestErrors.InvalidUrl, errorCode);
    }

    [Fact]
    public static async Task Should_translate_valid_HTTP_request()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "BaseUrl", "http://www.pudim.com.br", true),
            new(true, "MyHeaderValue", "Value1234", false),
            new(true, "K1", "17", false),
            new(true, "BasicAuthLogin", "usr", false),
            new(true, "BasicAuthPassword", "pwd", true)
        ];
        PororocaHttpRequestBody unresolvedBody = new();
        unresolvedBody.SetRawContent("{\"id\":{{K1}}}", "application/json");
        var unresolvedAuth = PororocaRequestAuth.MakeBasicAuth("{{BasicAuthLogin}}", "{{BasicAuthPassword}}");
        PororocaHttpRequest unresolvedReq = new(string.Empty)
        {
            HttpVersion = 3.0m,
            HttpMethod = "PUT",
            Url = "{{BaseUrl}}/index.html",
            Headers = [ new(true, "MyHeader", "{{MyHeaderValue}}") ],
            Body = unresolvedBody,
            CustomAuth = unresolvedAuth,
            ResponseCaptures = [ new(PororocaHttpResponseValueCaptureType.Body, "TARGET_VAR", null, "$.id") ]
        };

        // WHEN, THEN
        Assert.True(TryTranslateRequest(effectiveVars, null, unresolvedReq, out var resolvedReq, out var reqMsg, out string? errorCode));
        Assert.NotNull(resolvedReq);
        Assert.NotNull(reqMsg);
        Assert.Equal(3, reqMsg.Version.Major);
        Assert.Equal(0, reqMsg.Version.Minor);
        Assert.Equal("PUT", reqMsg.Method.ToString());
        Assert.Equal("http://www.pudim.com.br/index.html", reqMsg.RequestUri!.ToString());
        #pragma warning disable xUnit2012
        Assert.True(reqMsg.Headers.Any(x => x.Key == "MyHeader" && x.Value.Contains("Value1234")));
        Assert.True(reqMsg.Headers.Any(x => x.Key == "Authorization" && x.Value.Contains("Basic dXNyOnB3ZA==")));
        #pragma warning restore xUnit2012

        Assert.True(reqMsg.Content is StringContent);
        Assert.NotNull(reqMsg.Content.Headers.ContentType);
        Assert.Equal("application/json", reqMsg.Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", reqMsg.Content.Headers.ContentType!.CharSet);
        string? contentText = await reqMsg.Content.ReadAsStringAsync();
        Assert.Equal("{\"id\":17}", contentText);
    }

    #endregion

    #region RESOLUTION / REPLACE VARIABLE TEMPLATES

    [Fact]
    public static void Should_resolve_HTTP_request()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "BaseUrl", "http://www.pudim.com.br", true),
            new(true, "MyHeaderValue", "Value1234", false),
            new(true, "K1", "17", false),
            new(true, "BasicAuthLogin", "login", false),
            new(true, "BasicAuthPassword", "password", true)
        ];
        PororocaHttpRequestBody unresolvedBody = new();
        unresolvedBody.SetRawContent("{\"id\":{{K1}}}", "application/json");
        var unresolvedAuth = PororocaRequestAuth.MakeBasicAuth("{{BasicAuthLogin}}", "{{BasicAuthPassword}}");
        PororocaHttpRequest unresolvedReq = new(string.Empty)
        {
            HttpVersion = 3.0m,
            HttpMethod = "PUT",
            Url = "{{BaseUrl}}/index.html",
            Headers = [ new(true, "MyHeader", "{{MyHeaderValue}}") ],
            Body = unresolvedBody,
            CustomAuth = unresolvedAuth,
            ResponseCaptures = [ new(PororocaHttpResponseValueCaptureType.Body, "TARGET_VAR", null, "$.id") ]
        };

        // WHEN
        var resolvedReq = ResolveRequest(effectiveVars, null, unresolvedReq);

        // THEN
        Assert.NotNull(resolvedReq);
        Assert.Equal(3.0m, resolvedReq.HttpVersion);
        Assert.Equal("PUT", resolvedReq.HttpMethod);
        Assert.Equal("http://www.pudim.com.br/index.html", resolvedReq.Url);
        Assert.Equal([new(true, "MyHeader", "Value1234")], resolvedReq.Headers);
        Assert.NotNull(resolvedReq.Body);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, resolvedReq.Body.Mode);
        Assert.Equal("application/json", resolvedReq.Body.ContentType);
        Assert.Equal("{\"id\":17}", resolvedReq.Body.RawContent);
        Assert.NotNull(resolvedReq.CustomAuth);
        Assert.Equal(PororocaRequestAuthMode.Basic, resolvedReq.CustomAuth.Mode);
        Assert.Equal("login", resolvedReq.CustomAuth.BasicAuthLogin);
        Assert.Equal("password", resolvedReq.CustomAuth.BasicAuthPassword);
        Assert.Equal([ new(PororocaHttpResponseValueCaptureType.Body, "TARGET_VAR", null, "$.id") ], resolvedReq.ResponseCaptures);
    }

    [Fact]
    public static void Should_resolve_empty_body_to_null() =>
        // GIVEN, WHEN AND THEN
        Assert.Null(ResolveRequestBody([], null));

    [Fact]
    public static void Should_resolve_raw_body()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "K1", "4577", true)
        ];
        PororocaHttpRequestBody unresolvedBody = new();
        unresolvedBody.SetRawContent("{\"id\":{{K1}}}", "application/json");

        // WHEN
        var resolvedBody = ResolveRequestBody(effectiveVars, unresolvedBody);

        // THEN
        Assert.NotNull(resolvedBody);
        Assert.Equal(PororocaHttpRequestBodyMode.Raw, resolvedBody.Mode);
        Assert.Equal("application/json", resolvedBody.ContentType);
        Assert.Equal("{\"id\":4577}", resolvedBody.RawContent);
    }

    [Fact]
    public static void Should_resolve_file_body()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "FolderPath", "C:\\MYFILES", true),
            new(true, "FileName", "file.txt", true)
        ];
        PororocaHttpRequestBody unresolvedBody = new();
        unresolvedBody.SetFileContent("{{FolderPath}}\\{{FileName}}", "text/plain");

        // WHEN
        var resolvedBody = ResolveRequestBody(effectiveVars, unresolvedBody);

        // THEN
        Assert.NotNull(resolvedBody);
        Assert.Equal(PororocaHttpRequestBodyMode.File, resolvedBody.Mode);
        Assert.Equal("text/plain", resolvedBody.ContentType);
        Assert.Equal("C:\\MYFILES\\file.txt", resolvedBody.FileSrcPath);
    }

    [Fact]
    public static void Should_resolve_form_URL_encoded_body()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "K1", "4577", true),
            new(true, "K3", "4354", true),
        ];
        PororocaKeyValueParam[] unresolvedUrlEncodedParams =
        [
            new(false, "key0", "v0"),
            new(true, "key1", "{{K1}}"),
            new(true, "{{K3}}", "value3")
        ];
        PororocaHttpRequestBody unresolvedBody = new();
        unresolvedBody.SetUrlEncodedContent(unresolvedUrlEncodedParams);

        // WHEN
        var resolvedBody = ResolveRequestBody(effectiveVars, unresolvedBody);

        // THEN
        Assert.NotNull(resolvedBody);
        Assert.Equal(PororocaHttpRequestBodyMode.UrlEncoded, resolvedBody.Mode);
        Assert.Null(resolvedBody.ContentType);
        Assert.Equal([
            new(true, "key1", "4577"),
            new(true, "4354", "value3")],
            resolvedBody.UrlEncodedValues);
    }

    [Fact]
    public static void Should_resolve_multipart_form_data_encoded_body()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "K1", "4577", true),
            new(true, "K3", "4354", true),
            new(true, "FolderPath", "C:\\MYFILES", true),
            new(true, "FileName", "file.xml", true)
        ];
        var p0 = PororocaHttpRequestFormDataParam.MakeTextParam(false, "key0", "value0", "text/plain");
        var p1 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "key1", "oi{{K1}}", "text/plain");
        var p2 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "key2", "oi2", "text/plain");
        var p3 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "key{{K3}}", "[{{K1}}]", "application/json");
        var p4 = PororocaHttpRequestFormDataParam.MakeFileParam(true, "key4", "{{FolderPath}}\\{{FileName}}", "text/xml");
        PororocaHttpRequestBody unresolvedBody = new();
        unresolvedBody.SetFormDataContent([p0, p1, p2, p3, p4]);

        // WHEN
        var resolvedBody = ResolveRequestBody(effectiveVars, unresolvedBody);

        // THEN
        Assert.NotNull(resolvedBody);
        Assert.Equal(PororocaHttpRequestBodyMode.FormData, resolvedBody.Mode);
        Assert.Null(resolvedBody.ContentType);
        Assert.Equal([
            new(true, PororocaHttpRequestFormDataParamType.Text, "key1", "oi4577", "text/plain", string.Empty),
            new(true, PororocaHttpRequestFormDataParamType.Text, "key2", "oi2", "text/plain", string.Empty),
            new(true, PororocaHttpRequestFormDataParamType.Text, "key4354", "[4577]", "application/json", string.Empty),
            new(true, PororocaHttpRequestFormDataParamType.File, "key4", string.Empty, "text/xml", "C:\\MYFILES\\file.xml")],
            resolvedBody.FormDataValues);
    }

    [Fact]
    public static void Should_resolve_GraphQL_body()
    {
        // GIVEN
        PororocaVariable[] effectiveVars =
        [
            new(true, "K1", "18", true)
        ];
        PororocaHttpRequestBody unresolvedBody = new();
        unresolvedBody.SetGraphQlContent("myGraphQlQuery{{K1}}", "{\"id\":\n// some comment inside GraphQL variables\n{{K1}}}");

        // WHEN
        var resolvedBody = ResolveRequestBody(effectiveVars, unresolvedBody);

        // THEN
        Assert.NotNull(resolvedBody);
        Assert.Equal(PororocaHttpRequestBodyMode.GraphQl, resolvedBody.Mode);
        Assert.NotNull(resolvedBody.GraphQlValues);
        Assert.Equal("myGraphQlQuery{{K1}}", resolvedBody.GraphQlValues.Query);
        Assert.Equal("{\"id\":\n// some comment inside GraphQL variables\n18}", resolvedBody.GraphQlValues.Variables);
    }

    #endregion

    #region HTTP BODY

    [Fact]
    public static void Should_make_no_content_correctly()
    {
        // GIVEN
        // WHEN
        var resolvedReqContent = MakeRequestContent(null, new());

        // THEN
        Assert.Null(resolvedReqContent);
    }

    [Fact]
    public static async Task Should_make_raw_content_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody resolvedBody = new();
        resolvedBody.SetRawContent("{\"id\":3162}", "application/json");
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        // WHEN
        var resolvedReqContent = MakeRequestContent(resolvedBody, resolvedContentHeaders);

        // THEN
        Assert.NotNull(resolvedReqContent);
        Assert.True(resolvedReqContent is StringContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/json", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", resolvedReqContent.Headers.ContentType!.CharSet);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("{\"id\":3162}", contentText);
    }

    [Fact]
    public static async Task Should_make_file_content_correctly()
    {
        // GIVEN
        string testFilePath = GetTestFilePath("testfilecontent1.json");
        PororocaHttpRequestBody resolvedBody = new();
        resolvedBody.SetFileContent(testFilePath, "application/json");
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        // WHEN
        var resolvedReqContent = MakeRequestContent(resolvedBody, resolvedContentHeaders);

        // THEN
        Assert.NotNull(resolvedReqContent);
        Assert.True(resolvedReqContent is StreamContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/json", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("{\"id\":1}", contentText);
    }

    [Fact]
    public static async Task Should_resolve_form_url_encoded_content_correctly()
    {
        // GIVEN
        PororocaKeyValueParam[] resolvedUrlEncodedParams =
        [
            new(true, "key1", "abc"),
            new(true, "key3", "value3")
        ];
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        PororocaHttpRequestBody resolvedBody = new();
        resolvedBody.SetUrlEncodedContent(resolvedUrlEncodedParams);

        // WHEN
        var resolvedReqContent = MakeRequestContent(resolvedBody, resolvedContentHeaders);

        // THEN
        Assert.NotNull(resolvedReqContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/x-www-form-urlencoded", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("key1=abc&key3=value3", contentText);
    }

    [Fact]
    public static async Task Should_resolve_form_data_content_correctly()
    {
        // GIVEN
        string fileName = "testfilecontent2.json";
        string testFilePath = GetTestFilePath(fileName);
        var p1 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "key1", "oi", "text/plain");
        var p2 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "key1", "oi2", "text/plain");
        var p3 = PororocaHttpRequestFormDataParam.MakeTextParam(true, "key3", "value3", "application/json");
        var p4 = PororocaHttpRequestFormDataParam.MakeFileParam(true, "key4", testFilePath, "application/json");
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };
        var resolvedFormDataParams = new[] { p1, p2, p3, p4 };

        PororocaHttpRequestBody resolvedBody = new();
        resolvedBody.SetFormDataContent(resolvedFormDataParams);

        // WHEN
        var resolvedReqContent = MakeRequestContent(resolvedBody, resolvedContentHeaders);

        // THEN
        Assert.NotNull(resolvedReqContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("multipart/form-data", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        Assert.True(resolvedReqContent is MultipartFormDataContent);
        var castedContent = (MultipartFormDataContent)resolvedReqContent;
        Assert.Equal(4, castedContent.Count());

        var p1Content = (StringContent)castedContent.ElementAt(0);
        string? p1ContentText = await p1Content.ReadAsStringAsync();
        Assert.Equal("oi", p1ContentText);
        Assert.NotNull(p1Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p1Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key1", p1Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p1Content.Headers.ContentType);
        Assert.Equal("text/plain", p1Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", p1Content.Headers.ContentType!.CharSet);

        var p2Content = (StringContent)castedContent.ElementAt(1);
        string? p2ContentText = await p2Content.ReadAsStringAsync();
        Assert.Equal("oi2", p2ContentText);
        Assert.NotNull(p2Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p2Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key1", p2Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p2Content.Headers.ContentType);
        Assert.Equal("text/plain", p2Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", p2Content.Headers.ContentType!.CharSet);

        var p3Content = (StringContent)castedContent.ElementAt(2);
        string? p3ContentText = await p3Content.ReadAsStringAsync();
        Assert.Equal("value3", p3ContentText);
        Assert.NotNull(p3Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p3Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key3", p3Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p3Content.Headers.ContentType);
        Assert.Equal("application/json", p3Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", p3Content.Headers.ContentType!.CharSet);

        var p4Content = (StreamContent)castedContent.ElementAt(3);
        string? p4ContentText = await p4Content.ReadAsStringAsync();
        Assert.Equal("{\"id\":2}", p4ContentText);
        Assert.NotNull(p4Content.Headers.ContentDisposition);
        Assert.Equal("form-data", p4Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("key4", p4Content.Headers.ContentDisposition!.Name);
        Assert.NotNull(p4Content.Headers.ContentType);
        Assert.Equal("application/json", p4Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public static async Task Should_resolve_graphql_content_without_variables_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody resolvedBody = new();
        resolvedBody.SetGraphQlContent("myGraphQlQuery", null);
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        // WHEN
        var resolvedReqContent = MakeRequestContent(resolvedBody, resolvedContentHeaders);

        // THEN
        Assert.NotNull(resolvedReqContent);
        Assert.True(resolvedReqContent is StringContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/json", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("{\"query\":\"myGraphQlQuery\",\"variables\":null}", contentText);
    }

    [Fact]
    public static async Task Should_resolve_graphql_content_with_variables_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody resolvedBody = new();
        resolvedBody.SetGraphQlContent("myGraphQlQuery", "{\"id\":\n// some comment inside GraphQL variables\n19}");
        Dictionary<string, string> resolvedContentHeaders = new(1)
        {
            { "Content-Language", "pt-BR" }
        };

        // WHEN
        var resolvedReqContent = MakeRequestContent(resolvedBody, resolvedContentHeaders);

        // THEN
        Assert.NotNull(resolvedReqContent);
        Assert.True(resolvedReqContent is StringContent);
        Assert.NotNull(resolvedReqContent!.Headers.ContentType);
        Assert.Equal("application/json", resolvedReqContent.Headers.ContentType!.MediaType);
        Assert.Contains("pt-BR", resolvedReqContent!.Headers.ContentLanguage);

        string? contentText = await resolvedReqContent.ReadAsStringAsync();
        Assert.Equal("{\"query\":\"myGraphQlQuery\",\"variables\":{\"id\":19}}", contentText);
    }

    #endregion

    private static string GetTestFilePath(string fileName)
    {
        var testDataDirInfo = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!;
        return Path.Combine(testDataDirInfo.FullName, "TestData", fileName);
    }
}