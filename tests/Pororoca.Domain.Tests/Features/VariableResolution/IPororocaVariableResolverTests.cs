using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;

namespace Pororoca.Domain.Tests.Features.VariableResolution;

public static class IPororocaVariableResolverTests
{
    #region REPLACE TEMPLATES

    [Theory]
    [InlineData("", null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData("Hello my name is k3", "Hello my name is k3")]
    [InlineData("{{Hello my name is k3}}", "{{Hello my name is k3}}")]
    [InlineData("Hello my name is {{k3}}", "Hello my name is {{k3}}")]
    [InlineData("{{Hello}} my name is {{k3}}", "{{Hello}} my name is {{k3}}")]
    [InlineData("Hello my name is {{k4}}", "Hello my name is {{k4}}")]
    [InlineData("Hello my name is {{{{k4}}}}", "Hello my name is {{{{k4}}}}")]
    [InlineData("Hello my name is k1", "Hello my name is k1")]
    [InlineData("{{Hello my name is k1}}", "{{Hello my name is k1}}")]
    [InlineData("Hello my name is v1", "Hello my name is {{k1}}")]
    [InlineData("Hello my name is v1", "Hello my name is {{_k1}}")]
    [InlineData("{{Hello}} my name is v2", "{{Hello}} my name is {{k2..1}}")]
    [InlineData("Hello my name is v2", "Hello my name is {{k2_2}}")]
    [InlineData("Hello my name is {{v2}}", "Hello my name is {{{{k2..1}}}}")]
    public static void Should_replace_templates_correctly(string expectedString, string? inputString)
    {
        // GIVEN
        PororocaCollection col = new(string.Empty);
        col.Variables.Add(new(true, "k1", "v1", false));
        col.Variables.Add(new(true, "_k1", "v1", false));
        col.Variables.Add(new(true, "k2..1", "v2", true));
        col.Variables.Add(new(true, "k2_2", "v2", true));
        col.Variables.Add(new(false, "k3", "v3", false));
        col.Variables.Add(new(false, "k4", "v4", true));

        // WHEN
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();
        string resolvedString = ((IPororocaVariableResolver)col).ReplaceTemplates(inputString, effectiveVars);

        // THEN
        Assert.Equal(expectedString, resolvedString);
    }

    #endregion

    #region RESOLVE KEY VALUE PARAMS

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
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();
        var resolvedKvps = varResolver.ResolveKeyValueParams(kvps, effectiveVars).ToArray();

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
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();
        var resolvedKvps = varResolver.ResolveKeyValueParams(kvps, effectiveVars).ToArray();

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
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();
        var resolvedKvps = varResolver.ResolveKeyValueParams(null, effectiveVars).ToArray();

        // THEN
        Assert.NotNull(resolvedKvps);
        Assert.Empty(resolvedKvps);
    }

    #endregion

    #region EFFECTIVE VARIABLES

    [Fact]
    public static void Should_return_only_enabled_collection_vars_if_no_env_vars()
    {
        // GIVEN
        PororocaCollection col = new("col");       
        PororocaVariable v1, v2; 
        col.Variables.Add(v1 = new(true, "k1", "v1", false));
        col.Variables.Add(v2 = new(false, "k2", "v2", false));
        // no active env

        // WHEN
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();

        // THEN
        Assert.Single(effectiveVars, v1);
    }

    [Fact]
    public static void Should_return_only_enabled_collection_vars_if_no_active_env_vars()
    {
        // GIVEN
        PororocaCollection col = new("col");       
        PororocaVariable v1, v2, v3, v4; 
        col.Variables.Add(v1 = new(true, "k1", "v1", false));
        col.Variables.Add(v2 = new(false, "k2", "v2", false));
        
        PororocaEnvironment env = new("env");
        env.IsCurrent = false;
        env.Variables.Add(v3 = new(true, "k3", "v3", false));
        env.Variables.Add(v4 = new(false, "k4", "v4", false));
        col.Environments.Add(env);

        // WHEN
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();

        // THEN
        Assert.Single(effectiveVars, v1);
    }

    [Fact]
    public static void Should_return_enabled_vars_in_collection_and_selected_env_if_they_dont_coincide()
    {
        // GIVEN
        PororocaVariable v1,v2,v3,v4;
        PororocaCollection col = new("col");
        col.Variables.Add(v1 = new(true, "k1", "v1", false));
        col.Variables.Add(v2 = new(false, "k2", "v2", false));
        
        PororocaEnvironment env = new("env");
        env.IsCurrent = true;
        env.Variables.Add(v3 = new(true, "k3", "v3", false));
        env.Variables.Add(v4 = new(false, "k4", "v4", false));
        col.Environments.Add(env);

        // WHEN
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();

        // THEN
        Assert.Equal(2, effectiveVars.Count());
        Assert.Contains(v1, effectiveVars);
        Assert.Contains(v3, effectiveVars);
    }

    [Fact]
    public static void Enabled_selected_env_vars_should_override_collection_vars_if_they_coincide_keys()
    {
        // GIVEN
        PororocaVariable v1c,v2,v1e,v3,v4;
        PororocaCollection col = new("col");
        col.Variables.Add(v1c = new(true, "k1", "v1c", false));
        col.Variables.Add(v2 = new(false, "k2", "v2", false));
        
        PororocaEnvironment env = new("env");
        env.IsCurrent = true;
        env.Variables.Add(v1e = new(true, "k1", "v1e", false));
        env.Variables.Add(v3 = new(true, "k3", "v3", false));
        env.Variables.Add(v4 = new(false, "k4", "v4", false));
        col.Environments.Add(env);

        // WHEN
        var effectiveVars = ((IPororocaVariableResolver)col).GetEffectiveVariables();

        // THEN
        Assert.Equal(2, effectiveVars.Count());
        Assert.DoesNotContain(v1c, effectiveVars);
        Assert.Contains(v1e, effectiveVars);
        Assert.Contains(v3, effectiveVars);
    }

    #endregion
}