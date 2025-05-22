using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BBStopAgent : StockAgentBase
    {
        public BBStopAgent()
        {
            Period = 13;
            BBWidth = 2.0f;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }

        [StockAgentParam(0.5f, 4.0f, 0.5f)]
        public float BBWidth { get; set; }

        public override string Description => "Buy with BBStop";
        public override string DisplayIndicator => $"TRAILSTOP|TRAILBB({Period},{BBWidth},{-BBWidth})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILBB({Period},{BBWidth},{-BBWidth})");
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
