using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Linq;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMABODYAgent : StockAgentBase
    {
        public EMABODYAgent()
        {
            Period = 13;
        }

        [StockAgentParam(10, 40)]
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
            bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => v.BodyLow).ToArray());

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
