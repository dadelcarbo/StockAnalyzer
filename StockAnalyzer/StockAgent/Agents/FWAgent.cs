using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class FWAgent : StockAgentBase
    {
        public FWAgent()
        {
            FastPeriod = 12;
            SlowPeriod = 26;
            SignalPeriod = 9;
        }

        [StockAgentParam(12, 12)]
        public int FastPeriod { get; set; }

        [StockAgentParam(26, 26)]
        public int SlowPeriod { get; set; }

        //[StockAgentParam(6, 12)]
        public int SignalPeriod { get; set; }

        public override string Description => "Implements FW blue print strategy";

        public override string DisplayIndicator => $"INDICATOR|EMACD({SlowPeriod},{FastPeriod},{SignalPeriod})";

        FloatSerie histogramSerie;
        FloatSerie emacdSerie;
        FloatSerie signalSerie;
        FloatSerie natr;
        FloatSerie highest;
        FloatSerie volumeEMA;
        FloatSerie variation;

        protected override bool Init(StockSerie stockSerie)
        {
            if (SlowPeriod <= FastPeriod || stockSerie.Count < Math.Max(SlowPeriod, FastPeriod))
                return false;

            var emacd = stockSerie.GetIndicator($"EMACD({SlowPeriod},{FastPeriod},{SignalPeriod})");
            histogramSerie = emacd.Series[0];
            emacdSerie = emacd.Series[1];
            signalSerie = emacd.Series[2];

            natr = stockSerie.GetIndicator($"NATR(14)").Series[0];
            highest = stockSerie.GetIndicator($"HIGHEST(20)").Series[0];

            volumeEMA = volumeSerie.CalculateEMA(20);

            variation = stockSerie.GetSerie(StockDataType.VARIATION);

            return emacd != null;
        }

        bool waitSell = false;
        float stop;
        protected override TradeAction TryToOpenPosition(int index)
        {
            if (histogramSerie[index] > 0 && natr[index] < 8 && highest[index] > 20 && volumeSerie[index] > (volumeEMA[index] * 1.3f) && variation[index] > 0.05f && variation[index] < 0.20f)
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
                if (histogramSerie[index] > 0 && emacdSerie[index] > signalSerie[index])
                {
                    waitSell = false;
                }
            }
            else if (histogramSerie[index] < 0.0f)
            {
                waitSell = true;
                stop = lowSerie[index];
            }
            return TradeAction.Nothing;
        }
    }
}
