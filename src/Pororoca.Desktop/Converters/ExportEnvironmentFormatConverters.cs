namespace Pororoca.Desktop.Converters;

public enum ExportEnvironmentFormat
{
    Pororoca = 0,
    Postman = 1
}

internal static class ExportEnvironmentFormatMapping
{
    internal static ExportEnvironmentFormat MapIndexToEnum(int index) =>
        index switch
        {
            0 => ExportEnvironmentFormat.Pororoca,
            1 => ExportEnvironmentFormat.Postman,
            _ => ExportEnvironmentFormat.Pororoca
        };

    internal static int MapEnumToIndex(ExportEnvironmentFormat? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            ExportEnvironmentFormat.Pororoca => 0,
            ExportEnvironmentFormat.Postman => 1,
            _ => 0
        };
}

public sealed class ExportEnvironmentFormatMatchConverter : EnumMatchConverter<ExportEnvironmentFormat>
{
    protected override ExportEnvironmentFormat? MapIndexToEnum(int index) =>
        ExportEnvironmentFormatMapping.MapIndexToEnum(index);
}