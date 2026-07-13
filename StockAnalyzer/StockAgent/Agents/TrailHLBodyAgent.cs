using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailHLBodyAgent : StockAgentBase
    {
        public TrailHLBodyAgent()
        {
            Period = 13;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }

        public override string Description => "Buy with TrailHLBodyAgent Stop";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILHLBODY({Period})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init()
        {
            if (DataSerie.Count < Period)
                return false;
            trailStop = DataSerie.GetTrailStop($"TRAILHLBODY({Period})");
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
