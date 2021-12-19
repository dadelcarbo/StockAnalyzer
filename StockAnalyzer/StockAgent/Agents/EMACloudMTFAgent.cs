using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMACloudMTFAgent : StockAgentBase
    {
        public EMACloudMTFAgent()
        {
            FastPeriod = 13;
            SlowPeriod = FastPeriod * 2;
        }

        [StockAgentParam(15, 25, 1)]
        public int FastPeriod { get; set; }

        [StockAgentParam(40, 60, 1)]
        public int SlowPeriod { get; set; }

        [StockAgentParam(6, 12, 1)]
        public int SignalPeriod { get; set; }

        public override string Description => "Buy when using EMA Cloud and filter signals on a longer time frame ";

        BoolSerie filterEvents;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Math.Max(SlowPeriod, FastPeriod))
                return false;
            var cloud = stockSerie.GetCloud($"EMA2Lines({FastPeriod},{SlowPeriod},{SignalPeriod})");
            bullEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "BrokenUp")];
            bearEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "BrokenDown")];
            var filterCloud = stockSerie.GetCloud($"EMA2Lines({FastPeriod * 5},{SlowPeriod * 5},{SignalPeriod * 5})");
            filterEvents = filterCloud.Events[Array.IndexOf<string>(filterCloud.EventNames, "BullishCloud")];
            return bullEvents != null && bearEvents != null && filterEvents != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (filterEvents[index] && bullEvents[index])
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
