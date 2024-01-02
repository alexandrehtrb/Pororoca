using System.Text;
using Xunit;
using static Pororoca.Domain.Features.ImportEnvironment.PororocaEnvironmentImporter;

namespace Pororoca.Domain.Tests.Features.ImportEnvironment;

public static class PororocaEnvironmentImporterTests
{

    [Fact]
    public static void Should_import_valid_pororoca_environment_correctly()
    {
        // GIVEN
        string json = ReadTestFileText("EmptyEnvironment.pororoca_environment.json");

        // WHEN AND THEN
        Assert.True(TryImportPororocaEnvironment(json, out var env));

        // THEN
        Assert.NotNull(env);
        Assert.NotEqual(Guid.Parse("eb19683a-3676-4564-9daf-a711661c0862"), env!.Id);
        Assert.Equal(DateTimeOffset.Parse("2022-03-03T22:07:56.1967358-03:00"), env.CreatedAt);
        Assert.Equal("Novo ambiente", env.Name);
        Assert.False(env.IsCurrent); // Must always be non current environment, despite what is in JSON
        Assert.NotNull(env.Variables);
        Assert.Empty(env.Variables);
    }
}