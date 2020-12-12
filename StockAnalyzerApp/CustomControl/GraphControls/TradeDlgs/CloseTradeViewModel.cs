using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using System;

namespace StockAnalyzerApp.CustomControl.GraphControls.TradeDlgs
{
    public class CloseTradeViewModel : NotifyPropertyChangedBase
    {
        private int entryQty;
        private float entryValue;

        public string StockName { get; set; }

        private void OnExitChanged()
        {
            this.OnPropertyChanged("ExitQty");
            this.OnPropertyChanged("ExitValue");
            this.OnPropertyChanged("ExitAmount");
            this.OnPropertyChanged("Fee");

            this.OnPropertyChanged("Return");
            this.OnPropertyChanged("ReturnPercent");
            this.OnPropertyChanged("PortfolioReturnPercent");
        }

        public int ExitQty
        {
            get => entryQty;
            set
            {
                if (entryQty != value)
                {
                    entryQty = value;
                    OnExitChanged();
                }
            }
        }
        public float ExitValue
        {
            get => entryValue;
            set
            {
                if (entryValue != value)
                {
                    entryValue = value;
                    OnExitChanged();
                }
            }
        }
        public float ExitAmount => ExitQty * ExitValue - Fee;
        public float NetExitValue => ExitAmount / ExitQty;
        public float Fee => (ExitQty * ExitValue) < 1000f ? 2.5f : 5.0f;
        public float Return => (this.ExitAmount - this.Position.EntryNetValue * this.Position.EntryQty);
        public float ReturnPercent => (this.ExitAmount - this.Position.EntryNetValue * this.Position.EntryQty) / (this.Position.EntryNetValue * this.Position.EntryQty);
        public float PortfolioReturnPercent => this.Return / this.Portfolio.TotalValue;
        public DateTime ExitDate { get; set; }
        public string ExitComment { get; set; }
        public StockPortfolio Portfolio { get; set; }
        public StockPosition Position { get; set; }
    }
}
