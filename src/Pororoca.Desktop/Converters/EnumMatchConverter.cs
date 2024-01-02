using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Pororoca.Desktop.Converters;

public abstract class EnumMatchConverter<TEnum> : IValueConverter
    where TEnum : struct
{
    protected abstract TEnum? MapIndexToEnum(int index);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i && parameter is string s)
        {
            var acceptableMatches = ReadAcceptableMatches(s);
            var v = MapIndexToEnum(i)!;
            return v is not null && acceptableMatches.Contains((TEnum)v);
        }
        else
        {
            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
    }

    private static List<TEnum> ReadAcceptableMatches(string parameter) =>
        (parameter?.ToString()?.Trim('\'')?.Split(',') ?? Array.Empty<string>())
        .Select(x => Enum.Parse<TEnum>(x)).ToList();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
}