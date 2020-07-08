using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class CloudMDHAgent : StockAgentBase
    {
        public CloudMDHAgent()
        {
            FastPeriod = 9;
            SlowPeriod = 26;
        }

        [StockAgentParam(2, 100)]
        public int FastPeriod { get; set; }

        [StockAgentParam(2, 100)]
        public int SlowPeriod { get; set; }

        public override string Description => "Buy when Open above bullish MDH cloud";

        IStockCloud cloud;
        BoolSerie bullishCloudEvent;
        BoolSerie closeAboveCloudEvent;
        BoolSerie bearEvents;
        protected override void Init(StockSerie stockSerie)
        {
            cloud = stockSerie.GetCloud($"MDH({FastPeriod},{SlowPeriod})");
            bullishCloudEvent = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "BullishCloud")];
            closeAboveCloudEvent = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloseAboveCloud")];
            bearEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloseBelowCloud")];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (closeAboveCloudEvent[index] && bullishCloudEvent[index])
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
