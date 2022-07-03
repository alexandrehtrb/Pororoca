using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Postman;
using Xunit;
using static Pororoca.Domain.Features.ImportCollection.PostmanCollectionV21Importer;

namespace Pororoca.Domain.Tests.Features.ImportCollection;

public static class PostmanCollectionV21ImporterTests
{
    private static readonly Guid testGuid = Guid.NewGuid();
    private const string testName = "MyCollection";

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
        var hdrs = ConvertToPororocaHeaders(new[] { p1, p2 });

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
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.Null(reqBody);
    }

    [Fact]
    public static void Should_convert_postman_req_raw_json_body_to_pororoca_req_body_correctly()
    {
        // GIVEN
        PostmanRequestBody? postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Raw,
            Options = new() { Raw = new() { Language = "json" } },
            Raw = "[]"
        };

        // WHEN
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaRequestBodyMode.Raw, reqBody!.Mode);
        Assert.Equal("[]", reqBody.RawContent);
        Assert.Equal(MimeTypesDetector.DefaultMimeTypeForJson, reqBody.ContentType);
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
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaRequestBodyMode.Raw, reqBody!.Mode);
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
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaRequestBodyMode.Raw, reqBody!.Mode);
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
            Urlencoded = new[] { p1, p2 }
        };

        // WHEN
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaRequestBodyMode.UrlEncoded, reqBody!.Mode);

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
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaRequestBodyMode.File, reqBody!.Mode);
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
            ContentType = "text/plain",
            Type = PostmanRequestBodyFormDataParamType.File
        };
        PostmanRequestBodyFormDataParam p2f = new()
        {
            Key = "Key2File",
            Src = @"C:\Pasta1\arq2.jpg",
            ContentType = "image/jpeg",
            Type = PostmanRequestBodyFormDataParamType.File,
            Disabled = true
        };
        PostmanRequestBody postmanBody = new()
        {
            Mode = PostmanRequestBodyMode.Formdata,
            Formdata = new[] { p1t, p2t, p1f, p2f }
        };

        // WHEN
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaRequestBodyMode.FormData, reqBody!.Mode);

        Assert.Equal(4, reqBody.FormDataValues!.Count);

        var f1t = reqBody.FormDataValues[0];
        Assert.True(f1t.Enabled);
        Assert.Equal(PororocaRequestFormDataParamType.Text, f1t.Type);
        Assert.Equal("Key1Text", f1t.Key);
        Assert.Equal("Value1Text", f1t.TextValue);
        Assert.Equal("text/plain", f1t.ContentType);

        var f2t = reqBody.FormDataValues[1];
        Assert.False(f2t.Enabled);
        Assert.Equal(PororocaRequestFormDataParamType.Text, f2t.Type);
        Assert.Equal("Key2Text", f2t.Key);
        Assert.Equal("Value2Text", f2t.TextValue);
        Assert.Equal("application/json; charset=utf-8", f2t.ContentType);

        var f1f = reqBody.FormDataValues[2];
        Assert.True(f1f.Enabled);
        Assert.Equal(PororocaRequestFormDataParamType.File, f1f.Type);
        Assert.Equal("Key1File", f1f.Key);
        Assert.Equal(@"C:\Pasta1\arq.txt", f1f.FileSrcPath);
        Assert.Equal("text/plain", f1f.ContentType);

        var f2f = reqBody.FormDataValues[3];
        Assert.False(f2f.Enabled);
        Assert.Equal(PororocaRequestFormDataParamType.File, f2f.Type);
        Assert.Equal("Key2File", f2f.Key);
        Assert.Equal(@"C:\Pasta1\arq2.jpg", f2f.FileSrcPath);
        Assert.Equal("image/jpeg", f2f.ContentType);
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
        var reqBody = ConvertToPororocaRequestBody(postmanBody);

        // THEN
        Assert.NotNull(reqBody);
        Assert.Equal(PororocaRequestBodyMode.GraphQl, reqBody!.Mode);
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
    public static void Should_convert_postman_req_basic_auth_to_pororoca_req_auth_correctly()
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
    public static void Should_convert_postman_req_bearer_auth_to_pororoca_req_auth_correctly()
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
            Header = new PostmanVariable[]
            {
                new() { Disabled = null, Key = "Key1", Value = "Value1" },
                new() { Disabled = true, Key = "Key2", Value = "Value2" }
            },
            Body = new()
            {
                Mode = PostmanRequestBodyMode.Raw,
                Options = new() { Raw = new() { Language = "json" } },
                Raw = "[]"
            },
            Url = new() { Raw = "http://www.abc.com.br" }
        };

    [Fact]
    public static void Should_convert_postman_req_with_no_inherited_auth_to_pororoca_req_correctly()
    {
        // GIVEN
        PororocaRequestAuth? collectionScopedAuth = null;
        string reqName = "MyRequest";
        var postmanRequest = CreateTestRequestWithAuth();

        // WHEN
        var req = ConvertToPororocaRequest(reqName, postmanRequest, collectionScopedAuth);

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

        Assert.Equal(PororocaRequestBodyMode.Raw, req.Body!.Mode);
        Assert.Equal("[]", req.Body.RawContent);
        Assert.Equal(MimeTypesDetector.DefaultMimeTypeForJson, req.Body.ContentType);
    }

    [Fact]
    public static void Should_convert_postman_req_with_inherited_auth_to_pororoca_req_correctly()
    {
        // GIVEN
        PororocaRequestAuth collectionScopedAuth = new(PororocaRequestAuthMode.Bearer);
        collectionScopedAuth.SetBearerAuth("tkn");
        string reqName = "MyRequest";
        var postmanRequest = CreateTestRequestWithAuth();
        postmanRequest.Auth = null;

        // WHEN
        var req = ConvertToPororocaRequest(reqName, postmanRequest, collectionScopedAuth);

        // THEN
        Assert.NotNull(req);
        Assert.Equal(reqName, req.Name);
        Assert.Equal(1.1m, req.HttpVersion);
        Assert.Equal("POST", req.HttpMethod);
        Assert.Equal("http://www.abc.com.br", req.Url);

        Assert.Equal(PororocaRequestAuthMode.Bearer, req.CustomAuth!.Mode);
        Assert.Equal("tkn", req.CustomAuth.BearerToken);

        Assert.Equal(2, req.Headers!.Count);
        var hdr1 = req.Headers[0];
        Assert.True(hdr1.Enabled);
        Assert.Equal("Key1", hdr1.Key);
        Assert.Equal("Value1", hdr1.Value);
        var hdr2 = req.Headers[1];
        Assert.False(hdr2.Enabled);
        Assert.Equal("Key2", hdr2.Key);
        Assert.Equal("Value2", hdr2.Value);

        Assert.Equal(PororocaRequestBodyMode.Raw, req.Body!.Mode);
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
            Variable = new PostmanVariable[]
            {
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2", Disabled = true }
            },
            Auth = new()
            {
                Type = PostmanAuthType.bearer,
                Bearer = new PostmanVariable[]
                {
                    new() { Key = "token", Value = "tkn", Type = "string" }
                }
            },
            Items = new PostmanCollectionItem[]
            {
                new()
                {
                    Name = "Folder1",
                    Items = new PostmanCollectionItem[]
                    {
                        new()
                        {
                            Name = "Req1",
                            Request = new()
                            {
                                Method = "GET",
                                Header = Array.Empty<PostmanVariable>(),
                                Url = new() { Raw = "http://www.abc.com.br" }
                            }
                        }
                    }
                },
                new()
                {
                    Name = "Req2",
                    Request = new()
                    {
                        Method = "GET",
                        Header = Array.Empty<PostmanVariable>(),
                        Url = new() { Raw = "http://www.def.com.br" }
                    }
                }
            }
        };

    private static void AssertConvertedCollection(PororocaCollection? pororocaCollection)
    {
        Assert.NotNull(pororocaCollection);
        Assert.NotEqual(testGuid, pororocaCollection!.Id);
        Assert.Equal(testName, pororocaCollection.Name);
        Assert.Single(pororocaCollection.Folders);
        Assert.Single(pororocaCollection.Requests);

        var folder1 = pororocaCollection.Folders[0];
        Assert.Equal("Folder1", folder1.Name);
        Assert.Empty(folder1.Folders);
        Assert.Single(folder1.Requests);

        var req1 = folder1.Requests[0];
        Assert.Equal("Req1", req1.Name);
        Assert.Equal("GET", req1.HttpMethod);
        Assert.Equal("http://www.abc.com.br", req1.Url);
        Assert.Equal(PororocaRequestAuthMode.Bearer, req1.CustomAuth!.Mode);
        Assert.Equal("tkn", req1.CustomAuth.BearerToken);

        var req2 = pororocaCollection.Requests[0];
        Assert.Equal("Req2", req2.Name);
        Assert.Equal("GET", req2.HttpMethod);
        Assert.Equal("http://www.def.com.br", req2.Url);
        Assert.Equal(PororocaRequestAuthMode.Bearer, req2.CustomAuth!.Mode);
        Assert.Equal("tkn", req2.CustomAuth.BearerToken);

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