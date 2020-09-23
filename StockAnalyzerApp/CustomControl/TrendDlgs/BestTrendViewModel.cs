using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public class BestTrendViewModel : NotifyPropertyChangedBase
    {
        public BestTrendViewModel()
        {
            this.indicatorName = "ROR(100,1)";
        }
        private string group = string.Empty;
        public string Group
        {
            get { return group; }
            set
            {
                if (group != value)
                {
                    group = value;
                    this.OnPropertyChanged("Group");
                }
            }
        }

        static public IList<StockBarDuration> BarDurations
        {
            get { return StockBarDuration.Values; }
        }
        private StockBarDuration barDuration = StockBarDuration.Daily;
        public StockBarDuration BarDuration
        {
            get { return barDuration; }
            set
            {
                if (barDuration != value)
                {
                    barDuration = value;
                    this.OnPropertyChanged("BarDuration");
                }
            }
        }

        public List<string> Groups { get { return StockDictionary.Instance.GetValidGroupNames(); } }

        private string indicatorName;

        public string IndicatorName
        {
            get { return indicatorName; }
            set
            {
                if (indicatorName != value)
                {
                    indicatorName = value;
                    this.OnPropertyChanged("IndicatorName");
                }
            }
        }

        public List<MomentumViewModel> BestTrends { get; private set; }
        public void Perform()
        {
            BestTrends = new List<MomentumViewModel>();
            try
            {
                foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.Group)))
                {
                    int nbBars = 100;
                    if (stockSerie.Initialise())
                    {
                        stockSerie.BarDuration = this.barDuration;
                        if (stockSerie.Count <= nbBars) continue;
                        var indicatorSerie = stockSerie.GetIndicator(indicatorName).Series[0];
                        var maxIndex = indicatorSerie.FindMaxIndex(nbBars, stockSerie.Count - 1);
                        var minIndex = stockSerie.GetSerie(StockDataType.CLOSE).FindMinIndex(maxIndex - nbBars, maxIndex);

                        BestTrends.Add(new MomentumViewModel
                        {
                            StockSerie = stockSerie,
                            BarDuration = StockBarDuration.Weekly,
                            EndDate = stockSerie.Keys.ElementAt(maxIndex),
                            StartDate = stockSerie.Keys.ElementAt(minIndex),
                            Value = indicatorSerie[maxIndex]
                        });
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.OnPropertyChanged("BestTrends");
        }
    }
}
