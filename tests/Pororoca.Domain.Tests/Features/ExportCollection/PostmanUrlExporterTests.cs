using Xunit;
using Pororoca.Domain.Features.Entities.Postman;
using static Pororoca.Domain.Features.ExportCollection.PostmanCollectionV21Exporter;

namespace Pororoca.Domain.Tests.Features.ExportCollection;

public static class PostmanUrlExporterTests
{
    #region NON TEMPLATED URLS

    [Fact]
    public static void Should_convert_non_templated_url_with_all_components_to_postman_url_correctly()
    {  
        string rawUrl = "https://www.rqrq.com.br:8421/api/ckp/kop?qwe=123&rty=uyu";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("https", postmanUrl.Protocol);
        Assert.Equal(new [] { "www", "rqrq", "com", "br" }, postmanUrl.Host);
        Assert.Equal("8421", postmanUrl.Port);
        Assert.Equal(new [] { "api", "ckp", "kop" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Equal(2, postmanUrl.Query!.Length);

        PostmanVariable qryParam1 = postmanUrl.Query[0];
        Assert.Null(qryParam1.Disabled);
        Assert.Equal("qwe", qryParam1.Key);
        Assert.Equal("123", qryParam1.Value);

        PostmanVariable qryParam2 = postmanUrl.Query[1];
        Assert.Null(qryParam2.Disabled);
        Assert.Equal("rty", qryParam2.Key);
        Assert.Equal("uyu", qryParam2.Value);
    }

    [Fact]
    public static void Should_convert_non_templated_url_half_query_param_to_postman_url_correctly()
    {  
        string rawUrl = "https://www.rqrq.com.br:8421/api/ckp/kop?qwe";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("https", postmanUrl.Protocol);
        Assert.Equal(new [] { "www", "rqrq", "com", "br" }, postmanUrl.Host);
        Assert.Equal("8421", postmanUrl.Port);
        Assert.Equal(new [] { "api", "ckp", "kop" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Single(postmanUrl.Query);

        PostmanVariable qryParam1 = postmanUrl.Query![0];
        Assert.Null(qryParam1.Disabled);
        Assert.Equal("qwe", qryParam1.Key);
        Assert.Null(qryParam1.Value);
    }

    [Fact]
    public static void Should_convert_non_templated_url_no_query_params_to_postman_url_correctly()
    {  
        string rawUrl = "https://www.rqrq.com.br:8421/api/ckp/kop";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("https", postmanUrl.Protocol);
        Assert.Equal(new [] { "www", "rqrq", "com", "br" }, postmanUrl.Host);
        Assert.Equal("8421", postmanUrl.Port);
        Assert.Equal(new [] { "api", "ckp", "kop" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Empty(postmanUrl.Query);
    }

    [Fact]
    public static void Should_convert_non_templated_url_default_port_to_postman_url_correctly()
    {  
        string rawUrl = "http://www.rqrq.com.br:80/api/ckp/kop";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("http", postmanUrl.Protocol);
        Assert.Equal(new [] { "www", "rqrq", "com", "br" }, postmanUrl.Host);
        Assert.Null(postmanUrl.Port);
        Assert.Equal(new [] { "api", "ckp", "kop" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Empty(postmanUrl.Query);
    }

    [Fact]
    public static void Should_convert_non_templated_no_port_url_segment_with_dot_to_postman_url_correctly()
    {  
        string rawUrl = "http://www.rqrq.com.br/api/ckp/kop.txt";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("http", postmanUrl.Protocol);
        Assert.Equal(new [] { "www", "rqrq", "com", "br" }, postmanUrl.Host);
        Assert.Null(postmanUrl.Port);
        Assert.Equal(new [] { "api", "ckp", "kop.txt" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Empty(postmanUrl.Query);
    }

    #endregion

    #region TEMPLATED URLS

    [Fact]
    public static void Should_convert_templated_url_with_all_components_to_postman_url_correctly()
    {  
        string rawUrl = "{{SCHEME}}://{{HOST}}:{{PORT}}/{{ENDPOINT}}?p1={{PARAM1}}&p2={{PARAM2}}";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("{{SCHEME}}", postmanUrl.Protocol);
        Assert.Equal(new [] { "{{HOST}}" }, postmanUrl.Host);
        Assert.Equal("{{PORT}}", postmanUrl.Port);
        Assert.Equal(new [] { "{{ENDPOINT}}" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Equal(2, postmanUrl.Query!.Length);

        PostmanVariable qryParam1 = postmanUrl.Query[0];
        Assert.Null(qryParam1.Disabled);
        Assert.Equal("p1", qryParam1.Key);
        Assert.Equal("{{PARAM1}}", qryParam1.Value);

        PostmanVariable qryParam2 = postmanUrl.Query[1];
        Assert.Null(qryParam2.Disabled);
        Assert.Equal("p2", qryParam2.Key);
        Assert.Equal("{{PARAM2}}", qryParam2.Value);
    }

    [Fact]
    public static void Should_convert_templated_url_with_part_fixed_segment_to_postman_url_correctly()
    {  
        string rawUrl = "{{SCHEME}}://{{HOST}}:{{PORT}}/api/{{ENDPOINT}}?p1={{PARAM1}}&p2={{PARAM2}}";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("{{SCHEME}}", postmanUrl.Protocol);
        Assert.Equal(new [] { "{{HOST}}" }, postmanUrl.Host);
        Assert.Equal("{{PORT}}", postmanUrl.Port);
        Assert.Equal(new [] { "api", "{{ENDPOINT}}" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Equal(2, postmanUrl.Query!.Length);

        PostmanVariable qryParam1 = postmanUrl.Query[0];
        Assert.Null(qryParam1.Disabled);
        Assert.Equal("p1", qryParam1.Key);
        Assert.Equal("{{PARAM1}}", qryParam1.Value);

        PostmanVariable qryParam2 = postmanUrl.Query[1];
        Assert.Null(qryParam2.Disabled);
        Assert.Equal("p2", qryParam2.Key);
        Assert.Equal("{{PARAM2}}", qryParam2.Value);
    }

    [Fact]
    public static void Should_convert_templated_url_without_port_to_postman_url_correctly()
    {  
        string rawUrl = "{{SCHEME}}://{{HOST}}/api/{{ENDPOINT}}?p1={{PARAM1}}&p2={{PARAM2}}";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Equal("{{SCHEME}}", postmanUrl.Protocol);
        Assert.Equal(new [] { "{{HOST}}" }, postmanUrl.Host);
        Assert.Null(postmanUrl.Port);
        Assert.Equal(new [] { "api", "{{ENDPOINT}}" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Equal(2, postmanUrl.Query!.Length);

        PostmanVariable qryParam1 = postmanUrl.Query[0];
        Assert.Null(qryParam1.Disabled);
        Assert.Equal("p1", qryParam1.Key);
        Assert.Equal("{{PARAM1}}", qryParam1.Value);

        PostmanVariable qryParam2 = postmanUrl.Query[1];
        Assert.Null(qryParam2.Disabled);
        Assert.Equal("p2", qryParam2.Key);
        Assert.Equal("{{PARAM2}}", qryParam2.Value);
    }

    [Fact]
    public static void Should_convert_templated_url_without_scheme_to_postman_url_correctly()
    {  
        string rawUrl = "{{HOST}}/api/{{ENDPOINT}}?p1={{PARAM1}}&p2={{PARAM2}}";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Null(postmanUrl.Protocol);
        Assert.Equal(new [] { "{{HOST}}" }, postmanUrl.Host);
        Assert.Null(postmanUrl.Port);
        Assert.Equal(new [] { "api", "{{ENDPOINT}}" }, postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Equal(2, postmanUrl.Query!.Length);

        PostmanVariable qryParam1 = postmanUrl.Query[0];
        Assert.Null(qryParam1.Disabled);
        Assert.Equal("p1", qryParam1.Key);
        Assert.Equal("{{PARAM1}}", qryParam1.Value);

        PostmanVariable qryParam2 = postmanUrl.Query[1];
        Assert.Null(qryParam2.Disabled);
        Assert.Equal("p2", qryParam2.Key);
        Assert.Equal("{{PARAM2}}", qryParam2.Value);
    }

    [Fact]
    public static void Should_convert_templated_url_without_query_params_to_postman_url_correctly()
    {  
        string rawUrl = "{{HOST}}/api/{{ENDPOINT}}";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Null(postmanUrl.Protocol);
        Assert.Equal(new [] { "{{HOST}}" }, postmanUrl.Host);
        Assert.Null(postmanUrl.Port);
        Assert.Equal(new [] { "api", "{{ENDPOINT}}" }, postmanUrl.Path);
        
        Assert.Null(postmanUrl.Query);
    }

    [Fact]
    public static void Should_convert_templated_url_without_segments_to_postman_url_correctly()
    {  
        string rawUrl = "{{HOST}}?p1={{PARAM1}}&p2={{PARAM2}}";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Null(postmanUrl.Protocol);
        Assert.Equal(new [] { "{{HOST}}" }, postmanUrl.Host);
        Assert.Null(postmanUrl.Port);
        Assert.Empty(postmanUrl.Path);
        
        Assert.NotNull(postmanUrl.Query);
        Assert.Equal(2, postmanUrl.Query!.Length);

        PostmanVariable qryParam1 = postmanUrl.Query[0];
        Assert.Null(qryParam1.Disabled);
        Assert.Equal("p1", qryParam1.Key);
        Assert.Equal("{{PARAM1}}", qryParam1.Value);

        PostmanVariable qryParam2 = postmanUrl.Query[1];
        Assert.Null(qryParam2.Disabled);
        Assert.Equal("p2", qryParam2.Key);
        Assert.Equal("{{PARAM2}}", qryParam2.Value);
    }

    [Fact]
    public static void Should_convert_templated_url_with_host_only_to_postman_url_correctly()
    {  
        string rawUrl = "{{HOST}}";
        PostmanRequestUrl postmanUrl = ConvertToPostmanRequestUrl(rawUrl);

        Assert.Equal(rawUrl, postmanUrl.Raw);
        Assert.Null(postmanUrl.Protocol);
        Assert.Equal(new [] { "{{HOST}}" }, postmanUrl.Host);
        Assert.Null(postmanUrl.Port);
        Assert.Empty(postmanUrl.Path);
        
        Assert.Null(postmanUrl.Query);
    }

    #endregion
}