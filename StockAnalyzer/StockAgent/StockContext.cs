using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent
{
    public class StockContext
    {
        public int CurrentIndex { get; set; }
     
        public StockTrade Trade { get; set; }

        public StockContext()
        {
            this.TradeLog = new List<StockTrade>();
        }
        public List<StockTrade> TradeLog { get; set; }

        public void OpenTrade(StockSerie serie, int entryIndex, bool isLong = true)
        {
            if (entryIndex >= serie.Count) return;

            this.Trade = new StockTrade(serie, entryIndex, isLong);
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
        }
    }
}
