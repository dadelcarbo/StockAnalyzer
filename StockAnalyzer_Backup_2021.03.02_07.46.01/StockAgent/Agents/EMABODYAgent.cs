using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;
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

        public override string Description => "Buy when BODY is above EMA and closes when BODY is below EMA";

        public override string DisplayIndicator => $"CLOUD|EMA({Period})";

        FloatSerie bodyHighSerie;
        FloatSerie bodyLowSerie;
        FloatSerie emaSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            emaSerie = stockSerie.GetIndicator($"EMA({Period})").Series[0];
            bodyHighSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
            bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

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
            if (bodyLowSerie[index] < emaSerie[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
