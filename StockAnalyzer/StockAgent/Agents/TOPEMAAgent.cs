using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TOPEMAAgent : StockAgentBase
    {
        public TOPEMAAgent()
        {
            Period = 13;
        }

        [StockAgentParam(10, 40)]
        public int Period { get; set; }

        public override string Description => "Buy when TOPEMA Resistance is broken and sell when TOPEMA Support broken";

        public override string DisplayIndicator => $"INDICATOR|TOPEMA({Period})";

        FloatSerie supportSerie;
        FloatSerie resistanceSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            supportSerie = stockSerie.GetIndicator($"TOPEMA({Period})").Series[0];
            resistanceSerie = stockSerie.GetIndicator($"TOPEMA({Period})").Series[1];

            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (closeSerie[index] > resistanceSerie[index - 1])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (closeSerie[index] < supportSerie[index - 1])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
