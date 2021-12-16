using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TFLowAgent : StockAgentBase
    {
        public TFLowAgent()
        {
            Period = 13;
        }

        [StockAgentParam(2, 20)]
        public int Period { get; set; }

        public override string Description => "Buy with highest bar in 'Trigger' periods, and hold until TRENDBODY bear line not broken";

        public override string DisplayIndicator => $"CLOUD|TRENDBODY({Period})";

        FloatSerie bearLine;
        FloatSerie highest;
        FloatSerie close;

        protected override bool Init(StockSerie stockSerie)
        {
            bearLine = stockSerie.GetCloud($"TRENDBODY({Period})").Series[1];
            highest = stockSerie.GetIndicator($"HIGHEST({Period})").Series[0];
            close = stockSerie.GetSerie(StockDataType.CLOSE);
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (highest[index] > Period)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (close[index] < bearLine[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
