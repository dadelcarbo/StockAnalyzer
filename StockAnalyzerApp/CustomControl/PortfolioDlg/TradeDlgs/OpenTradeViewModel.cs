using StockAnalyzer;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using StockAnalyzerApp.CustomControl.GraphControls;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public class OpenTradeViewModel : NotifyPropertyChangedBase
    {
        private int entryQty;
        private float entryValue;
        private float stopValue;
        private bool marketOrder = true;
        private bool limitOrder;
        private bool thresholdOrder;

        public StockSerie StockSerie { get; set; }

        private void OnEntryChanged()
        {
            this.OnPropertyChanged("EntryQty");
            this.OnPropertyChanged("EntryValue");
            this.OnPropertyChanged("EntryCost");
            this.OnPropertyChanged("Fee");

            this.OnPropertyChanged("StopValue");
            this.OnPropertyChanged("Risk");
            this.OnPropertyChanged("TradeRisk");
            this.OnPropertyChanged("PortfolioRisk");
            this.OnPropertyChanged("PortfolioPercent");

            this.OnPropertyChanged("IsTradeRisky");
            this.OnPropertyChanged("IsPortfolioRisky");
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
        public float EntryValue
        {
            get => entryValue;
            set
            {
                if (entryValue != value)
                {
                    entryValue = value;
                    OnEntryChanged();
                }
            }
        }
        public bool MarketOrder { get => marketOrder; set { marketOrder = value; this.OnPropertyChanged("IsValueEditable"); } }
        public bool LimitOrder { get => limitOrder; set { limitOrder = value; this.OnPropertyChanged("IsValueEditable"); } }
        public bool ThresholdOrder { get => thresholdOrder; set { thresholdOrder = value; this.OnPropertyChanged("IsValueEditable"); } }
        public bool IsValueEditable => !this.marketOrder;

        public float EntryCost => EntryQty * EntryValue + Fee;
        public float Fee => (EntryQty * EntryValue) < 1000f ? 2.5f : 5.0f;
        public float StopValue
        {
            get => stopValue;
            set
            {
                if (stopValue != value)
                {
                    stopValue = value;
                    OnEntryChanged();
                }
            }
        }
        public float Risk => 1 + (this.PortfolioRisk - this.Portfolio.MaxRisk) / this.Portfolio.MaxRisk;
        public float TradeRisk => (EntryValue - StopValue) / EntryValue;
        public float PortfolioPercent => 1f - (this.Portfolio.TotalValue - this.EntryCost) / this.Portfolio.TotalValue;
        public float PortfolioRisk => (EntryValue - StopValue) * EntryQty / this.Portfolio.TotalValue;
        public float PortfolioReturn => (this.Portfolio.TotalValue - this.Portfolio.InitialBalance) / this.Portfolio.TotalValue;
        public StockBarDuration BarDuration { get; set; }
        public string Theme { get; set; }
        public string EntryComment { get; set; }
        public static Array BarDurations => Enum.GetValues(typeof(BarDuration));
        public static IList<int> LineBreaks => new List<int> { 0, 1, 2, 3, 4, 5 };
        public IEnumerable<string> Themes { get; set; }
        public StockPortfolio Portfolio { get; set; }

        public bool IsTradeRisky => PortfolioPercent > this.Portfolio.MaxPositionSize;
        public bool IsPortfolioRisky => PortfolioRisk > this.Portfolio.MaxRisk;

        public void OnStopValueChanged(FullGraphUserControl sender, DateTime date, float value, bool crossMode)
        {
            if (crossMode)
                this.StopValue = value;
        }
        public void CalculatePositionSize()
        {
            // Calculate position size according to money management
            var qty = (int)Math.Ceiling(this.Portfolio.MaxRisk * this.Portfolio.TotalValue / (this.EntryValue - this.StopValue));
            qty = Math.Min(qty, (int)(this.Portfolio.MaxPositionSize * this.Portfolio.TotalValue / this.EntryValue));
            this.EntryQty = qty;
        }

        public void Refresh()
        {
            this.Portfolio.Refresh();
            this.OnPropertyChanged("Portfolio");
            this.CalculatePositionSize();
        }
    }
}
