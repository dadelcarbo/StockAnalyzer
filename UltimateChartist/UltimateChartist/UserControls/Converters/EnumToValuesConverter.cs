using System;
using System.Globalization;
using System.Windows.Data;

namespace UltimateChartist.UserControls.Converters;

public class EnumToValuesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum)
        {
            return Enum.GetValues(value.GetType());
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
