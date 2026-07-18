using System;
using System.Globalization;
using System.Windows.Data;

namespace StockAnalyzer.StockHelpers
{
    public class FloatToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float floatValue)
            {
                var currency = parameter?.ToString();

                return Convert(floatValue, currency);
            }
            return value.ToString();
        }

        static public string Convert(float value, string currency = "€")
        {
            if (currency == null)
                currency = "€";

            if (value > 1000000000000)
            {
                return $"{value / 1000000000000:0.##}T{currency}";
            }
            else if (value > 1000000000)
            {
                return $"{value / 1000000000:0.##}G{currency}";
            }
            else if (value > 1000000)
            {
                return $"{value / 1000000:0.##}M{currency}";
            }
            else if (value > 1000)
            {
                return $"{value / 1000:0.##}K{currency}";
            }
            else
            {
                return $"{value:0.##}{currency}";
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
