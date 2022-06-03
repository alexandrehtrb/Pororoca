using Pororoca.Domain.Features.Entities.Pororoca;
using Xunit;
using static Pororoca.Domain.Features.ExportEnvironment.PororocaEnvironmentExporter;

namespace Pororoca.Domain.Tests.Features.ExportEnvironment;

public static class PororocaEnvironmentExporterTests
{
    private static readonly Guid testEnvId = Guid.NewGuid();
    private const string testEnvName = "TestEnvironment";
    private static readonly DateTimeOffset testEnvCreationDate = DateTimeOffset.Now;


    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public static void Should_hide_pororoca_environment_secrets_correctly(bool shouldHideSecrets, bool preserveIsCurrentEnvironment)
    {
        // GIVEN
        var pororocaEnvironment = CreateTestPororocaEnvironment();

        // WHEN
        var env = GenerateEnvironmentToExport(pororocaEnvironment, shouldHideSecrets, preserveIsCurrentEnvironment);

        // THEN
        AssertEnvironment(env, shouldHideSecrets, preserveIsCurrentEnvironment);
    }

    private static void AssertEnvironment(PororocaEnvironment env, bool areSecretsHidden, bool shouldPreserveIsCurrentEnv)
    {
        Assert.NotNull(env);
        Assert.Equal(testEnvId, env.Id);
        Assert.Equal(testEnvName, env.Name);
        Assert.Equal(testEnvCreationDate, env.CreatedAt);
        // Always export as non current environment,
        // unless if exporting environment inside of a collection
        if (shouldPreserveIsCurrentEnv)
        {
            Assert.True(env.IsCurrent);
        }
        else
        {
            Assert.False(env.IsCurrent);
        }

        Assert.NotNull(env.Variables);
        Assert.Equal(2, env.Variables.Count);

        var var1 = env.Variables[0];
        Assert.True(var1.Enabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);
        Assert.False(var1.IsSecret);

        var var2 = env.Variables[1];
        Assert.False(var2.Enabled);
        Assert.Equal("Key2", var2.Key);
        Assert.True(var2.IsSecret);

        if (areSecretsHidden)
        {
            Assert.Equal(string.Empty, var2.Value); // Hidden secret value
        }
        else
        {
            Assert.Equal("Value2", var2.Value);
        }
    }

    private static PororocaEnvironment CreateTestPororocaEnvironment()
    {
        PororocaEnvironment env = new(testEnvId, testEnvName, testEnvCreationDate)
        {
            IsCurrent = true
        };
        env.UpdateVariables(new PororocaVariable[]
        {
            new(true, "Key1", "Value1", false),
            new(false, "Key2", "Value2", true)
        });
        return env;
    }
}