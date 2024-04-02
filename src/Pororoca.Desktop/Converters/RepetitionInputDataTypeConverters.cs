using Pororoca.Domain.Features.Entities.Pororoca.Repetition;

namespace Pororoca.Desktop.Converters;

internal static class RepetitionInputDataTypeMapping
{
    internal static PororocaRepetitionInputDataType MapIndexToEnum(int index) =>
        index switch
        {
            0 => PororocaRepetitionInputDataType.RawJsonArray,
            1 => PororocaRepetitionInputDataType.File,
            _ => PororocaRepetitionInputDataType.RawJsonArray
        };

    internal static int MapEnumToIndex(PororocaRepetitionInputDataType? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaRepetitionInputDataType.RawJsonArray => 0,
            PororocaRepetitionInputDataType.File => 1,
            _ => 0
        };
}

public class RepetitionInputDataTypeMatchConverter : EnumMatchConverter<PororocaRepetitionInputDataType>
{
    protected override PororocaRepetitionInputDataType? MapIndexToEnum(int index) =>
        RepetitionInputDataTypeMapping.MapIndexToEnum(index);
}