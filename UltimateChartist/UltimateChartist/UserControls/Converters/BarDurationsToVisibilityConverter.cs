using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using UltimateChartist.DataModels;

namespace UltimateChartist.UserControls.Converters;

public class BarDurationsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var durations = value as BarDuration[];
        if (durations != null && durations.Contains((BarDuration)parameter))
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
