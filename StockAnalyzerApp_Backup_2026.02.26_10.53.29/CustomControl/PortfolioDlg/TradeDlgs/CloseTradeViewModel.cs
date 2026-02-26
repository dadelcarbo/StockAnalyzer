using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
using StockAnalyzerApp.CustomControl.GraphControls;
using System;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public class CloseTradeViewModel : NotifyPropertyChangedBase
    {
        #region EVENTS
        public delegate void OrdersChangedHandler();
        public event OrdersChangedHandler OrdersChanged;
        public void RaiseOrdersChanged() { this.OrdersChanged?.Invoke(); }
        #endregion

        private int exitQty;
        private float exitValue;
        private bool marketOrder = true;
        private bool limitOrder;
        private bool thresholdOrder;

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

        public bool MarketOrder { get => marketOrder; set { marketOrder = value; if (value) this.ExitValue = this.StockSerie.LastValue.CLOSE; this.OnPropertyChanged("IsValueEditable"); } }
        public bool LimitOrder { get => limitOrder; set { limitOrder = value; if (value) this.ExitValue = this.StockSerie.LastValue.HIGH; this.OnPropertyChanged("IsValueEditable"); } }
        public bool ThresholdOrder { get => thresholdOrder; set { thresholdOrder = value; if (value) this.ExitValue = this.StockSerie.LastValue.LOW; this.OnPropertyChanged("IsValueEditable"); } }
        public bool IsValueEditable => !this.marketOrder;

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
            get => exitValue;
            set
            {
                if (exitValue != value)
                {
                    exitValue = value;
                    OnExitChanged();
                }
            }
        }
        public float ExitAmount => ExitQty * ExitValue - Fee;
        public float NetExitValue => ExitAmount / ExitQty;
        public float Fee => Math.Max(2f, (ExitQty * ExitValue) * 0.0008f);

        public float Return => this.ExitAmount - this.Position.EntryCost;
        public float ReturnPercent => (this.ExitAmount - this.Position.EntryCost) / this.Position.EntryCost;
        public float PortfolioReturnPercent => this.Return / this.Portfolio.TotalValue;

        public string ExitComment { get; set; }
        public StockPortfolio Portfolio { get; set; }
        public StockPosition Position { get; set; }

        #region Tick Size Management

        public void CalculateTickSize()
        {
            var instrumentDetails = this.Portfolio.GetInstrumentDetails(this.StockSerie);
            if (instrumentDetails == null)
            {
                return;
            }

            this.SmallChange = (float)instrumentDetails.GetTickSize(this.ExitValue);
            this.ExitValue = (float)instrumentDetails.RoundToTickSize(this.ExitValue);
        }

        private float smallChange = 0.01f;
        public float SmallChange { get { return smallChange; } set { smallChange = value; this.OnPropertyChanged("SmallChange"); this.OnPropertyChanged("LargeChange"); this.OnPropertyChanged("NbDecimals"); } }
        public float LargeChange => smallChange * 10;
        public int NbDecimals => Math.Max(0, (int)Math.Ceiling(Math.Round(-Math.Log10(this.SmallChange), 4)));
        #endregion

        public void OnOrderValueChanged(FullGraphUserControl sender, DateTime date, float value, bool crossMode)
        {
            if (crossMode)
                this.ExitValue = value;
        }
    }
}
