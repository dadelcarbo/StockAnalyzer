using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System;
using System.Globalization;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.GroupViewDlg
{
    public class GroupLineViewModel : NotifyPropertyChangedBase
    {
        public const int MIN_BARS = 300;
        public StockSerie StockSerie { get; }

        public GroupLineViewModel(StockSerie stockSerie)
        {
            this.StockSerie = stockSerie;

            if (!stockSerie.IsInitialised)
                throw new ArgumentException("StockSerie must be initialized");

            var barDuration = stockSerie.BarDuration;
            try
            {
                stockSerie.BarDuration = BarDuration.Daily;

                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var lastIndex = closeSerie.LastIndex;
                if (lastIndex < MIN_BARS)
                    throw new ArgumentException($"StockSerie contain at elast {MIN_BARS} daily bars");
                var lastClose = closeSerie[lastIndex];
                var lastDate = stockSerie.Keys.Last();

                this.Daily = (lastClose - closeSerie[lastIndex - 1]) / closeSerie[lastIndex - 1];

                // Calculate Weekly Index
                int lastWeekNum = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(lastDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                int index = lastIndex - 1;
                for (; index > 0; index--)
                {
                    var date = stockSerie.Keys.ElementAt(index);
                    int weekNum = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    if (weekNum != lastWeekNum)
                        break;
                }
                this.Weekly = (lastClose - closeSerie[index]) / closeSerie[index];

                // Calculate Monthly Index
                int lastMonth = lastDate.Month;
                index = lastIndex - 1;
                for (; index > 0; index--)
                {
                    var date = stockSerie.Keys.ElementAt(index);
                    if (date.Month != lastMonth)
                        break;
                }
                this.Monthly = (lastClose - closeSerie[index]) / closeSerie[index];

                // Calculate YTD Index
                int lastYear = lastDate.Year;
                index = lastIndex - 1;
                for (; index > 0; index--)
                {
                    var date = stockSerie.Keys.ElementAt(index);
                    if (date.Year != lastYear)
                        break;
                }
                this.YTD = (lastClose - closeSerie[index]) / closeSerie[index];

                // Calculate Yearly Index
                var lastYearDate = lastDate.AddYears(-1);
                index = lastIndex - 1;
                for (; index > 0; index--)
                {
                    var date = stockSerie.Keys.ElementAt(index);
                    if (date < lastYearDate)
                        break;
                }
                this.Yearly = (lastClose - closeSerie[index]) / closeSerie[index];
            }
            finally
            {
                stockSerie.BarDuration = barDuration;
            }
        }

        public float Daily { get; }
        public float Weekly { get; }
        public float Monthly { get; }
        public float Yearly { get; }
        public float YTD { get; }
    }
}
