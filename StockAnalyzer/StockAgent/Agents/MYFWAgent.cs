using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class MYFWAgent : StockAgentBase
    {
        public MYFWAgent()
        {
            Period = 12;
        }

        [StockAgentParam(5, 50, 1)]
        public int Period { get; set; }

        public override string Description => "Implements My FW blue print strategy";

        public override string DisplayIndicator => $"INDICATOR|EMA({Period})";

        //FloatSerie histogramSerie;
        //FloatSerie emacdSerie;
        //FloatSerie signalSerie;
        FloatSerie natr;
        FloatSerie volumeEMA;
        FloatSerie highest;
        FloatSerie variation;
        FloatSerie ror;
        FloatSerie ema;

        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            natr = stockSerie.GetIndicator($"NATR(14)").Series[0];
            highest = stockSerie.GetIndicator($"HIGHEST(20)").Series[0];

            volumeEMA = volumeSerie.CalculateEMA(20);

            variation = stockSerie.GetSerie(StockDataType.VARIATION);

            ema = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(Period);
            ror = stockSerie.GetIndicator($"ROR(50)").Series[0];

            return true;
        }

        bool waitSell = false;
        float stop;
        protected override TradeAction TryToOpenPosition(int index)
        {
            bool c1 = highest[index] > 20f;
            bool c2 = ror[index] > 0.3f;
            bool c3 = variation[index] > 0.03f && variation[index] < 0.2f;
            bool c4 = (closeSerie[index] - lowSerie[index]) / (highSerie[index] - lowSerie[index]) > 0.75f;

            if (c1 && c2 && c3 && c4)
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
                if (closeSerie[index] > ema[index])
                {
                    waitSell = false;
                }
            }
            else if (closeSerie[index] < ema[index])
            {
                waitSell = true;
                stop = lowSerie[index];
            }
            return TradeAction.Nothing;
        }
    }
}
