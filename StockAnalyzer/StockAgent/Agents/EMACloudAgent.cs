using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMACloudAgent : StockAgentBase
    {
        public EMACloudAgent()
        {
            FastPeriod = 13;
        }

        [StockAgentParam(15, 25)]
        public int FastPeriod { get; set; }

        [StockAgentParam(40, 60)]
        public int SlowPeriod { get; set; }

        [StockAgentParam(6, 12)]
        public int SignalPeriod { get; set; }

        public override string Description => "Buy when Open and close are above EMA";

        public override string DisplayIndicator => $"CLOUD|EMA2Lines({FastPeriod},{SlowPeriod},{SignalPeriod})";

        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Math.Max(SlowPeriod, FastPeriod))
                return false;
            var cloud = stockSerie.GetCloud($"EMA2Lines({FastPeriod},{SlowPeriod},{SignalPeriod})");
            bullEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "BrokenUp")];
            bearEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "BrokenDown")];
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
