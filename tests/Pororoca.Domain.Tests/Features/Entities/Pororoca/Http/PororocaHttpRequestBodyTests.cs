using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Xunit;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.Http;

public static class PororocaHttpRequestBodyTests
{
    [Fact]
    public static void Should_copy_raw_req_body_creating_new_instance()
    {
        // GIVEN
        var body = MakeRawContent("{\"id\":3162}", "application/json");

        // WHEN
        var copy = body.Copy();

        // THEN
        Assert.Equal(body, copy);
        Assert.NotSame(body, copy);
    }

    [Fact]
    public static void Should_copy_file_req_body_creating_new_instance()
    {
        // GIVEN
        var body = MakeFileContent("testfilecontent1.json", "application/json");

        // WHEN
        var copy = body.Copy();

        // THEN
        Assert.Equal(body, copy);
        Assert.NotSame(body, copy);
    }

    [Fact]
    public static void Should_copy_url_encoded_req_body_creating_new_instance()
    {
        // GIVEN
        var body = MakeUrlEncodedContent([
            new(true, "key1", "abc"),
            new(true, "key3", "value3")
        ]);

        // WHEN
        var copy = body.Copy();

        // THEN
        Assert.NotSame(body, copy);
        Assert.Equal(PororocaHttpRequestBodyMode.UrlEncoded, copy.Mode);
        Assert.NotNull(copy.UrlEncodedValues);
        Assert.Equal(body.UrlEncodedValues!.Count, copy.UrlEncodedValues.Count);
        foreach (var (pBody, pCopy) in body.UrlEncodedValues.Zip(copy.UrlEncodedValues))
        {
            Assert.Equal(pBody, pCopy);
            Assert.NotSame(pBody, pCopy);
        }
    }

    [Fact]
    public static void Should_copy_form_data_req_body_creating_new_instance()
    {
        // GIVEN
        var body = MakeFormDataContent([
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "key1", "oi", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "key1", "oi2", "text/plain"),
            PororocaHttpRequestFormDataParam.MakeTextParam(true, "key3", "value3", "application/json"),
            PororocaHttpRequestFormDataParam.MakeFileParam(true, "key4", "testfilecontent2.json", "application/json")
        ]);

        // WHEN
        var copy = body.Copy();

        // THEN
        Assert.NotSame(body, copy);
        Assert.Equal(PororocaHttpRequestBodyMode.FormData, copy.Mode);
        Assert.NotNull(copy.FormDataValues);
        Assert.Equal(body.FormDataValues!.Count, copy.FormDataValues.Count);
        foreach (var (pBody, pCopy) in body.FormDataValues.Zip(copy.FormDataValues))
        {
            Assert.Equal(pBody, pCopy);
            Assert.NotSame(pBody, pCopy);
        }
    }

    [Fact]
    public static void Should_copy_graphql_no_variables_req_body_creating_new_instance()
    {
        // GIVEN
        var body = MakeGraphQlContent("myGraphQlQuery", null);

        // WHEN
        var copy = body.Copy();

        // THEN
        Assert.Equal(body, copy);
        Assert.NotSame(body, copy);
    }

    [Fact]
    public static void Should_copy_graphql_with_variables_req_body_creating_new_instance()
    {
        // GIVEN
        var body = MakeGraphQlContent("myGraphQlQuery", "{\"id\":\n// some comment inside GraphQL variables\n19}");

        // WHEN
        var copy = body.Copy();

        // THEN
        Assert.Equal(body, copy);
        Assert.NotSame(body, copy);
    }
}