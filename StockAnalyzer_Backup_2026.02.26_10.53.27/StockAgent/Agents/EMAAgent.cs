using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMAAgent : StockAgentBase
    {
        public EMAAgent()
        {
            Period = 20;
        }

        [StockAgentParam(5, 200, 5)]
        public int Period { get; set; }

        public override string Description => "Buy when closing above EMA and sell when closing is below EMA";

        public override string DisplayIndicator => $"INDICATOR|EMA({Period})";

        FloatSerie emaSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            emaSerie = stockSerie.GetIndicator($"EMA({Period})").Series[0];

            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (closeSerie[index] > emaSerie[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (closeSerie[index] < emaSerie[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
