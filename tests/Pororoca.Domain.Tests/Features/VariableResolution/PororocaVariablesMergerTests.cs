using Xunit;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Tests.Features.VariableResolution;

public static class PororocaVariablesMergerTests
{
    [Fact]
    public static void Should_return_only_enabled_collection_vars_if_no_env_vars()
    {  
        // GIVEN
        PororocaVariable v1 = new(true, "k1", "v1", false);
        PororocaVariable v2 = new(false, "k2", "v2", false);

        PororocaVariable[] colVars = new PororocaVariable[] { v1, v2 };
        PororocaVariable[]? selectedEnvVars = null;

        // WHEN
        PororocaVariable[] effectiveVars = PororocaVariablesMerger.MergeVariables(colVars, selectedEnvVars);

        // THEN
        Assert.Single(effectiveVars, v1);
    }

    [Fact]
    public static void Should_return_enabled_vars_in_collection_and_selected_env_if_they_dont_coincide()
    {  
        // GIVEN
        PororocaVariable v1 = new(true, "k1", "v1", false);
        PororocaVariable v2 = new(false, "k2", "v2", false);
        PororocaVariable[] colVars = new PororocaVariable[] { v1, v2 };

        PororocaVariable v3 = new(true, "k3", "v3", false);
        PororocaVariable v4 = new(false, "k4", "v4", false);
        PororocaVariable[] selectedEnvVars = new PororocaVariable[] { v3, v4 };

        // WHEN
        PororocaVariable[] effectiveVars = PororocaVariablesMerger.MergeVariables(colVars, selectedEnvVars);

        // THEN
        Assert.Equal(2, effectiveVars.Length);
        Assert.Contains(v1, effectiveVars);
        Assert.Contains(v3, effectiveVars);
    }

    [Fact]
    public static void Enabled_selected_env_vars_should_override_collection_vars_if_they_coincide_keys()
    {  
        // GIVEN
        PororocaVariable v1c = new(true, "k1", "v1c", false);
        PororocaVariable v2 = new(false, "k2", "v2", false);
        PororocaVariable[] colVars = new PororocaVariable[] { v1c, v2 };

        PororocaVariable v1e = new(true, "k1", "v1e", false);
        PororocaVariable v3 = new(true, "k3", "v3", false);
        PororocaVariable v4 = new(false, "k4", "v4", false);
        PororocaVariable[] selectedEnvVars = new PororocaVariable[] { v1e, v3, v4 };

        // WHEN
        PororocaVariable[] effectiveVars = PororocaVariablesMerger.MergeVariables(colVars, selectedEnvVars);

        // THEN
        Assert.Equal(2, effectiveVars.Length);
        Assert.DoesNotContain(v1c, effectiveVars);
        Assert.Contains(v1e, effectiveVars);
        Assert.Contains(v3, effectiveVars);
    }
}