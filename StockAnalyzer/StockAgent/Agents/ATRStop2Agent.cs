using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class ATRStop2Agent : StockAgentBase
    {
        public ATRStop2Agent()
        {
            Period = 12;
            UpWidth = 2.0f;
            DownWidth = 2.0f;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }
        [StockAgentParam(0.5f, 4.0f, 0.5f)]
        public float UpWidth { get; set; }
        [StockAgentParam(0.5f, 4.0f, 0.5f)]
        public float DownWidth { get; set; }

        public override string Description => "Buy according to TrailATR with different up and down width";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILATR({Period},{UpWidth},{-DownWidth},EMA)";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILATR({Period},{UpWidth},{-DownWidth},EMA)");
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
