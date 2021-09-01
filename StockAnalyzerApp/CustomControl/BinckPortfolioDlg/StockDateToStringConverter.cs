using System;
using System.Globalization;
using System.Windows.Data;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    class StockDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            DateTime date;
            if (value is DateTime)
            {
                date = (DateTime)value;
            }
            else if (value is DateTime?)
            {
                date = ((DateTime?)value).Value;
            }
            else
            {
                return null;
            }

            if (date.Date == date)
            {
                return date.ToShortDateString();
            }
            else
            {
                return date.ToShortDateString() + " " + date.ToString("hh:mm");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
