namespace Pororoca.Desktop.Converters;

public enum ExportCollectionFormat
{
    Pororoca = 0,
    Postman = 1
}

internal static class ExportCollectionFormatMapping
{
    internal static ExportCollectionFormat MapIndexToEnum(int index) =>
        index switch
        {
            0 => ExportCollectionFormat.Pororoca,
            1 => ExportCollectionFormat.Postman,
            _ => ExportCollectionFormat.Pororoca
        };

    internal static int MapEnumToIndex(ExportCollectionFormat? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            ExportCollectionFormat.Pororoca => 0,
            ExportCollectionFormat.Postman => 1,
            _ => 0
        };
}

public sealed class ExportCollectionFormatMatchConverter : EnumMatchConverter<ExportCollectionFormat>
{
    protected override ExportCollectionFormat? MapIndexToEnum(int index) =>
        ExportCollectionFormatMapping.MapIndexToEnum(index);
}