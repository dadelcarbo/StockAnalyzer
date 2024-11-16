using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;
using System.Linq;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMAMonthlyAgent : StockAgentBase
    {
        public EMAMonthlyAgent()
        {
            Period = 13;
        }

        [StockAgentParam(10, 40, 5)]
        public int Period { get; set; }

        public override string Description => "Buy when CLOSE is above EMA, do the check only on the first day of the month";

        public override string DisplayIndicator => $"INDICATOR|EMA({Period})";

        FloatSerie bodyLowSerie;
        FloatSerie emaSerie;
        DateTime[] dates;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            dates = stockSerie.Keys.ToArray();
            emaSerie = stockSerie.GetIndicator($"EMA({Period})").Series[0];
            bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (dates[index - 1].Month != dates[index].Month && bodyLowSerie[index] > emaSerie[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (dates[index - 1].Month != dates[index].Month && closeSerie[index] < emaSerie[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
