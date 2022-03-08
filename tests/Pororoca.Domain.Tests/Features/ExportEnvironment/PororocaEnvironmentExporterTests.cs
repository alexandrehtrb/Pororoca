using Xunit;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Domain.Features.ExportEnvironment.PororocaEnvironmentExporter;

namespace Pororoca.Domain.Tests.Features.ExportEnvironment;

public static class PororocaEnvironmentExporterTests
{
    private static readonly Guid testEnvId = Guid.NewGuid();
    private const string testEnvName = "TestEnvironment";
    private static readonly DateTimeOffset testEnvCreationDate = DateTimeOffset.Now;

    [Fact]
    public static void Should_hide_pororoca_environment_secrets_correctly()
    {
        // GIVEN
        PororocaEnvironment pororocaEnvironment = CreateTestPororocaEnvironment();

        // WHEN
        PororocaEnvironment env = GenerateEnvironmentWithHiddenSecrets(pororocaEnvironment);

        // THEN
        Assert.NotNull(env);
        Assert.Equal(testEnvId, env.Id);
        Assert.Equal(testEnvName, env.Name);
        Assert.Equal(testEnvCreationDate, env.CreatedAt);
        Assert.False(env.IsCurrent);  // Always export as non current environment

        Assert.NotNull(env.Variables);
        Assert.Equal(2, env.Variables.Count);

        PororocaVariable var1 = env.Variables[0];
        Assert.True(var1.Enabled);
        Assert.Equal("Key1", var1.Key);
        Assert.Equal("Value1", var1.Value);
        Assert.False(var1.IsSecret);

        PororocaVariable var2 = env.Variables[1];
        Assert.False(var2.Enabled);
        Assert.Equal("Key2", var2.Key);
        Assert.Equal(string.Empty, var2.Value); // Hidden secret value
        Assert.True(var2.IsSecret);
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