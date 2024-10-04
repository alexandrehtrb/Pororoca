using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.Converters;

internal static class AuthModeMapping
{
    internal static PororocaRequestAuthMode? MapIndexToEnum(int index) =>
        index switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            5 => PororocaRequestAuthMode.ClientCertificate,
            4 => PororocaRequestAuthMode.Windows,
            3 => PororocaRequestAuthMode.Bearer,
            2 => PororocaRequestAuthMode.Basic,
            1 => PororocaRequestAuthMode.InheritFromCollection,
            _ => null
        };

    internal static int MapEnumToIndex(PororocaRequestAuthMode? mode) =>
        mode switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaRequestAuthMode.ClientCertificate => 5,
            PororocaRequestAuthMode.Windows => 4,
            PororocaRequestAuthMode.Bearer => 3,
            PororocaRequestAuthMode.Basic => 2,
            PororocaRequestAuthMode.InheritFromCollection => 1,
            _ => 0
        };
}

public sealed class AuthModeMatchConverter : EnumMatchConverter<PororocaRequestAuthMode>
{
    protected override PororocaRequestAuthMode? MapIndexToEnum(int index) =>
        AuthModeMapping.MapIndexToEnum(index);
}