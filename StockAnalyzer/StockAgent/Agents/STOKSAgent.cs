using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class STOKSAgent : StockAgentBase
    {
        public STOKSAgent()
        {
            Period = 14;
            Smoothing = 3;
        }

        [StockAgentParam(10, 120)]
        public int Period { get; set; }

        [StockAgentParam(2, 20)]
        public int Smoothing { get; set; }

        public override string Description => "Buy when according to STOKS signals";

        FloatSerie STOKS;
        protected override void Init(StockSerie stockSerie)
        {
            STOKS = stockSerie.GetIndicator($"STOKS({Period}, {Smoothing}, {Smoothing})").Series[0];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (STOKS[index - 1] < 25 && STOKS[index] >= 25) // bar fast above slow STOKS
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (STOKS[index - 1] > 75 && STOKS[index] < 75) // bar fast above slow STOKS
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
