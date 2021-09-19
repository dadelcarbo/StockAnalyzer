using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMAFWAgent : StockAgentBase
    {
        public EMAFWAgent()
        {
            Period = 13;
        }

        [StockAgentParam(10, 40)]
        public int Period { get; set; }

        public override string Description => "Buy when CLOSE above EMA and trigger a Financial Wisdom trail stop when close below EMA";

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
                trailValue = float.MinValue;
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        float trailValue = float.MinValue;
        protected override TradeAction TryToClosePosition(int index)
        {
            if (trailValue != float.MinValue)
            {
                if (closeSerie[index] < trailValue)
                {
                    trailValue = float.MinValue;
                    return TradeAction.Sell;
                }
            }

            if (closeSerie[index] < emaSerie[index])
            {
                trailValue = Math.Max(trailValue, lowSerie[index]);
            }
            return TradeAction.Nothing;
        }
    }
}
