using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.GraphControls.TradeDlgs
{
    public class OpenTradeViewModel : NotifyPropertyChangedBase
    {
        private int entryQty;

        public string StockName { get; set; }

        private void OnEntryChanged()
        {
            this.OnPropertyChanged("EntryQty");
            this.OnPropertyChanged("EntryValue");
            this.OnPropertyChanged("EntryCost");
            this.OnPropertyChanged("Fee");

            this.OnPropertyChanged("TradeRisk");
            this.OnPropertyChanged("PortfolioRisk");
        }

        public int EntryQty
        {
            get => entryQty;
            set
            {
                if (entryQty != value)
                {
                    entryQty = value;
                    OnEntryChanged();
                }
            }
        }
        public float EntryValue { get; set; }
        public float EntryCost => EntryQty * EntryValue + Fee;
        public float Fee => (EntryQty * EntryValue) < 1000f ? 2.5f : 5.0f;
        public float StopValue { get; set; }
        public float TradeRisk => (EntryValue - StopValue) / EntryValue;
        public float PortfolioRisk => (this.Portfolio.TotalValue - ((EntryValue - StopValue) * EntryQty)) / this.Portfolio.TotalValue;
        public DateTime EntryDate { get; set; }
        public StockBarDuration BarDuration { get; set; }
        public string IndicatorName { get; set; }
        public string EntryComment { get; set; }
        public static Array BarDurations => Enum.GetValues(typeof(BarDuration));
        public static IList<int> LineBreaks => new List<int> { 0, 1, 2, 3, 4, 5 };
        public IList<string> IndicatorNames { get; set; }
        public StockPortfolio Portfolio { get; set; }

        public void SetIndicatorsFromTheme(Dictionary<string, List<string>> theme)
        {
            this.IndicatorNames = new List<string>();
            foreach (var section in theme)
            {
                foreach (var line in section.Value.Where(l => l.StartsWith("INDICATOR") || l.StartsWith("CLOUD") || l.StartsWith("PAINTBAR") || l.StartsWith("TRAILSTOP") || l.StartsWith("DECORATOR") || l.StartsWith("TRAIL")))
                {
                    var fields = line.Split('|');
                    this.IndicatorNames.Add($"{fields[0]}|{fields[1]}");
                }
            }
            this.IndicatorName = this.IndicatorNames.FirstOrDefault();
        }
    }
}
