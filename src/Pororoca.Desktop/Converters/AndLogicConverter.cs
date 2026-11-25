using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Pororoca.Desktop.Converters;

internal class AndLogicConverter : IMultiValueConverter
{
    public static readonly AndLogicConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) => values.Count switch
    {
        1 when values[0] is bool b1 => b1,
        2 when values[0] is bool b1 && values[1] is bool b2 => b1 && b2,
        3 when values[0] is bool b1 && values[1] is bool b2 && values[2] is bool b3 => b1 && b2 && b3,
        4 when values[0] is bool b1 && values[1] is bool b2 && values[2] is bool b3 && values[3] is bool b4 => b1 && b2 && b3 && b4,
        _ => new BindingNotification(new InvalidCastException(), BindingErrorType.Error)
    };
}