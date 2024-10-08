﻿using StockAnalyzer;
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
            this.Period = 100;
        }

        string group;
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

        public List<StockSerie.Groups> Groups => StockDictionary.Instance.GetValidGroups();

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
                foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.Group)))
                {
                    if (stockSerie.Initialise())
                    {
                        stockSerie.BarDuration = this.barDuration;
                        if (stockSerie.Count <= period) continue;
                        var indicatorSerie = stockSerie.GetIndicator($"ROR({this.period})").Series[0];
                        var maxIndex = indicatorSerie.FindMaxIndex(period, stockSerie.Count - 1);
                        var minIndex = stockSerie.GetSerie(StockDataType.CLOSE).FindMinIndex(maxIndex - period, maxIndex);

                        BestTrends.Add(new MomentumViewModel
                        {
                            StockSerie = stockSerie,
                            BarDuration = this.barDuration,
                            StartIndex = minIndex,
                            EndIndex = maxIndex,
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
