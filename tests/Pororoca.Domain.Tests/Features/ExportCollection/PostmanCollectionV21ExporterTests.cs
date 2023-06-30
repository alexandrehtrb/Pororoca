using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Postman;
using Xunit;
using static Pororoca.Domain.Features.ExportCollection.PostmanCollectionV21Exporter;

namespace Pororoca.Domain.Tests.Features.ExportCollection;

public static class PostmanCollectionV21ExporterTests
{
    private static readonly Guid testGuid = Guid.NewGuid();
    private const string testName = "MyCollection";

    #region REQUEST HEADERS

    [Fact]
    public static void Should_convert_pororoca_req_header_to_postman_variable_correctly()
    {
        // GIVEN
        PororocaKeyValueParam p1 = new(true, "Key1", "Value1");
        PororocaKeyValueParam p2 = new(false, "Key2", "Value2");

        // WHEN
        var hdrs = ConvertToPostmanHeaders(new[] { p1, p2 });

        // THEN
        Assert.NotNull(hdrs);
        Assert.Equal(2, hdrs.Length);

        var h1 = hdrs[0];
        Assert.Null(h1.Disabled);
        Assert.Equal("Key1", h1.Key);
        Assert.Equal("Value1", h1.Value);

        var h2 = hdrs[1];
        Assert.True(h2.Disabled);
        Assert.Equal("Key2", h2.Key);
        Assert.Equal("Value2", h2.Value);
    }

    #endregion

    #region REQUEST BODY

