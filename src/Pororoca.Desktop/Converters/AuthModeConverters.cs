using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.Converters;

internal static class AuthModeMapping
{
    internal static PororocaRequestAuthMode? MapIndexToEnum(int index) =>
        index switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            4 => PororocaRequestAuthMode.ClientCertificate,
            3 => PororocaRequestAuthMode.Windows,
            2 => PororocaRequestAuthMode.Bearer,
            1 => PororocaRequestAuthMode.Basic,
            _ => null
        };

    internal static int MapEnumToIndex(PororocaRequestAuthMode? mode) =>
        mode switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaRequestAuthMode.ClientCertificate => 4,
            PororocaRequestAuthMode.Windows => 3,
            PororocaRequestAuthMode.Bearer => 2,
            PororocaRequestAuthMode.Basic => 1,
            _ => 0
        };
}

public class AuthModeMatchConverter : EnumMatchConverter<PororocaRequestAuthMode>
{
    protected override PororocaRequestAuthMode? MapIndexToEnum(int index) =>
        AuthModeMapping.MapIndexToEnum(index);
}