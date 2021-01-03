using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class CloudMDH2Agent : StockAgentBase
    {
        public CloudMDH2Agent()
        {
            FastPeriod = 9;
            SlowPeriod = 26;
        }

        [StockAgentParam(2, 100)]
        public int FastPeriod { get; set; }

        [StockAgentParam(2, 100)]
        public int SlowPeriod { get; set; }

        public override string Description => "Buy when full bar above bullish MDH cloud";

        IStockCloud cloud;
        BoolSerie bullishCloudEvent;
        BoolSerie bearEvents;
        FloatSerie lowSerie;
        FloatSerie bullSerie;
        protected override void Init(StockSerie stockSerie)
        {
            lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            cloud = stockSerie.GetCloud($"MDH({FastPeriod},{SlowPeriod})");
            bullSerie = cloud.Series[0];
            bullishCloudEvent = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "BullishCloud")];
            bearEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloseBelowCloud")];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (lowSerie[index] > bullSerie[index] && bullishCloudEvent[index])
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
