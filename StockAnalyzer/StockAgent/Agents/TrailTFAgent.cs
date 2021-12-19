using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailTFAgent : StockAgentBase
    {
        public TrailTFAgent()
        {
            TrailPeriod = 13;
            Trigger = 20;
        }

        [StockAgentParam(5, 80, 5)]
        public int Trigger { get; set; }

        [StockAgentParam(5, 80, 5)]
        public int TrailPeriod { get; set; }

        public override string Description => "Buy with TrailTF Stop";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILTF({Trigger},{TrailPeriod / 2})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < TrailPeriod)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILTF({TrailPeriod},{TrailPeriod / 2})");
            bullEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenUp")];
            bearEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenDown")];
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
