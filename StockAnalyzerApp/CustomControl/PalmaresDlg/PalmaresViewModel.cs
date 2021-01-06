using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresViewModel : NotifyPropertyChangedBase
    {
        static public Array Groups
        {
            get { return Enum.GetValues(typeof(StockSerie.Groups)); }
        }

        private StockSerie.Groups group;
        public StockSerie.Groups Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {
                    group = value;
                    OnPropertyChanged("Group");
                }
            }
        }

        private StockBarDuration barDuration;
        public StockBarDuration BarDuration { get { return barDuration; } set { if (value != barDuration) { barDuration = value; OnPropertyChanged("BarDuration"); } } }

        private string indicator1;
        public string Indicator1
        {
            get { return indicator1; }
            set
            {
                if (value != indicator1)
                {
                    indicator1 = value;
                    OnPropertyChanged("Indicator1");
                }
            }
        }
        private string indicator2;
        public string Indicator2
        {
            get { return indicator2; }
            set
            {
                if (value != indicator2)
                {
                    indicator2 = value;
                    OnPropertyChanged("Indicator2");
                }
            }
        }

        private DateTime fromDate;
        public DateTime FromDate
        {
            get { return fromDate; }
            set
            {
                if (value != fromDate)
                {
                    fromDate = value;
                    OnPropertyChanged("FromDate");
                }
            }
        }

        private DateTime toDate;
        public DateTime ToDate
        {
            get { return toDate; }
            set
            {
                if (value != toDate)
                {
                    toDate = value;
                    OnPropertyChanged("ToDate");
                }
            }
        }


        public List<PalmaresLine> Lines { get; set; }

        public PalmaresViewModel()
        {
            Indicator1 = "ROR(100,1)";
            Indicator2 = "HIGHEST(20)";
            this.Group = StockSerie.Groups.COUNTRY;
            this.Lines = new List<PalmaresLine>();
            this.ToDate = DateTime.Now;
            this.FromDate = new DateTime(this.ToDate.Year, 1, 1);
        }
        public bool Calculate()
        {
            #region Sanity Check
            IStockIndicator viewableSeries1 = null;
            if (!string.IsNullOrEmpty(this.indicator1))
            {
                try
                {
                    viewableSeries1 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator1) as IStockIndicator;
                }
                catch { }
            }
            IStockIndicator viewableSeries2 = null;
            if (!string.IsNullOrEmpty(this.indicator2))
            {
                try
                {
                    viewableSeries2 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator2) as IStockIndicator;
                }
                catch { }
            }
            #endregion

            Lines = new List<PalmaresLine>();
            foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.group)))
            {
                if (!stockSerie.Initialise())
                    continue;
                var previousDuration = stockSerie.BarDuration;
                stockSerie.BarDuration = this.barDuration;

                #region Calculate Indicators
                float stockIndicator1 = float.NaN;
                if (viewableSeries1 != null)
                {
                    try { viewableSeries1.ApplyTo(stockSerie); stockIndicator1 = viewableSeries1.Series[0].Last; } catch { }
                }
                float stockIndicator2 = float.NaN;
                if (viewableSeries2 != null)
                {
                    try { viewableSeries2.ApplyTo(stockSerie); stockIndicator2 = viewableSeries2.Series[0].Last; } catch { }
                }
                #endregion
                #region Calculate 
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                var openSerie = stockSerie.GetSerie(StockDataType.OPEN);

                var startIndex = stockSerie.IndexOfFirstGreaterOrEquals(this.FromDate);
                if (startIndex == -1)
                {
                    continue;
                }
                var endIndex = stockSerie.IndexOfFirstLowerOrEquals(this.ToDate);
                if (endIndex == -1)
                {
                    continue;
                }

                float lastValue = closeSerie[endIndex];
                float firstValue = closeSerie[startIndex];
                float variation = (lastValue - firstValue) / firstValue;

                #endregion

                Lines.Add(new PalmaresLine
                {
                    Name = stockSerie.StockName,
                    Value = lastValue,
                    Indicator1 = stockIndicator1,
                    Indicator2 = stockIndicator2,
                    Variation = variation
                });

                stockSerie.BarDuration = previousDuration;
            }

            OnPropertyChanged("Lines");
            return true;
        }
    }
}
