using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMA2LinesAgent : StockAgentBase
    {
        public EMA2LinesAgent()
        {
            FastPeriod = 13;
        }

        [StockAgentParam(5, 80, 5)]
        public int FastPeriod { get; set; }

        [StockAgentParam(5, 80, 5)]
        public int SlowPeriod { get; set; }

        public int SignalPeriod => 1;

        public override string Description => "Buy when fast EMA closes above slow EMA";

        public override string DisplayIndicator => $"CLOUD|EMA2Lines({FastPeriod},{SlowPeriod},{SignalPeriod})";

        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Math.Max(SlowPeriod, FastPeriod))
                return false;
            var cloud = stockSerie.GetCloud($"EMA2Lines({FastPeriod},{SlowPeriod},{SignalPeriod})");
            bullEvents = cloud.Events[Array.IndexOf(cloud.EventNames, "CloudUp")];
            bearEvents = cloud.Events[Array.IndexOf(cloud.EventNames, "CloudDown")];
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
