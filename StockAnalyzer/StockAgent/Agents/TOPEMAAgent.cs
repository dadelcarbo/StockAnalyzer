using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;
using System.Linq;

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

        FloatSerie bodyLowSerie;
        FloatSerie emaSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            emaSerie = stockSerie.GetIndicator($"TOPEMA({Period})").Series[0];
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
            if (closeSerie[index] < emaSerie[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
