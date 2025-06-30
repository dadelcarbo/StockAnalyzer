using System.Globalization;
using System.Windows.Data;

namespace DonkeyKong.Helpers
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Enum.Parse(value.GetType(), parameter.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, parameter.ToString());
        }
    }

}
