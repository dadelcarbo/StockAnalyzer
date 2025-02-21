using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailHIGHESTATRAgent : StockAgentBase
    {
        public TrailHIGHESTATRAgent()
        {
            Trigger = 13;
            TrailATR = 2;
        }

        [StockAgentParam(5, 80, 5)]
        public int Trigger { get; set; }

        [StockAgentParam(1f, 10f, 0.5f)]
        public float TrailATR { get; set; }

        [StockAgentParam(1f, 10f, 0.5f)]
        public float StopATR { get; set; }

        public override string Description => "Buy with TrailHIGHESTATRAgent Stop";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILHIGHESTATR({Trigger},{StopATR},{TrailATR})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Trigger)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILHIGHESTATR({Trigger},{StopATR},{TrailATR})");
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
            if (bearEvents[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
