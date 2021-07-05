using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMAOSCAgent : StockAgentBase
    {
        public EMAOSCAgent()
        {
            FastPeriod = 12;
            FastPeriod = 26;
        }

        [StockAgentParam(15, 25)]
        public int FastPeriod { get; set; }

        [StockAgentParam(40, 60)]
        public int SlowPeriod { get; set; }

        public override string Description => "Buy when OSC crosses above 0 and sell after lower close";
        public override string DisplayIndicator => $"INDICATOR|OSC({FastPeriod},{SlowPeriod},True,EMA)";

        FloatSerie closeSerie, lowSerie, oscSerie;
        float stop = float.NaN;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Math.Max(SlowPeriod, FastPeriod))
                return false;
            oscSerie = stockSerie.GetIndicator($"OSC({FastPeriod},{SlowPeriod},True,EMA)").Series[0];
            closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            return oscSerie != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (oscSerie[index] > 0)
            {
                stop = float.NaN;
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (float.IsNaN(stop))
            {
                if (oscSerie[index] < 0)
                {
                    stop = lowSerie[index];
                }
            }
            else
            {
                if (oscSerie[index] > 0)
                {
                    stop = float.NaN;
                }
                else if (closeSerie[index] < stop)
                {
                    return TradeAction.Sell;
                }
            }
            return TradeAction.Nothing;
        }
    }
}
