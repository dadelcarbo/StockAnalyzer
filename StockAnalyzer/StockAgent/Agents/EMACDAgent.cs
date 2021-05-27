using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMACDAgent : StockAgentBase
    {
        public EMACDAgent()
        {
            FastPeriod = 12;
            SlowPeriod = 26;
            SignalPeriod = 9;
        }

        [StockAgentParam(15, 25)]
        public int FastPeriod { get; set; }

        [StockAgentParam(40, 60)]
        public int SlowPeriod { get; set; }

        //[StockAgentParam(6, 12)]
        public int SignalPeriod { get; set; }

        public override string Description => "Buy when Open and close are above EMA";

        public override string DisplayIndicator => $"INDICATOR|EMACD({SlowPeriod},{FastPeriod},{SignalPeriod})";

        FloatSerie histogramSerie;
        FloatSerie emacdSerie;
        FloatSerie signalSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Math.Max(SlowPeriod, FastPeriod))
                return false;

            var emacd = stockSerie.GetIndicator($"EMACD({SlowPeriod},{FastPeriod},{SignalPeriod})");
            histogramSerie = emacd.Series[0];
            emacdSerie = emacd.Series[1];
            signalSerie = emacd.Series[2];
            return emacd != null;
        }

        bool waitSell = false;
        float stop;
        protected override TradeAction TryToOpenPosition(int index)
        {
            if (histogramSerie[index] > 0 && emacdSerie[index] > signalSerie[index])
            {
                waitSell = false;
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (waitSell)
            {
                if (closeSerie[index] < stop)
                {
                    waitSell = false;
                    return TradeAction.Sell;
                }
            }
            else if (emacdSerie[index] < signalSerie[index])
            {
                waitSell = true;
                stop = Math.Min(lowSerie[index], lowSerie[index - 1]);
            }
            return TradeAction.Nothing;
        }
    }
}
