using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BBTFAgent : StockAgentBase
    {
        public BBTFAgent()
        {
            Period = 13;
            BBWidth = 2.0f;
        }

        [StockAgentParam(5, 60)]
        public int Period { get; set; }

        [StockAgentParam(0.75f, 3.0f)]
        public float BBWidth { get; set; }

        public override string Description => "Buy with BBTF signals";
        public override string DisplayIndicator => $"TRAILSTOP|TRAILBBTF({Period},{BBWidth},{-BBWidth},MA)";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILBBTF({Period},{BBWidth},{-BBWidth},MA)");
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
