using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
using StockAnalyzerApp.CustomControl.GraphControls;
using System;
using System.Collections.Generic;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public class OpenTradeViewModel : NotifyPropertyChangedBase
    {
        #region EVENTS
        public delegate void OrdersChangedHandler();
        public event OrdersChangedHandler OrdersChanged;
        public void RaiseOrdersChanged() { this.OrdersChanged?.Invoke(); }
        #endregion

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
            this.OnPropertyChanged("IsExceedingCash");
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
                    if (LimitOrder)
                    {
                        entryValue = Math.Min(value, this.entryValue = this.StockSerie.LastValue.CLOSE);
                    }
                    else
                    {
                        if (ThresholdOrder)
                        {
                            // Find long reentry
                            entryValue = this.LongReentry != 0 ? this.LongReentry : entryValue = Math.Max(value, this.entryValue = this.StockSerie.LastValue.CLOSE);
                            this.LongReentry = 0;
                        }
                        else
                        {
                            this.entryValue = this.StockSerie.LastValue.CLOSE;
                        }
                    }
                    OnEntryChanged();
                    this.OrdersChanged?.Invoke();
                }
            }
        }
        public bool MarketOrder { get => marketOrder; set { marketOrder = value; if (value) this.EntryValue = this.StockSerie.LastValue.CLOSE; this.OnPropertyChanged("IsValueEditable"); } }
        public bool LimitOrder { get => limitOrder; set { limitOrder = value; if (value) this.EntryValue = this.StockSerie.LastValue.LOW; this.OnPropertyChanged("IsValueEditable"); } }
        public bool ThresholdOrder { get => thresholdOrder; set { thresholdOrder = value; if (value) this.EntryValue = this.StockSerie.LastValue.HIGH; this.OnPropertyChanged("IsValueEditable"); } }
        public bool IsValueEditable => !this.marketOrder;

        /// <summary>
        /// Indicates if sell 50% at T1
        /// </summary>
        public bool T1 { get; set; }


        public float EntryCost => EntryQty * EntryValue + Fee;
        public float Fee => Math.Max(2f, EntryQty * EntryValue * 0.0008f);

        public float StopValue
        {
            get => stopValue;
            set
            {
                if (stopValue != value)
                {
                    stopValue = Math.Min(value, this.stopValue = this.StockSerie.LastValue.CLOSE);
                    OnEntryChanged();
                    this.OrdersChanged?.Invoke();
                }
            }
        }
        public float LongReentry { get; set; }
        public float Risk => 1 + (this.PortfolioRisk - this.Portfolio.DynamicRisk) / this.Portfolio.DynamicRisk;
        public float TradeRisk => (EntryValue - StopValue) / EntryValue;
        public float PortfolioPercent => 1f - (this.Portfolio.TotalValue - this.EntryCost) / this.Portfolio.TotalValue;
        public float PortfolioRisk => (EntryValue - StopValue) * EntryQty / this.Portfolio.TotalValue;
        public float PortfolioReturn => (this.Portfolio.TotalValue - this.Portfolio.InitialBalance) / this.Portfolio.TotalValue;
        public BarDuration BarDuration { get; set; }
        public string Theme { get; set; }
        public string EntryComment { get; set; }
        public IEnumerable<string> Themes { get; set; }
        public StockPortfolio Portfolio { get; set; }
        public string Symbol { get; set; }

        #region Tick Size Management
        private float smallChange = 0.01f;
        public float SmallChange { get { return smallChange; } set { smallChange = value; this.OnPropertyChanged("SmallChange"); this.OnPropertyChanged("LargeChange"); this.OnPropertyChanged("NbDecimals"); } }
        public float LargeChange => smallChange * 10;
        public int NbDecimals => Math.Max(0, (int)Math.Ceiling(Math.Round(-Math.Log10(this.SmallChange), 4)));
        #endregion

        public bool IsTradeRisky => PortfolioRisk > this.Portfolio.DynamicRisk;
        public bool IsPortfolioRisky => PortfolioPercent > this.Portfolio.MaxPositionSize;
        public bool IsExceedingCash => this.Portfolio.Balance < this.EntryCost;

        public void OnOrderValueChanged(FullGraphUserControl sender, DateTime date, float value, bool crossMode)
        {
            if (crossMode)
                this.StopValue = value;
        }
        public void CalculatePositionSize()
        {
            CalculateTickSize();

            // Calculate position size according to money management
            var qty = (int)Math.Floor(this.Portfolio.DynamicRisk * this.Portfolio.TotalValue / (this.EntryValue - this.StopValue));
            qty = Math.Min(qty, (int)(this.Portfolio.MaxPositionSize * this.Portfolio.TotalValue / this.EntryValue));
            this.EntryQty = qty;
        }
        private void CalculateTickSize()
        {
            var instrumentDetails = this.Portfolio.GetInstrumentDetails(this.StockSerie);
            if (instrumentDetails == null)
            {
                return;
            }

            this.SmallChange = (float)instrumentDetails.GetTickSize(this.StopValue);
            this.StopValue = (float)instrumentDetails.RoundToTickSize(this.StopValue);
            this.EntryValue = (float)instrumentDetails.RoundToTickSize(this.EntryValue);
            this.Symbol = instrumentDetails.Symbol;
        }
        public void Refresh(bool forceLogin)
        {
            if (forceLogin)
            {
                this.Portfolio.SaxoLogin();
            }
            if (this.Portfolio.SaxoSilentLogin())
            {
                this.Portfolio.Refresh();
            }
            this.OnPropertyChanged("Portfolio");
            this.CalculatePositionSize();
        }
    }
}
