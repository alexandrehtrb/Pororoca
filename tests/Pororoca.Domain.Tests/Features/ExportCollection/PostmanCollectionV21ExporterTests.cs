using Xunit;
using Pororoca.Domain.Features.Entities.Postman;
using Pororoca.Domain.Features.Entities.Pororoca;
using System;
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
        PostmanVariable[] hdrs = ConvertToPostmanHeaders(new [] { p1, p2 });

        // THEN
        Assert.NotNull(hdrs);
        Assert.Equal(2, hdrs.Length);

        PostmanVariable h1 = hdrs[0];
        Assert.Null(h1.Disabled);
        Assert.Equal("Key1", h1.Key);
        Assert.Equal("Value1", h1.Value);

        PostmanVariable h2 = hdrs[1];
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
        PororocaRequestBody? reqBody = null;

        // WHEN
        PostmanRequestBody? postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.Null(postmanBody);
    }

    [Fact]
    public static void Should_convert_pororoca_req_raw_json_body_to_postman_req_body_correctly()
    {  
        // GIVEN
        PororocaRequestBody reqBody = new();
        reqBody.SetRawContent("[]", "application/json");

        // WHEN
        PostmanRequestBody? postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Raw, postmanBody!.Mode);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Equal("[]", postmanBody.Raw);
        Assert.Equal("json", postmanBody.Options!.Raw!.Language);
    }

    [Fact]
    public static void Should_convert_pororoca_req_raw_text_body_to_postman_req_body_correctly()
    {  
        // GIVEN
        PororocaRequestBody reqBody = new();
        reqBody.SetRawContent("aeiou", "text/plain");

        // WHEN
        PostmanRequestBody? postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Raw, postmanBody!.Mode);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Equal("aeiou", postmanBody.Raw);
        Assert.Equal("text", postmanBody.Options!.Raw!.Language);
    }

    [Fact]
    public static void Should_convert_pororoca_req_raw_xml_body_to_postman_req_body_correctly()
    {  
        // GIVEN
        PororocaRequestBody reqBody = new();
        reqBody.SetRawContent("<a k=\"1\"/>", "text/xml");

        // WHEN
        PostmanRequestBody? postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Raw, postmanBody!.Mode);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Equal("<a k=\"1\"/>", postmanBody.Raw);
        Assert.Equal("xml", postmanBody.Options!.Raw!.Language);
    }

    [Fact]
    public static void Should_convert_pororoca_req_url_encoded_body_to_postman_req_body_correctly()
    {  
        // GIVEN
        PororocaRequestBody reqBody = new();
        PororocaKeyValueParam p1 = new(true, "Key1", "Value1");
        PororocaKeyValueParam p2 = new(false, "Key2", "Value2");
        reqBody.SetUrlEncodedContent(new [] { p1, p2 });

        // WHEN
        PostmanRequestBody? postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Urlencoded, postmanBody!.Mode);
        Assert.Null(postmanBody.Raw);
        Assert.Null(postmanBody.Options);
        Assert.Null(postmanBody.File);
        Assert.Null(postmanBody.Formdata);
        Assert.NotNull(postmanBody.Urlencoded);
        Assert.Equal(2, postmanBody.Urlencoded!.Length);

        PostmanVariable h1 = postmanBody.Urlencoded[0];
        Assert.Null(h1.Disabled);
        Assert.Equal("Key1", h1.Key);
        Assert.Equal("Value1", h1.Value);

        PostmanVariable h2 = postmanBody.Urlencoded[1];
        Assert.True(h2.Disabled);
        Assert.Equal("Key2", h2.Key);
        Assert.Equal("Value2", h2.Value);
    }

    [Fact]
    public static void Should_convert_pororoca_req_file_body_to_postman_req_body_correctly()
    {  
        // GIVEN
        PororocaRequestBody reqBody = new();
        reqBody.SetFileContent(@"C:\Pasta1\arq.txt", "text/plain");

        // WHEN
        PostmanRequestBody? postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.File, postmanBody!.Mode);
        Assert.Null(postmanBody.Formdata);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Raw);
        Assert.Null(postmanBody.Options);
        Assert.NotNull(postmanBody.File);
        Assert.Equal(@"C:\Pasta1\arq.txt", postmanBody.File!.Src);
    }

    [Fact]
    public static void Should_convert_pororoca_req_form_data_body_to_postman_req_body_correctly()
    {  
        // GIVEN
        PororocaRequestBody reqBody = new();
        PororocaRequestFormDataParam p1t = new(true, "Key1Text");
        p1t.SetTextValue("Value1Text", "text/plain");
        PororocaRequestFormDataParam p2t = new(false, "Key2Text");
        p2t.SetTextValue("Value2Text", "application/json; charset=utf-8");
        PororocaRequestFormDataParam p1f = new(true, "Key1File");
        p1f.SetFileValue(@"C:\Pasta1\arq.txt", "text/plain");
        PororocaRequestFormDataParam p2f = new(false, "Key2File");
        p2f.SetFileValue(@"C:\Pasta1\arq2.jpg", "image/jpeg");
        reqBody.SetFormDataContent(new [] { p1t, p2t, p1f, p2f });

        // WHEN
        PostmanRequestBody? postmanBody = ConvertToPostmanRequestBody(reqBody);

        // THEN
        Assert.NotNull(postmanBody);
        Assert.Equal(PostmanRequestBodyMode.Formdata, postmanBody!.Mode);
        Assert.Null(postmanBody.Urlencoded);
        Assert.Null(postmanBody.Raw);
        Assert.Null(postmanBody.Options);
        Assert.Null(postmanBody.File);
        Assert.NotNull(postmanBody.Formdata);

        Assert.NotNull(postmanBody.Formdata);
        Assert.Equal(4, postmanBody.Formdata!.Length);

        PostmanRequestBodyFormDataParam fp1 = postmanBody.Formdata[0];
        Assert.Null(fp1.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.Text, fp1.Type);
        Assert.Equal("Key1Text", fp1.Key);
        Assert.Equal("Value1Text", fp1.Value);
        Assert.Equal("text/plain", fp1.ContentType);
        Assert.Null(fp1.Src);

        PostmanRequestBodyFormDataParam fp2 = postmanBody.Formdata[1];
        Assert.True(fp2.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.Text, fp2.Type);
        Assert.Equal("Key2Text", fp2.Key);
        Assert.Equal("Value2Text", fp2.Value);
        Assert.Equal("application/json; charset=utf-8", fp2.ContentType);
        Assert.Null(fp2.Src);

        PostmanRequestBodyFormDataParam fp3 = postmanBody.Formdata[2];
        Assert.Null(fp3.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.File, fp3.Type);
        Assert.Equal("Key1File", fp3.Key);
        Assert.Null(fp3.Value);
        Assert.Equal("text/plain", fp3.ContentType);
        Assert.Equal(@"C:\Pasta1\arq.txt", fp3.Src);

        PostmanRequestBodyFormDataParam fp4 = postmanBody.Formdata[3];
        Assert.True(fp4.Disabled);
        Assert.Equal(PostmanRequestBodyFormDataParamType.File, fp4.Type);
        Assert.Equal("Key2File", fp4.Key);
        Assert.Null(fp4.Value);
        Assert.Equal("image/jpeg", fp4.ContentType);
        Assert.Equal(@"C:\Pasta1\arq2.jpg", fp4.Src);
    }

    #endregion

    #region REQUEST AUTH

    [Fact]
    public static void Should_convert_pororoca_req_none_auth_to_postman_req_auth_correctly()
    {  
        // GIVEN
        PororocaRequestAuth? reqAuth = null;

        // WHEN
        PostmanAuth? postmanAuth = ConvertToPostmanAuth(reqAuth);

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
        PororocaRequestAuth reqAuth = new();
        reqAuth.SetBasicAuth("usr", "pwd");

        // WHEN
        PostmanAuth? postmanAuth = ConvertToPostmanAuth(reqAuth);

        // THEN
        Assert.NotNull(postmanAuth);
        Assert.Equal(PostmanAuthType.basic, postmanAuth.Type);
        Assert.Null(postmanAuth.Bearer);

        Assert.NotNull(postmanAuth.Basic);
        Assert.Equal(2, postmanAuth.Basic!.Length);

        PostmanVariable a1 = postmanAuth.Basic[0];
        Assert.Null(a1.Disabled);
        Assert.Equal("string", a1.Type);
        Assert.Equal("username", a1.Key);
        Assert.Equal("usr", a1.Value);

        PostmanVariable a2 = postmanAuth.Basic[1];
        Assert.Null(a2.Disabled);
        Assert.Equal("string", a2.Type);
        Assert.Equal("password", a2.Key);
        Assert.Equal("pwd", a2.Value);
    }

    [Fact]
    public static void Should_convert_pororoca_req_bearer_auth_to_postman_req_auth_correctly()
    {  
        // GIVEN
        PororocaRequestAuth reqAuth = new();
        reqAuth.SetBearerAuth("tkn");

        // WHEN
        PostmanAuth? postmanAuth = ConvertToPostmanAuth(reqAuth);

        // THEN
        Assert.NotNull(postmanAuth);
        Assert.Equal(PostmanAuthType.bearer, postmanAuth.Type);
        Assert.Null(postmanAuth.Basic);

        Assert.NotNull(postmanAuth.Bearer);
        Assert.Single(postmanAuth.Bearer);

        PostmanVariable a1 = postmanAuth.Bearer![0];
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
        PororocaRequest req = new("Req1");
        PororocaKeyValueParam h1 = new(true, "Key1", "Value1");
        PororocaKeyValueParam h2 = new(false, "Key2", "Value2");
        req.UpdateHeaders(new [] { h1, h2 });
        PororocaRequestAuth auth = new();
        auth.SetBasicAuth("usr", "pwd");
        req.UpdateCustomAuth(auth);
        req.UpdateMethod("POST");
        req.UpdateUrl("http://www.abc.com.br");
        PororocaRequestBody body = new();
        body.SetRawContent("[]", "application/json");
        req.UpdateBody(body);

        // WHEN
        PostmanCollectionItem? postmanReq = ConvertToPostmanItem(req);

        // THEN
        Assert.NotNull(postmanReq);
        Assert.Equal("Req1", postmanReq.Name);
        Assert.Null(postmanReq.Items);
        Assert.NotNull(postmanReq.Request);

        PostmanVariable[]? hdrs = postmanReq.Request?.Header;

        Assert.NotNull(hdrs);
        Assert.Equal(2, hdrs!.Length);

        PostmanVariable hdr1 = hdrs[0];
        Assert.Null(hdr1.Disabled);
        Assert.Equal("Key1", hdr1.Key);
        Assert.Equal("Value1", hdr1.Value);

        PostmanVariable hdr2 = hdrs[1];
        Assert.True(hdr2.Disabled);
        Assert.Equal("Key2", hdr2.Key);
        Assert.Equal("Value2", hdr2.Value);

        PostmanAuth? postmanAuth = postmanReq.Request?.Auth;
        Assert.NotNull(postmanAuth);
        Assert.NotNull(postmanAuth!.Basic);
        Assert.Equal(2, postmanAuth!.Basic!.Length);

        PostmanVariable a1 = postmanAuth.Basic[0];
        Assert.Null(a1.Disabled);
        Assert.Equal("string", a1.Type);
        Assert.Equal("username", a1.Key);
        Assert.Equal("usr", a1.Value);

        PostmanVariable a2 = postmanAuth.Basic[1];
        Assert.Null(a2.Disabled);
        Assert.Equal("string", a2.Type);
        Assert.Equal("password", a2.Key);
        Assert.Equal("pwd", a2.Value);

        Assert.Equal("POST", postmanReq.Request!.Method);
        Assert.Equal("http://www.abc.com.br", postmanReq.Request.Url.Raw);

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
        PororocaCollection col = CreateTestCollection();

        // WHEN
        PostmanCollectionV21 postmanCollection = ConvertToPostmanCollectionV21(col, true);

        // THEN
        AssertConvertedCollection(postmanCollection, true);
    }

    [Fact]
    public static void Should_convert_pororoca_collection_showing_secrets_to_postman_collection_correctly()
    {  
        // GIVEN
        PororocaCollection col = CreateTestCollection();

        // WHEN
        PostmanCollectionV21 postmanCollection = ConvertToPostmanCollectionV21(col, false);

        // THEN
        AssertConvertedCollection(postmanCollection, false);
    }

    private static PororocaCollection CreateTestCollection()
    {
        PororocaCollection col = new(testGuid, testName, DateTimeOffset.Now);
        PororocaRequest req1 = new("Req1");
        req1.UpdateUrl("http://www.abc.com.br");
        PororocaRequest req2 = new("Req2");
        req2.UpdateUrl("https://www.ghi.com.br");
        PororocaCollectionFolder folder1 = new("Folder1");
        folder1.AddRequest(req2);
        col.AddRequest(req1);
        col.AddFolder(folder1);
        col.UpdateVariables(new PororocaVariable[]
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

        PostmanCollectionItem postmanReq1 = postmanCollection.Items[0];
        Assert.Equal("Req1", postmanReq1.Name);
        Assert.Null(postmanReq1.Items);
        Assert.Equal("GET", postmanReq1.Request!.Method);
        Assert.Equal("http://www.abc.com.br", postmanReq1.Request!.Url.Raw);

        PostmanCollectionItem postmanFolder1 = postmanCollection.Items[1];
        Assert.Equal("Folder1", postmanFolder1.Name);
        Assert.Null(postmanFolder1.Request);
        Assert.NotNull(postmanFolder1.Items);
        Assert.Single(postmanFolder1.Items);

        PostmanCollectionItem postmanReq2 = postmanFolder1.Items![0];
        Assert.Equal("Req2", postmanReq2.Name);
        Assert.Null(postmanReq2.Items);
        Assert.Equal("GET", postmanReq2.Request!.Method);
        Assert.Equal("https://www.ghi.com.br", postmanReq2.Request!.Url.Raw);

        Assert.NotNull(postmanCollection.Variable);
        Assert.Equal(2, postmanCollection.Variable!.Length);

        PostmanVariable var1 = postmanCollection.Variable[0];
        Assert.False(var1.Disabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);

        PostmanVariable var2 = postmanCollection.Variable[1];
        Assert.True(var2.Disabled);
        Assert.Equal("Key2", var2.Key);
        if (hideSecrets)
            Assert.Equal(string.Empty, var2.Value);
        else
            Assert.Equal("Value2", var2.Value);
    }

    #endregion

}