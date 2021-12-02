using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;
using System.Linq;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TOPEMAReentryAgent : StockAgentBase
    {
        public TOPEMAReentryAgent()
        {
            Period = 13;
        }

        [StockAgentParam(10, 40)]
        public int Period { get; set; }

        public override string Description => "Buy when TOPEMA signals reentry and sell when TOPEMA Support broken";

        public override string DisplayIndicator => $"TRAILSTOP|TOPEMA({Period})";

        FloatSerie supportSerie;
        FloatSerie resistanceSerie;
        IStockEvent events;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            events = stockSerie.GetIndicator($"TOPEMA({Period})");
            supportSerie = stockSerie.GetIndicator($"TOPEMA({Period})").Series[0];
            resistanceSerie = stockSerie.GetIndicator($"TOPEMA({Period})").Series[1];

            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (events.Events[2][index] && !events.Events[4][index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (events.Events[3][index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
