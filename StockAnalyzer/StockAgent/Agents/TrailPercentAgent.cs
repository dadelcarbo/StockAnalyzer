using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailPercentAgent : StockAgentBase
    {
        public TrailPercentAgent()
        {
            BuyPercent = 0.2f;
            SellPercent = 0.2f;
        }

        [StockAgentParam(0.1f, 0.50f, 0.1f)]
        public float BuyPercent { get; set; }
        [StockAgentParam(0.1f, 0.50f, 0.1f)]
        public float SellPercent { get; set; }

        public override string Description => "Buy with TRAILPercent Stop";
        public override string DisplayIndicator => $"TRAILSTOP|TRAILPERCENT({BuyPercent},{SellPercent})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < 50)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILPERCENT({BuyPercent},{SellPercent})");
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
