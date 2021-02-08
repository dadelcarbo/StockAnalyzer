using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TFMansfieldAgent : StockAgentBase
    {
        public TFMansfieldAgent()
        {
            Period = 13;
        }

        [StockAgentParam(2, 20)]
        public int Period { get; set; }

        public override string Description => "Buy when MANSFIELD turn positive";

        public override string DisplayIndicator => $"INDICATOR|MANSFIELD({Period})";

        FloatSerie mansfield;

        protected override bool Init(StockSerie stockSerie)
        {
            mansfield = stockSerie.GetIndicator($"MANSFIELD({Period})").Series[0];
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (mansfield[index] > 0)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (mansfield[index] < 0)
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
