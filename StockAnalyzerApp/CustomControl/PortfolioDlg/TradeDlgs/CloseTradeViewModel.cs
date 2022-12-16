using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public class CloseTradeViewModel : NotifyPropertyChangedBase
    {
        private int exitQty;
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

        internal void Refresh()
        {
            this.Portfolio.Refresh();
            this.OnPropertyChanged("Portfolio");
            this.OnExitChanged();
        }

        public StockSerie StockSerie { get; set; }

        public int ExitQty
        {
            get => exitQty;
            set
            {
                if (exitQty != value)
                {
                    exitQty = value;
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

        public float Return => this.ExitAmount - this.Position.EntryCost;
        public float ReturnPercent => (this.ExitAmount - this.Position.EntryCost) / this.Position.EntryCost;
        public float PortfolioReturnPercent => this.Return / this.Portfolio.TotalValue;

        public string ExitComment { get; set; }
        public StockPortfolio Portfolio { get; set; }
        public StockPosition Position { get; set; }
    }
}
