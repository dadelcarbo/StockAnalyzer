using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMAMMAgent : StockAgentMoneyManagedBase
    {
        public EMAMMAgent()
        {
            Period = 7;
        }

        [StockAgentParam(2, 200)]
        public int Period { get; set; }


        public override string Description => "Buy when Closes above EMA with Money management";
        public override string DisplayIndicator => $"INDICATOR|EMA({Period})";

        FloatSerie ema;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            ema = stockSerie.GetIndicator($"EMA({Period})").Series[0];
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (closeSerie[index] >= ema[index]) // bar fast above slow EMA
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (closeSerie[index] < ema[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
