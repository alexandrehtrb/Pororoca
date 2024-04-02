using Pororoca.Domain.Features.Entities.Pororoca.Repetition;

namespace Pororoca.Desktop.Converters;

internal static class RepetitionModeMapping
{
    internal static PororocaRepetitionMode MapIndexToEnum(int index) =>
        index switch
        {
            0 => PororocaRepetitionMode.Simple,
            1 => PororocaRepetitionMode.Sequential,
            2 => PororocaRepetitionMode.Random,
            _ => PororocaRepetitionMode.Simple
        };

    internal static int MapEnumToIndex(PororocaRepetitionMode? mode) =>
        mode switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaRepetitionMode.Simple => 0,
            PororocaRepetitionMode.Sequential => 1,
            PororocaRepetitionMode.Random => 2,
            _ => 0
        };
}

public class RepetitionModeMatchConverter : EnumMatchConverter<PororocaRepetitionMode>
{
    protected override PororocaRepetitionMode? MapIndexToEnum(int index) =>
        RepetitionModeMapping.MapIndexToEnum(index);
}