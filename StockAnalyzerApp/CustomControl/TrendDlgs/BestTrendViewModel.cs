using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
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
            this.Period = 100;
        }

        Groups group;
        public Groups Group
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
        public List<Groups> Groups => StockDictionary.GetValidGroups();

        static public IList<BarDuration> BarDurations => StockBarDuration.BarDurations;
        private BarDuration barDuration;
        public BarDuration BarDuration
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


        private int period;

        public int Period
        {
            get { return period; }
            set
            {
                if (period != value)
                {
                    period = value;
                    this.OnPropertyChanged("Period");
                }
            }
        }

        public List<MomentumViewModel> BestTrends { get; private set; }
        public void Perform()
        {
            BestTrends = new List<MomentumViewModel>();
            try
            {
                foreach (var instrument in StockDictionary.Instruments.Values.Where(s => s.BelongsToGroup(this.Group)))
                {
                    var dataSerie = instrument.GetDataSerie(this.barDuration);
                    if (dataSerie == null || dataSerie.Count < period)
                        continue;

                    var emaSerie = dataSerie.GetIndicator($"EMA({this.period})").Series[0];
                    for (int i = period; i < dataSerie.Count; i++)
                    {
                        if (dataSerie.Values[i - 1].CLOSE < emaSerie[i - 1] && dataSerie.Values[i].CLOSE > emaSerie[i])
                        {
                            int minIndex = i;
                            while (i < dataSerie.Count - 1 && dataSerie.Values[i + 1].CLOSE > emaSerie[i + 1])
                            {
                                i++;
                            }

                            BestTrends.Add(new MomentumViewModel
                            {
                                Instrument = instrument,
                                BarDuration = this.barDuration,
                                StartIndex = minIndex,
                                EndIndex = i,
                                EndDate = dataSerie.Values[i].DATE,
                                StartDate = dataSerie.Values[minIndex].DATE,
                                Value = (dataSerie.Values[i].CLOSE - dataSerie.Values[minIndex].CLOSE) / dataSerie.Values[minIndex].CLOSE
                            });
                        }
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
