using Xunit;

namespace Pororoca.Test.Tests;

public static class AssertExtensions
{
    public static void AssertTypeAndCast<T>(object @object, out T castedObject)
    {
        Assert.IsType<T>(@object);
        castedObject = (T) @object;
    }
}