

using System;
using System.Globalization;
using System.Windows.Data;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class OperatorToStringConverter : IValueConverter
    {
        public static Array All => Enum.GetValues(typeof(Operator));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Operator))
                return "No";

            return (Operator)value switch
            {
                Operator.No => "No",
                Operator.LT => "<=",
                Operator.GT => ">=",
                _ => value.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Operator.No;

            return value switch
            {
                "No" => Operator.No,
                "<=" => Operator.LT,
                ">=" => Operator.GT,
                _ => Operator.No
            };
        }
    }

}
