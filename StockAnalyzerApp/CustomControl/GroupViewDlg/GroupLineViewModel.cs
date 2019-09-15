using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzerApp.CustomControl.GroupViewDlg
{
    public class GroupLineViewModel : NotifyPropertyChangedBase
    {
        public const int MIN_BARS = 300;

        public StockSerie StockSerie;
        public GroupLineViewModel(StockSerie stockSerie)
        {
            this.StockSerie = stockSerie;
            if (!stockSerie.IsInitialised)
                throw new ArgumentException("StockSerie must be initialized");
            var barDuration = stockSerie.BarDuration;

            stockSerie.BarDuration = BarDuration.Daily;

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var count = closeSerie.Count;
            if (count < MIN_BARS)
                throw new ArgumentException($"StockSerie contain at elast {MIN_BARS} daily bars");
            var lastClose = closeSerie[count - 1];

            this.Daily = (lastClose - closeSerie[count - 2]) / closeSerie[count - 2];
            this.Weekly = (lastClose - closeSerie[count - 5]) / closeSerie[count - 5];
            this.Monthly = (lastClose - closeSerie[count - 20]) / closeSerie[count - 20];
            this.Yearly = (lastClose - closeSerie[count - 200]) / closeSerie[count - 200];

            stockSerie.BarDuration = barDuration;
        }

        public float Daily { get; }
        public float Weekly { get; }
        public float Monthly { get; }
        public float Yearly { get; }
    }
}
