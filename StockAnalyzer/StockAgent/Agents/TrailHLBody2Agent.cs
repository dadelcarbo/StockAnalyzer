using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailHLBody2Agent : StockAgentBase
    {
        public TrailHLBody2Agent()
        {
            PeriodUp = 13;
            PeriodDown = 13;
        }

        [StockAgentParam(2, 80, 1)]
        public int PeriodUp { get; set; }
        [StockAgentParam(2, 80, 1)]
        public int PeriodDown { get; set; }

        public override string Description => "Buy with TrailHLBodyAgent Stop";

        public override string DisplayIndicator => $"TRAILSTOP|TRAILHLBODY2({PeriodUp},{PeriodDown})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Math.Max(PeriodUp, PeriodDown))
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILHLBODY2({PeriodUp},{PeriodDown})");
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
