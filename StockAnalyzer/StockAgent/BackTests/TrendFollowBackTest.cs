using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.BackTests
{
    public class TrendFollowBackTest : BackTestBase
    {
        public TrendFollowBackTest()
        {
            Period = 13;
        }

        [StockAgentParam(15, 175, 5)]
        public int Period { get; set; }

        public override string Description => "Test implementation, no valid back test";
        public override string DisplayIndicator => $"INDICATOR|TOPEMA(75,6,3)";

        FloatSerie filter;

        FloatSerie stopSerie;
        IStockEvent stockEvents;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init()
        {
            if (DataSerie.Count < Period)
                return false;
            filter = DataSerie.GetIndicator($"ROR({Period})").Series[0];
            stopSerie = DataSerie.GetIndicator($"EMA({Period})").Series[0];
            stockEvents = DataSerie.GetIndicator($"TOPEMA({Period},6,3)");
            bullEvents = stockEvents.Events[Array.IndexOf(stockEvents.EventNames, "FirstResistanceBroken")];
            bearEvents = stockEvents.Events[Array.IndexOf(stockEvents.EventNames, "SupportBroken")];
            return bullEvents != null && bearEvents != null;
        }

        public override float GetStop(int index)
        {
            return stopSerie[index];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (bullEvents[index] & filter[index] < 0.3f)
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
