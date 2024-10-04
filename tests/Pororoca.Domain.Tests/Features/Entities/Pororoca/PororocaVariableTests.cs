using Pororoca.Domain.Features.Entities.Pororoca;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca;

public static class PororocaVariableTests
{
    [Fact]
    public static void Should_not_censor_pororoca_variable_when_is_not_secret()
    {
        // GIVEN
        PororocaVariable v = new(false, "key", "value", false);

        // WHEN
        var censored = v.Censor();

        // THEN
        Assert.False(censored.Enabled);
        Assert.Equal(v.Key, censored.Key);
        Assert.Equal(v.Value, censored.Value);
        Assert.Equal(v.IsSecret, censored.IsSecret);
    }

    [Fact]
    public static void Should_censor_pororoca_variable_when_is_secret()
    {
        // GIVEN
        PororocaVariable v = new(true, "key", "value", true);

        // WHEN
        var censored = v.Censor();

        // THEN
        Assert.True(censored.Enabled);
        Assert.Equal(v.Key, censored.Key);
        Assert.Equal(string.Empty, censored.Value);
        Assert.Equal(v.IsSecret, censored.IsSecret);
    }
}