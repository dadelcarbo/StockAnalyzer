using System;
using System.Windows.Data;
using UltimateChartist.DataModels;

namespace UltimateChartist.UserControls.Converters;

public class AxisDateLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            return value.ToString();
            var date = (DateTime)value;
            var duration = (BarDuration)parameter;
            if (duration < BarDuration.Daily)
            {
                return string.Format("{H:mm}" + Environment.NewLine + "{MM/dd/yy}", date);
            }
            else
            {
                if (date.Month == 1)
                    return string.Format("{0:MMM}" + Environment.NewLine + "{0:yyyy}", date);
                else
                    return string.Format("{0:MMM}", date);
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
