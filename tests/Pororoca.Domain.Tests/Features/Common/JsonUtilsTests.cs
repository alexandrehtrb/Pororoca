using Pororoca.Domain.Features.Common;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Common;

public static class JsonUtilsTests
{
    [Theory]
    [InlineData("{a:1}")]
    [InlineData("[{]")]
    [InlineData("'John'")]
    [InlineData("{\"John says \"Hello!\"")]
    [InlineData("$1.00")]
    [InlineData("<99.00 * 0.15")]
    [InlineData("[\"Hello\", 3.14, true, ]")]
    [InlineData("[\"Hello\", 3.14, , true]")]
    [InlineData("[\"Hello\", 3.14, true}")]
    [InlineData("[\"Hello\", 3.14, true, \"name\": \"Joe\"]")]
    [InlineData("{\"name\": \"Joe\", \"age\": null, }")]
    [InlineData("{\"name\": \"Joe\", , \"age\": null}")]
    [InlineData("{\"name\": \"Joe\", \"age\": null]")]
    [InlineData("{\"name\": \"Joe\", \"age\": }")]
    [InlineData("{\"name\": \"Joe\", \"age\" }")]
    [InlineData("{{}}")]
    public static void Should_detect_invalid_jsons(string txt) =>
        Assert.False(JsonUtils.IsValidJson(txt));

    [Theory]
    [InlineData("{\"a\":1}")]
    [InlineData("[1]")]
    [InlineData("\"John\"")]
    [InlineData("\"John says \"Hello!\"\"")]
    [InlineData("\"$1.00\"")]
    [InlineData("\"99.00 * 0.15\"")]
    [InlineData("[\"Hello\", 3.14, true, false]")]
    [InlineData("[\"Hello\", 3.14, \"oi\", true]")]
    [InlineData("[\"Hello\", 3.14, true]")]
    [InlineData("[\"Hello\", 3.14, true, \"name\", \"Joe\"]")]
    [InlineData("[\"Hello\", 3.14, true, \"name\", \"Joe\", {\"a\":111}]")]
    [InlineData("[{\"a\":111}, {\"b\":112}]")]
    [InlineData("{\"name\": \"Joe\", \"age\": null }")]
    [InlineData("{\"name\": \"Joe\", \"a\":123, \"age\": null}")]
    [InlineData("{\"name\": \"Joe\", \"age\": null}")]
    [InlineData("{\"name\": \"Joe\", \"age\": 14}")]
    [InlineData("{\"name\": \"Joe\", \"age\":{\"value\":14 } }")]
    [InlineData("{\"v\":{\"v\":0}}")]
    public static void Should_detect_valid_jsons(string txt) =>
        Assert.True(JsonUtils.IsValidJson(txt));
}