using Xunit;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;

namespace Pororoca.Test.Tests;

public sealed class FactOnlyIfOSSupportsHttp3 : FactAttribute
{
    public FactOnlyIfOSSupportsHttp3() {
        if (!IsHttpVersionAvailableInOS(3.0m, out _)) {
            Skip = ".NET does not support HTTP/3 on this operating system, skipping...";
        }
    }    
}