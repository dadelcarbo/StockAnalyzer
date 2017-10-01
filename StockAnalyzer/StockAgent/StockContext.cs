using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent
{
    public delegate void SerieChangedHandler();

    public class StockContext
    {
        public int CurrentIndex { get; set; }
        private StockSerie serie;
        public StockSerie Serie
        {
            get { return serie; }
            set
            {
                if (serie != value)
                {
                    this.serie = value;
                    if (this.OnSerieChanged != null) this.OnSerieChanged();
                }
            }
        }

        public StockTrade Trade { get; set; }

        public event SerieChangedHandler OnSerieChanged;

        public StockContext()
        {
            this.TradeLog = new List<StockTrade>();
        }
        public List<StockTrade> TradeLog { get; set; }

        public void OpenTrade(int entryIndex, bool isLong = true)
        {
            if (entryIndex >= this.Serie.Count) return;

            this.Trade = new StockTrade(this.Serie, entryIndex, isLong);
            this.TradeLog.Add(this.Trade);
        }

        public void CloseTrade(int exitIndex)
        {
            if (this.Trade == null)
                throw new InvalidOperationException("Cannot close the trade as it's not opened");
            this.Trade.Close(exitIndex);
            this.Trade = null;
        }

        public StockTradeSummary GetTradeSummary()
        {
            return new StockTradeSummary(this);
        }

        public void Clear()
        {
            this.Trade = null;
            this.TradeLog = new List<StockTrade>();
            this.Serie = null;
        }
    }
}
