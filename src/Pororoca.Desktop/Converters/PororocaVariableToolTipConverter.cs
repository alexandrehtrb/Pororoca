using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Desktop.Converters;

public class PororocaVariableToolTipConverter : IMultiValueConverter
{
    public static readonly PororocaVariableToolTipConverter Instance = new();

    private PororocaVariableToolTipConverter() { }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is string srcTxt && values[1] is CollectionViewModel varResolver)
        {
            return IPororocaVariableResolver.ReplaceTemplates(srcTxt, ((IPororocaVariableResolver)varResolver).GetEffectiveVariables());
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}