using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Postman;
using Xunit;
using static Pororoca.Domain.Features.ExportEnvironment.PostmanEnvironmentExporter;

namespace Pororoca.Domain.Tests.Features.ExportEnvironment;

public static class PostmanEnvironmentExporterTests
{
    private static readonly Guid testEnvId = Guid.NewGuid();
    private const string testEnvName = "TestEnvironment";

    [Fact]
    public static void Should_convert_pororoca_environment_correctly()
    {
        var pororocaEnvironment = CreateTestPororocaEnvironment();
        var env = ConvertToPostmanEnvironment(pororocaEnvironment);
        AssertConvertedEnvironment(env);
    }

    private static void AssertConvertedEnvironment(PostmanEnvironment env)
    {
        Assert.NotNull(env);
        Assert.Equal(testEnvId, env.Id);
        Assert.Equal(testEnvName, env.Name);
        Assert.Equal("environment", env.Scope);
        Assert.Contains(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'"), env.ExportedAt);
        Assert.Equal("Postman/10.5.2", env.ExportedUsing);
        Assert.Equal(2, env.Values.Length);

        var var1 = env.Values[0];
        Assert.True(var1.Enabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);
        Assert.Null(var1.Type);

        var var2 = env.Values[1];
        Assert.False(var2.Enabled);
        Assert.Equal("Key2", var2.Key);
        Assert.Equal("Value2", var2.Value);
        Assert.Equal("secret", var2.Type);
    }

    private static PororocaEnvironment CreateTestPororocaEnvironment() =>
        new(testEnvId,
            DateTimeOffset.Now,
            testEnvName,
            false,
            [
                new(true, "Key1", "Value1", false),
                new(false, "Key2", "Value2", true)
            ]);
}