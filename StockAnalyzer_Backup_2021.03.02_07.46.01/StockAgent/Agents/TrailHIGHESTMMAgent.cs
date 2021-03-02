using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailHIGHESTMMAgent : StockAgentMoneyManagedBase
    {
        public TrailHIGHESTMMAgent()
        {
            Trigger = 13;
        }

        [StockAgentParam(20, 160)]
        public int Trigger { get; set; }

        public override string Description => "Buy with TRAILHIGHEST Stop";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILHIGHEST({Trigger},{Trigger / 2})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Trigger)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILHIGHEST({Trigger},{Trigger / 2})");
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
