using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.MultiTimeFrameDlg
{
   public class TrendToColorConverter : IValueConverter
   {
      public static readonly IValueConverter Instance = new TrendToColorConverter();

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         StockSerie.Trend trend = (StockSerie.Trend) value;
         return
            trend == StockSerie.Trend.DownTrend
               ? Brushes.DarkRed
               : trend == StockSerie.Trend.UpTrend
                  ? Brushes.DarkGreen
                  : Brushes.Transparent; // For NoTrend
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