    [Fact]
    public static void Should_convert_pororoca_req_none_body_to_postman_req_body_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody? reqBody = null;

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.Null(postmanBody);
    }

    [Fact]
    public static void Should_convert_pororoca_req_raw_json_body_to_postman_req_body_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody reqBody = new();
        reqBody.SetRawContent("[]", "application/json");

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Raw, postmanBody!.Mode);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Graphql);
        Assert.Equal("[]", postmanBody.Raw);
        Assert.Equal("json", postmanBody.Options!.Raw!.Language);
    }

    [Fact]
    public static void Should_convert_pororoca_req_raw_text_body_to_postman_req_body_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody reqBody = new();
        reqBody.SetRawContent("aeiou", "text/plain");

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Raw, postmanBody!.Mode);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Graphql);
        Assert.Equal("aeiou", postmanBody.Raw);
        Assert.Equal("text", postmanBody.Options!.Raw!.Language);
    }

    [Fact]
    public static void Should_convert_pororoca_req_raw_xml_body_to_postman_req_body_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody reqBody = new();
        reqBody.SetRawContent("<a k=\"1\"/>", "text/xml");

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Raw, postmanBody!.Mode);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Graphql);
        Assert.Equal("<a k=\"1\"/>", postmanBody.Raw);
        Assert.Equal("xml", postmanBody.Options!.Raw!.Language);
    }

    [Fact]
    public static void Should_convert_pororoca_req_url_encoded_body_to_postman_req_body_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody reqBody = new();
        PororocaKeyValueParam p1 = new(true, "Key1", "Value1");
        PororocaKeyValueParam p2 = new(false, "Key2", "Value2");
        reqBody.SetUrlEncodedContent(new[] { p1, p2 });

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Urlencoded, postmanBody!.Mode);
        Assert.Null(postmanBody.Raw);
        Assert.Null(postmanBody.Options);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Graphql);
        Assert.NotNull(postmanBody.Urlencoded);
        Assert.Equal(2, postmanBody.Urlencoded!.Length);

        var h1 = postmanBody.Urlencoded[0];
        Assert.Null(h1.Disabled);
        Assert.Equal("Key1", h1.Key);
        Assert.Equal("Value1", h1.Value);

        var h2 = postmanBody.Urlencoded[1];
        Assert.True(h2.Disabled);
        Assert.Equal("Key2", h2.Key);
        Assert.Equal("Value2", h2.Value);
    }

    [Fact]
    public static void Should_convert_pororoca_req_file_body_to_postman_req_body_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody reqBody = new();
        reqBody.SetFileContent(@"C:\Pasta1\arq.txt", "text/plain");

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.File, postmanBody!.Mode);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Raw);
        Assert.Null(postmanBody.Options);
        Assert.Null(postmanBody.Graphql);
        Assert.NotNull(postmanBody.File);
        Assert.Equal(@"C:\Pasta1\arq.txt", postmanBody.File!.Src);
    }

    [Fact]
    public static void Should_convert_pororoca_req_form_data_body_to_postman_req_body_correctly()
    {
        // GIVEN
        PororocaHttpRequestBody reqBody = new();
        PororocaHttpRequestFormDataParam p1t = new(true, "Key1Text");
        p1t.SetTextValue("Value1Text", "text/plain");
        PororocaHttpRequestFormDataParam p2t = new(false, "Key2Text");
        p2t.SetTextValue("Value2Text", "application/json; charset=utf-8");
        PororocaHttpRequestFormDataParam p1f = new(true, "Key1File");
        p1f.SetFileValue(@"C:\Pasta1\arq.txt", "text/plain");
        PororocaHttpRequestFormDataParam p2f = new(false, "Key2File");
        p2f.SetFileValue(@"C:\Pasta1\arq2.jpg", "image/jpeg");
        reqBody.SetFormDataContent(new[] { p1t, p2t, p1f, p2f });

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Formdata, postmanBody!.Mode);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Raw);
        Assert.Null(postmanBody.Options);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Graphql);
        Assert.NotNull(postmanBody.Formdata);
        Assert.Equal(4, postmanBody.Formdata!.Length);

        var fp1 = postmanBody.Formdata[0];
        Assert.Null(fp1.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.Text, fp1.Type);
        Assert.Equal("Key1Text", fp1.Key);
        Assert.Equal("Value1Text", fp1.Value);
        Assert.Equal("text/plain", fp1.ContentType);
        Assert.Null(fp1.Src);

        var fp2 = postmanBody.Formdata[1];
        Assert.True(fp2.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.Text, fp2.Type);
        Assert.Equal("Key2Text", fp2.Key);
        Assert.Equal("Value2Text", fp2.Value);
        Assert.Equal("application/json; charset=utf-8", fp2.ContentType);
        Assert.Null(fp2.Src);

        var fp3 = postmanBody.Formdata[2];
        Assert.Null(fp3.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.File, fp3.Type);
        Assert.Equal("Key1File", fp3.Key);
        Assert.Null(fp3.Value);
        Assert.Equal("text/plain", fp3.ContentType);
        Assert.Equal(@"C:\Pasta1\arq.txt", fp3.Src);

        var fp4 = postmanBody.Formdata[3];
        Assert.True(fp4.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.File, fp4.Type);
        Assert.Equal("Key2File", fp4.Key);
        Assert.Null(fp4.Value);
        Assert.Equal("image/jpeg", fp4.ContentType);
        Assert.Equal(@"C:\Pasta1\arq2.jpg", fp4.Src);
    }

    [Fact]
    public static void Should_convert_pororoca_req_graphql_body_to_postman_req_body_correctly()
    {
        // GIVEN
        const string qry = "query allFruits { fruits { fruit_name } }";
        const string variables = "{\"id\":{{CocoId}}}";
        PororocaHttpRequestBody reqBody = new();
        reqBody.SetGraphQlContent(qry, variables);

        // WHEN
        var postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Graphql, postmanBody!.Mode);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Raw);
        Assert.Null(postmanBody.Options);
        Assert.Null(postmanBody.File);
        Assert.NotNull(postmanBody.Graphql);
        Assert.Equal(qry, postmanBody.Graphql!.Query);
        Assert.Equal(variables, postmanBody.Graphql!.Variables);
    }

    #endregion

    #region REQUEST AUTH

    [Fact]
    public static void Should_convert_pororoca_req_none_auth_to_postman_req_auth_correctly()
    {
        // GIVEN
        PororocaRequestAuth? reqAuth = null;

        // WHEN
        var postmanAuth = ConvertToPostmanAuth(reqAuth);

        // THEN
        Assert.NotNull(postmanAuth);
        Assert.Equal(PostmanAuthType.noauth, postmanAuth.Type);
        Assert.Null(postmanAuth.Basic);
        Assert.Null(postmanAuth.Bearer);
    }

    [Fact]
    public static void Should_convert_pororoca_req_basic_auth_to_postman_req_auth_correctly()
    {
        // GIVEN
        PororocaRequestAuth reqAuth = PororocaRequestAuth.MakeBasicAuth("usr", "pwd");

        // WHEN
        var postmanAuth = ConvertToPostmanAuth(reqAuth);

        // THEN
        Assert.NotNull(postmanAuth);
        Assert.Equal(PostmanAuthType.basic, postmanAuth.Type);
        Assert.Null(postmanAuth.Bearer);

        var basic = Assert.IsType<PostmanVariable[]>(postmanAuth.Basic);
        Assert.Equal(2, basic.Length);

        var a1 = basic[0];
        Assert.Null(a1.Disabled);
        Assert.Equal("string", a1.Type);
        Assert.Equal("username", a1.Key);
        Assert.Equal("usr", a1.Value);

        var a2 = basic[1];
        Assert.Null(a2.Disabled);
        Assert.Equal("string", a2.Type);
        Assert.Equal("password", a2.Key);
        Assert.Equal("pwd", a2.Value);
    }

    [Fact]
    public static void Should_convert_pororoca_req_bearer_auth_to_postman_req_auth_correctly()
    {
        // GIVEN
        PororocaRequestAuth reqAuth = PororocaRequestAuth.MakeBearerAuth("tkn");

        // WHEN
        var postmanAuth = ConvertToPostmanAuth(reqAuth);

        // THEN
        Assert.NotNull(postmanAuth);
        Assert.Equal(PostmanAuthType.bearer, postmanAuth.Type);
        Assert.Null(postmanAuth.Basic);

        var bearer = Assert.IsType<PostmanVariable[]>(postmanAuth.Bearer);
        Assert.Single(bearer);

        var a1 = bearer[0];
        Assert.Null(a1.Disabled);
        Assert.Equal("string", a1.Type);
        Assert.Equal("token", a1.Key);
        Assert.Equal("tkn", a1.Value);
    }

    #endregion

    #region REQUEST

    [Fact]
    public static void Should_convert_pororoca_req_to_postman_req_correctly()
    {
        // GIVEN
        PororocaHttpRequest req = new("Req1");
        PororocaKeyValueParam h1 = new(true, "Key1", "Value1");
        PororocaKeyValueParam h2 = new(false, "Key2", "Value2");
        req.UpdateHeaders(new[] { h1, h2 });
        PororocaRequestAuth auth = PororocaRequestAuth.MakeBasicAuth("usr", "pwd");
        req.UpdateCustomAuth(auth);
        req.UpdateMethod("POST");
        req.UpdateUrl("http://www.abc.com.br");
        PororocaHttpRequestBody body = new();
        body.SetRawContent("[]", "application/json");
        req.UpdateBody(body);

        // WHEN
        var postmanReq = ConvertToPostmanItem(req);

        // THEN
        Assert.NotNull(postmanReq);
        Assert.Equal("Req1", postmanReq.Name);
        Assert.Null(postmanReq.Items);
        Assert.NotNull(postmanReq.Request);
        Assert.NotNull(postmanReq.Response);
        Assert.Empty(postmanReq.Response);

        var hdrs = postmanReq.Request?.Header;

        Assert.NotNull(hdrs);
        Assert.Equal(2, hdrs!.Length);

        var hdr1 = hdrs[0];
        Assert.Null(hdr1.Disabled);
        Assert.Equal("Key1", hdr1.Key);
        Assert.Equal("Value1", hdr1.Value);

        var hdr2 = hdrs[1];
        Assert.True(hdr2.Disabled);
        Assert.Equal("Key2", hdr2.Key);
        Assert.Equal("Value2", hdr2.Value);

        var postmanAuth = postmanReq.Request?.Auth;
        Assert.NotNull(postmanAuth);
        var basic = Assert.IsType<PostmanVariable[]>(postmanAuth!.Basic);
        Assert.Equal(2, basic.Length);

        var a1 = basic[0];
        Assert.Null(a1.Disabled);
        Assert.Equal("string", a1.Type);
        Assert.Equal("username", a1.Key);
        Assert.Equal("usr", a1.Value);

        var a2 = basic[1];
        Assert.Null(a2.Disabled);
        Assert.Equal("string", a2.Type);
        Assert.Equal("password", a2.Key);
        Assert.Equal("pwd", a2.Value);

        Assert.Equal("POST", postmanReq.Request!.Method);
        Assert.Equal("http://www.abc.com.br", postmanReq.Request.Url);

        Assert.Equal(PostmanRequestBodyMode.Raw, postmanReq.Request.Body!.Mode);
        Assert.Equal("[]", postmanReq.Request.Body.Raw);
        Assert.Equal("json", postmanReq.Request.Body.Options!.Raw!.Language);
    }

    #endregion

    #region COLLECTION, FOLDERS AND COLLECTION VARIABLES

    [Fact]
    public static void Should_convert_pororoca_collection_hiding_secrets_to_postman_collection_correctly()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var postmanCollection = ConvertToPostmanCollectionV21(col, true);

        // THEN
        AssertConvertedCollection(postmanCollection, true);
    }

    [Fact]
    public static void Should_convert_pororoca_collection_showing_secrets_to_postman_collection_correctly()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var postmanCollection = ConvertToPostmanCollectionV21(col, false);

        // THEN
        AssertConvertedCollection(postmanCollection, false);
    }

    private static PororocaCollection CreateTestCollection()
    {
        PororocaCollection col = new(testGuid, testName, DateTimeOffset.Now);
        PororocaHttpRequest req1 = new("Req1");
        req1.UpdateUrl("http://www.abc.com.br");
        PororocaHttpRequest req2 = new("Req2");
        req2.UpdateUrl("https://www.ghi.com.br");
        PororocaCollectionFolder folder1 = new("Folder1");
        folder1.AddRequest(req2);
        col.Requests.Add(req1);
        col.Folders.Add(folder1);
        col.Variables.AddRange(new PororocaVariable[]
        {
            new(true, "Key1", "Value1", false),
            new(false, "Key2", "Value2", true)
        });
        return col;
    }

    private static void AssertConvertedCollection(PostmanCollectionV21? postmanCollection, bool hideSecrets)
    {
        Assert.NotNull(postmanCollection);
        Assert.Equal(testGuid, postmanCollection!.Info!.Id);
        Assert.Equal(testName, postmanCollection.Info.Name);
        Assert.Equal("https://schema.getpostman.com/json/collection/v2.1.0/collection.json", postmanCollection.Info.Schema);
        Assert.Equal(2, postmanCollection.Items.Length);

        var postmanReq1 = postmanCollection.Items[0];
        Assert.Equal("Req1", postmanReq1.Name);
        Assert.Null(postmanReq1.Items);
        Assert.Equal("GET", postmanReq1.Request!.Method);
        Assert.Equal("http://www.abc.com.br", postmanReq1.Request!.Url);

        var postmanFolder1 = postmanCollection.Items[1];
        Assert.Equal("Folder1", postmanFolder1.Name);
        Assert.Null(postmanFolder1.Request);
        Assert.NotNull(postmanFolder1.Items);
        Assert.Single(postmanFolder1.Items);

        var postmanReq2 = postmanFolder1.Items![0];
        Assert.Equal("Req2", postmanReq2.Name);
        Assert.Null(postmanReq2.Items);
        Assert.Equal("GET", postmanReq2.Request!.Method);
        Assert.Equal("https://www.ghi.com.br", postmanReq2.Request!.Url);

        Assert.NotNull(postmanCollection.Variable);
        Assert.Equal(2, postmanCollection.Variable!.Length);

        var var1 = postmanCollection.Variable[0];
        Assert.Null(var1.Disabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);

        var var2 = postmanCollection.Variable[1];
        Assert.True(var2.Disabled);
        Assert.Equal("Key2", var2.Key);
        if (hideSecrets)
            Assert.Equal(string.Empty, var2.Value);
        else
            Assert.Equal("Value2", var2.Value);
    }

    #endregion

}