using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Xunit;
using static Pororoca.Domain.Features.ExportEnvironment.PororocaEnvironmentExporter;
using static Pororoca.Domain.Features.ImportEnvironment.PororocaEnvironmentImporter;

namespace Pororoca.Domain.Tests.Features.ExportEnvironment;

public static class PororocaEnvironmentExporterTests
{
    private static readonly Guid testEnvId = Guid.NewGuid();
    private const string testEnvName = "TestEnvironment";
    private static readonly DateTimeOffset testEnvCreationDate = DateTimeOffset.Now;

    [Fact]
    public static void Should_export_and_reimport_pororoca_environment_correctly()
    {
        // GIVEN
        var env = CreateTestPororocaEnvironment();

        // WHEN AND THEN
        string json = Encoding.UTF8.GetString(ExportAsPororocaEnvironment(env));
        Assert.True(TryImportPororocaEnvironment(json, out var reimportedEnv));
        Assert.NotNull(reimportedEnv);
        AssertEnvironment(reimportedEnv);
    }

    private static void AssertEnvironment(PororocaEnvironment env)
    {
        Assert.NotNull(env);
        Assert.NotEqual(testEnvId, env.Id); // imported envs always have new ids
        Assert.Equal(testEnvName, env.Name);
        Assert.Equal(testEnvCreationDate, env.CreatedAt);
        Assert.False(env.IsCurrent); // imported envs are always not current

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
        Assert.Equal("Value2", var2.Value);
    }

    private static PororocaEnvironment CreateTestPororocaEnvironment() =>
        new(testEnvId,
            testEnvCreationDate,
            testEnvName,
            true,
            [
                new(true, "Key1", "Value1", false),
                new(false, "Key2", "Value2", true)
            ]);
}