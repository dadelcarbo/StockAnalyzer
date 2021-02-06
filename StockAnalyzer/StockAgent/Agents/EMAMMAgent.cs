using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMAMMAgent : StockAgentMoneyManagedBase
    {
        public EMAMMAgent()
        {
            FastPeriod = 7;
            SlowPeriod = 50;
        }

        public int FastPeriod { get; set; }

        public int SlowPeriod { get; set; }

        public override string Description => "Buy when Open and close are above EMA";

        FloatSerie ema, emaFilter;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Math.Max(SlowPeriod, FastPeriod))
                return false;
            ema = stockSerie.GetIndicator($"EMA({FastPeriod})").Series[0];
            emaFilter = stockSerie.GetIndicator($"EMA({SlowPeriod})").Series[0];
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (ema[index] >= emaFilter[index]) // bar fast above slow EMA
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (ema[index] < emaFilter[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
