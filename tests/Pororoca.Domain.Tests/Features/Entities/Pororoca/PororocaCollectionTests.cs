using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca;

public static class PororocaCollectionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public static void If_str_to_replace_templates_null_or_whitespace_then_return_empty_str(string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        string resolvedStr = ((IPororocaVariableResolver)col).ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        if (strToReplaceTemplates != null)
        {
            Assert.Equal(strToReplaceTemplates, resolvedStr);
        }
        else
        {
            Assert.Equal(string.Empty, resolvedStr);
        }
    }

    [Theory]
    [InlineData("v1/{{k3}}", "{{k1}}/{{k3}}")]
    [InlineData("v1/{{k4}}", "{{k1}}/{{k4}}")]
    public static void Should_use_collection_vars_to_resolve_template_if_no_env(string expectedResult, string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();
        col.Environments.Clear();
        col.Environments.AddRange(Array.Empty<PororocaEnvironment>());

        // WHEN
        string resolvedStr = ((IPororocaVariableResolver)col).ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        Assert.Equal(expectedResult, resolvedStr);
    }

    [Theory]
    [InlineData("v1/{{k3}}", "{{k1}}/{{k3}}")]
    [InlineData("v1/{{k4}}", "{{k1}}/{{k4}}")]
    public static void Should_use_collection_vars_to_resolve_template_if_no_selected_env(string expectedResult, string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();
        foreach (var env in col.Environments)
        {
            env.IsCurrent = false;
        }

        // WHEN
        string resolvedStr = ((IPororocaVariableResolver)col).ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        Assert.Equal(expectedResult, resolvedStr);
    }

    [Theory]
    [InlineData("v0/v1env1/v3env1", "{{k0}}/{{k1}}/{{k3}}")]
    [InlineData("v0/v1env1/{{k4}}", "{{k0}}/{{k1}}/{{k4}}")]
    [InlineData("k0k1k4", "k0k1k4")]
    [InlineData("k0v1env1k4", "k0{{k1}}k4")]
    public static void Should_use_selected_env_vars_with_collection_vars_to_resolve_template(string expectedResult, string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();
        col.Environments.First(e => e.Name == "MyEnvironment1").IsCurrent = true;
        col.Environments.First(e => e.Name == "MyEnvironment2").IsCurrent = false;

        // WHEN
        string resolvedStr = ((IPororocaVariableResolver)col).ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        // Should use only enabled vars
        // Should use only vars from selected environment
        // Selected environment enabled vars override collection enabled vars
        Assert.Equal(expectedResult, resolvedStr);
    }

    private static PororocaCollection CreateTestCollection()
    {
        PororocaCollection col = new("MyCollection");

        PororocaVariable v0 = new(true, "k0", "v0", false);
        PororocaVariable v1 = new(true, "k1", "v1", false);
        PororocaVariable v2 = new(false, "k2", "v2", false);
        var colVars = new PororocaVariable[] { v0, v1, v2 };

        PororocaVariable v1env1 = new(true, "k1", "v1env1", false);
        PororocaVariable v3env1 = new(true, "k3", "v3env1", false);
        PororocaVariable v4env1 = new(false, "k4", "v4env1", false);
        var env1Vars = new PororocaVariable[] { v1env1, v3env1, v4env1 };

        PororocaVariable v1env2 = new(true, "k1", "v1env2", false);
        PororocaVariable v3env2 = new(true, "k3", "v3env2", false);
        PororocaVariable v4env2 = new(false, "k4", "v4env2", false);
        var env2Vars = new PororocaVariable[] { v1env2, v3env2, v4env2 };

        col.Variables.Clear();
        col.Variables.AddRange(colVars);

        PororocaEnvironment env1 = new("MyEnvironment1");
        env1.IsCurrent = false;
        env1.UpdateVariables(env1Vars);
        col.Environments.Add(env1);

        PororocaEnvironment env2 = new("MyEnvironment2");
        env2.IsCurrent = true;
        env2.UpdateVariables(env2Vars);
        col.Environments.Add(env2);

        return col;
    }
}