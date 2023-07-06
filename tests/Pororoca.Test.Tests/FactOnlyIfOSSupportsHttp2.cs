using Xunit;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Test.Tests;

public sealed class FactOnlyIfOSSupportsHttp2 : FactAttribute
{
    public FactOnlyIfOSSupportsHttp2()
    {
        if (!IsHttpVersionAvailableInOS(2.0m, out _))
        {
            Skip = ".NET does not support HTTP/2 on this operating system, skipping...";
        }
    }
}