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
        col.AddVariable(new(true, "k1", "v1", false));
        col.AddVariable(new(false, "k2", "v2", false));
        col.AddVariable(new(true, "k3", "v3", true));
        col.AddVariable(new(false, "k4", "v4", true));

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
    public static void Should_return_empty_collection_if_no_kv_params()
    {
        // GIVEN
        PororocaCollection col = new(string.Empty);
        IPororocaVariableResolver varResolver = col;
        col.AddVariable(new(true, "k1", "v1", false));
        col.AddVariable(new(false, "k2", "v2", false));
        col.AddVariable(new(true, "k3", "v3", true));
        col.AddVariable(new(false, "k4", "v4", true));

        // WHEN
        var resolvedKvps = varResolver.ResolveKeyValueParams(null).ToArray();

        // THEN
        Assert.NotNull(resolvedKvps);
        Assert.Empty(resolvedKvps);
    }
}