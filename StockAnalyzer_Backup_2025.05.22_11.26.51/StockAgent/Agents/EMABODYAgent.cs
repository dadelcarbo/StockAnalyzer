using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMABODYAgent : StockAgentBase
    {
        public EMABODYAgent()
        {
            Period = 13;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }

        public override string Description => "Buy when BODY is above EMA and closes when CLOSE is below EMA";

        public override string DisplayIndicator => $"INDICATOR|EMA({Period})";

        FloatSerie bodyLowSerie;
        FloatSerie emaSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            emaSerie = stockSerie.GetIndicator($"EMA({Period})").Series[0];
            bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (bodyLowSerie[index] > emaSerie[index])
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
