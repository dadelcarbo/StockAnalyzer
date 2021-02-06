using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailHL2Agent : StockAgentBase
    {
        public TrailHL2Agent()
        {
            Period = 13;
        }

        [StockAgentParam(2, 80)]
        public int Period { get; set; }

        public override string Description => "Buy with TRAILHL2SR Stop";

        IStockEvent stockEvents;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            stockEvents = stockSerie.GetIndicator($"TRAILHL2SR({Period})");
            bullEvents = stockEvents.Events[Array.IndexOf<string>(stockEvents.EventNames, "BullStart")];
            bearEvents = stockEvents.Events[Array.IndexOf<string>(stockEvents.EventNames, "BullEnd")];
            return bullEvents != null && bearEvents != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (bullEvents[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (bearEvents[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
