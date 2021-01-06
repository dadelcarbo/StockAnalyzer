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
        static public Array BarDurations
        {
            get { return Enum.GetValues(typeof(BarDuration)); }
        }

        private BarDuration barDuration;
        public BarDuration BarDuration { get { return barDuration; } set { if (value != barDuration) { barDuration = value; OnPropertyChanged("BarDuration"); } } }

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

        public List<PalmaresLine> Lines { get; set; }

        public PalmaresViewModel()
        {
            Indicator1 = "ROR(100,1)";
            Indicator2 = "HIGHEST(20)";
            this.BarDuration = BarDuration.Daily;
            this.Group = StockSerie.Groups.COUNTRY;
            this.Lines = new List<PalmaresLine>();
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

                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                var openSerie = stockSerie.GetSerie(StockDataType.OPEN);

                Lines.Add(new PalmaresLine
                {
                    Serie = stockSerie.StockName,
                    Value = closeSerie.Last,
                    Indicator1 = stockIndicator1,
                    Indicator2 = stockIndicator2,
                    Variation = 0
                });

                stockSerie.BarDuration = previousDuration;
            }

            OnPropertyChanged("Lines");
            return true;
        }
    }
}
