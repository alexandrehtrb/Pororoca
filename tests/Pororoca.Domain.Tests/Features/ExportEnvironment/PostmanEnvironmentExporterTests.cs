using Xunit;
using System;
using Pororoca.Domain.Features.Entities.Postman;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Common;
using static Pororoca.Domain.Features.ExportEnvironment.PostmanEnvironmentExporter;
using System.Reflection;

namespace Pororoca.Domain.Tests.Features.ExportEnvironment;

public static class PostmanEnvironmentExporterTests
{
    private static readonly Guid testEnvId = Guid.NewGuid();
    private const string testEnvName = "TestEnvironment";
    private static readonly DateTimeOffset testEnvCreationDate = DateTimeOffset.Now;

    [Fact]
    public static void Should_convert_pororoca_environment_without_secrets_correctly()
    {  
        PororocaEnvironment pororocaEnvironment = CreateTestPororocaEnvironment();
        PostmanEnvironment env = ConvertToPostmanEnvironment(pororocaEnvironment, true);
        AssertConvertedEnvironment(env, true);
    }

    [Fact]
    public static void Should_convert_pororoca_environment_with_secrets_correctly()
    {  
        PororocaEnvironment pororocaEnvironment = CreateTestPororocaEnvironment();
        PostmanEnvironment env = ConvertToPostmanEnvironment(pororocaEnvironment, false);
        AssertConvertedEnvironment(env, false);
    }

    private static void AssertConvertedEnvironment(PostmanEnvironment env, bool hideSecrets)
    {
        Assert.NotNull(env);
        Assert.Equal(testEnvId, env.Id);
        Assert.Equal(testEnvName, env.Name);
        Assert.Equal("environment", env.Scope);
        Assert.Equal(DateTimeOffset.Now.Date, env.ExportedAt.Date);
        Assert.Contains("Pororoca/", env.ExportedUsing);
        Assert.Equal(2, env.Values.Length);

        PostmanEnvironmentVariable var1 = env.Values[0];
        Assert.True(var1.Enabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);

        PostmanEnvironmentVariable var2 = env.Values[1];
        Assert.False(var2.Enabled);
        Assert.Equal("Key2", var2.Key);
        if (hideSecrets)
            Assert.Equal(string.Empty, var2.Value);
        else
            Assert.Equal("Value2", var2.Value);
    }

    private static PororocaEnvironment CreateTestPororocaEnvironment()
    {
        PororocaEnvironment env = new(testEnvId, testEnvName, testEnvCreationDate)
        {
            IsCurrent = false
        };
        env.UpdateVariables(new PororocaVariable[]
        {
            new(true, "Key1", "Value1", false),
            new(false, "Key2", "Value2", true)
        });
        return env;
    }
}