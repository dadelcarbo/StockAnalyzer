using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class STOKSTRENDAgent : StockAgentBase
    {
        public STOKSTRENDAgent()
        {
            Period = 14;
            Smoothing = 3;
        }

        [StockAgentParam(10, 120)]
        public int Period { get; set; }

        [StockAgentParam(2, 20)]
        public int Smoothing { get; set; }

        public override string Description => "Buy when according to STOKSTREND signals";

        FloatSerie STOKSTREND;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            STOKSTREND = stockSerie.GetIndicator($"STOKSTREND({Period}, {Smoothing}, {Smoothing})").Series[0];
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (STOKSTREND[index] >= 0) // bar fast above slow STOKSTREND
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (STOKSTREND[index] < 0) // bar fast below slow STOKSTREND
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
