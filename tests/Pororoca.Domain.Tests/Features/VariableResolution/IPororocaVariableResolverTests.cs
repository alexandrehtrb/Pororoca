using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;

namespace Pororoca.Domain.Tests.Features.VariableResolution;

public static class IPororocaVariableResolverTests
{
    [Fact]
    public static void Should_resolve_enabled_kv_params_with_enabled_vars()
    {
        // GIVEN
        PororocaCollection col = new(string.Empty);
        IPororocaVariableResolver varResolver = col;
        col.Variables.Add(new(true, "k1", "v1", false));
        col.Variables.Add(new(false, "k2", "v2", false));
        col.Variables.Add(new(true, "k3", "v3", true));
        col.Variables.Add(new(false, "k4", "v4", true));

        List<PororocaKeyValueParam> kvps = new();
        kvps.Add(new(true, "Key1", "Value1"));
        kvps.Add(new(true, "Key2", "Value2"));
        kvps.Add(new(false, "Key2", "Value78"));
        kvps.Add(new(false, "{{k1}}", "ValueK1"));
        kvps.Add(new(true, "{{k1}}", "ValueK1"));
        kvps.Add(new(true, "{{k4}}", "ValueK4"));
        kvps.Add(new(true, "{{k3}}", "{{k1}}"));

        // WHEN
        var resolvedKvps = varResolver.ResolveKeyValueParams(kvps).ToArray();

        // THEN
        Assert.NotNull(resolvedKvps);
        Assert.Equal(5, resolvedKvps.Length);
        Assert.Equal(new KeyValuePair<string, string>("Key1", "Value1"), resolvedKvps[0]);
        Assert.Equal(new KeyValuePair<string, string>("Key2", "Value2"), resolvedKvps[1]);
        Assert.Equal(new KeyValuePair<string, string>("v1", "ValueK1"), resolvedKvps[2]);
        Assert.Equal(new KeyValuePair<string, string>("{{k4}}", "ValueK4"), resolvedKvps[3]);
        Assert.Equal(new KeyValuePair<string, string>("v3", "v1"), resolvedKvps[4]);
    }

    [Fact]
    public static void Should_resolve_enabled_kv_params_removing_duplicated_keys()
    {
        // GIVEN
        PororocaCollection col = new(string.Empty);
        IPororocaVariableResolver varResolver = col;
        col.Variables.Add(new(true, "k1", "v1", false));

        List<PororocaKeyValueParam> kvps = new();
        kvps.Add(new(true, "{{k1}}", "ValueK1"));
        kvps.Add(new(true, "v1", "ValueK1"));
        kvps.Add(new(true, "K1", "ValueK1"));

        // WHEN
        var resolvedKvps = varResolver.ResolveKeyValueParams(kvps).ToArray();

        // THEN
        Assert.NotNull(resolvedKvps);
        Assert.Equal(2, resolvedKvps.Length);
        Assert.Equal(new KeyValuePair<string, string>("v1", "ValueK1"), resolvedKvps[0]);
        Assert.Equal(new KeyValuePair<string, string>("K1", "ValueK1"), resolvedKvps[1]);
    }

    [Fact]
    public static void Should_return_empty_collection_if_no_kv_params()
    {
        // GIVEN
        PororocaCollection col = new(string.Empty);
        IPororocaVariableResolver varResolver = col;
        col.Variables.Add(new(true, "k1", "v1", false));
        col.Variables.Add(new(false, "k2", "v2", false));
        col.Variables.Add(new(true, "k3", "v3", true));
        col.Variables.Add(new(false, "k4", "v4", true));

        // WHEN
        var resolvedKvps = varResolver.ResolveKeyValueParams(null).ToArray();

        // THEN
        Assert.NotNull(resolvedKvps);
        Assert.Empty(resolvedKvps);
    }

    #region VARIABLES MERGE

    [Fact]
    public static void Should_return_only_enabled_collection_vars_if_no_env_vars()
    {
        // GIVEN
        PororocaVariable v1 = new(true, "k1", "v1", false);
        PororocaVariable v2 = new(false, "k2", "v2", false);

        var colVars = new PororocaVariable[] { v1, v2 };
        PororocaVariable[]? selectedEnvVars = null;

        // WHEN
        var effectiveVars = IPororocaVariableResolver.MergeVariables(colVars, selectedEnvVars);

        // THEN
        Assert.Single(effectiveVars, v1);
    }

    [Fact]
    public static void Should_return_enabled_vars_in_collection_and_selected_env_if_they_dont_coincide()
    {
        // GIVEN
        PororocaVariable v1 = new(true, "k1", "v1", false);
        PororocaVariable v2 = new(false, "k2", "v2", false);
        var colVars = new PororocaVariable[] { v1, v2 };

        PororocaVariable v3 = new(true, "k3", "v3", false);
        PororocaVariable v4 = new(false, "k4", "v4", false);
        var selectedEnvVars = new PororocaVariable[] { v3, v4 };

        // WHEN
        var effectiveVars = IPororocaVariableResolver.MergeVariables(colVars, selectedEnvVars);

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
        var colVars = new PororocaVariable[] { v1c, v2 };

        PororocaVariable v1e = new(true, "k1", "v1e", false);
        PororocaVariable v3 = new(true, "k3", "v3", false);
        PororocaVariable v4 = new(false, "k4", "v4", false);
        var selectedEnvVars = new PororocaVariable[] { v1e, v3, v4 };

        // WHEN
        var effectiveVars = IPororocaVariableResolver.MergeVariables(colVars, selectedEnvVars);

        // THEN
        Assert.Equal(2, effectiveVars.Length);
        Assert.DoesNotContain(v1c, effectiveVars);
        Assert.Contains(v1e, effectiveVars);
        Assert.Contains(v3, effectiveVars);
    }

    #endregion
}