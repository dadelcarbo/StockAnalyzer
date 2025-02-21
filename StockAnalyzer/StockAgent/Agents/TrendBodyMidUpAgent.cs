using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrendBodyMidUpAgent : StockAgentBase
    {
        public TrendBodyMidUpAgent()
        {
            Period = 13;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }

        public override string Description => "Buy with TRENDBODY Cross above MidUp line in bullish trend";

        public override string DisplayIndicator => $"CLOUD|TRENDBODY({Period})";

        IStockCloud cloudTrend;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        FloatSerie midUpSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            cloudTrend = stockSerie.GetCloud($"TRENDBODY({Period})");
            bullEvents = cloudTrend.Events[Array.IndexOf(cloudTrend.EventNames, "BullishCloud")];
            bearEvents = cloudTrend.Events[Array.IndexOf(cloudTrend.EventNames, "BearishCloud")];

            midUpSerie = (2.0f * cloudTrend.Series[0] + cloudTrend.Series[1]) / 3.0f;
            return bullEvents != null && bearEvents != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (bullEvents[index] && closeSerie[index - 1] < midUpSerie[index - 1] && closeSerie[index] > midUpSerie[index])
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
