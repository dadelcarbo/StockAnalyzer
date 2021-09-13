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
            StopATR = 2;
            //TrailATR = 6;
        }

        [StockAgentParam(20, 80)]
        public int Trigger { get; set; }

        [StockAgentParam(0.5f, 10f)]
        public float StopATR2 { get; set; }

       // [StockAgentParam(0.5f, 10f)]
        public float TrailATR => StopATR2 * 3.0f;
        //public float TrailATR { get; set; }

        public override string Description => "Buy with TrailHIGHESTATRAgent Stop";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILHIGHESTATR({Trigger},{StopATR2},{TrailATR})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Trigger)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILHIGHESTATR({Trigger},{StopATR2},{TrailATR})");
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
