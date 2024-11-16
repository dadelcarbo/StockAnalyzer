using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailSTOKAgent : StockAgentBase
    {
        public TrailSTOKAgent()
        {
            Period = 13;
            Threeshold = 2;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }

        [StockAgentParam(0f, 1f, 0.1f)]
        public float Threeshold { get; set; }

        public override string Description => "Buy with TrailSTOK Stop";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILSTOK({Period},{Threeshold})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count <= Period)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILSTOK({Period},{Threeshold})");
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
            if (bearEvents[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
