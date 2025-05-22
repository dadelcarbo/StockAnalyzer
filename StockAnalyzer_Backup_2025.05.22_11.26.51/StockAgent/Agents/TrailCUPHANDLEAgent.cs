using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailCUPHANDLEAgent : StockAgentBase
    {
        public TrailCUPHANDLEAgent()
        {
            Period = 13;
            HL = 0;
        }

        [StockAgentParam(2, 120, 1)]
        public int Period { get; set; }

        [StockAgentParam(0, 1, 1)]
        public int HL { get; set; }

        public override string Description => "Buy with TRAIL CUP & HANDLE";
        public override string DisplayIndicator => $"TRAILSTOP|TRAILCUPEMA(4,{HL > 0.5f},{Period})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILCUPEMA(4,{HL > 0.5f},{Period})");
            bullEvents = trailStop.Events[Array.IndexOf(trailStop.EventNames, "BrokenUp")];
            bearEvents = trailStop.Events[Array.IndexOf(trailStop.EventNames, "BrokenDown")];
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
