using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailEMATFAgent : StockAgentBase
    {
        public TrailEMATFAgent()
        {
            Period = 13;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }
        [StockAgentParam(0.5f, 4.0f, 0.5f)]
        public float NbATR { get; set; }

        public override string Description => "Buy with TRAILEMATF Stop";
        public override string DisplayIndicator => $"TRAILSTOP|TRAILEMATF({Period},{NbATR},False)";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILEMATF({Period},{NbATR},False)");
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
            if (bearEvents[index]) // bar fast below slow EMATF
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
