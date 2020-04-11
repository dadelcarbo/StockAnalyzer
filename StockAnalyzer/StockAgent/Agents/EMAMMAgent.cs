using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class EMAMMAgent : StockAgentMoneyManagedBase
    {
        public EMAMMAgent(StockContext context)
            : base(context)
        {
            FastPeriod = 7;
            SlowPeriod = 50;
        }

        public int FastPeriod { get; set; }

        public int SlowPeriod { get; set; }

        public override string Description => "Buy when Open and close are above EMA";

        FloatSerie ema, emaFilter;
        public override void Initialize(StockSerie stockSerie)
        {
            base.Initialize(stockSerie);
            ema = context.Serie.GetIndicator($"EMA({FastPeriod})").Series[0];
            emaFilter = context.Serie.GetIndicator($"EMA({SlowPeriod})").Series[0];
        }

        protected override TradeAction TryToOpenPosition()
        {
            int i = context.CurrentIndex;

            if (ema[i] >= emaFilter[i]) // bar fast above slow EMA
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition()
        {
            int i = context.CurrentIndex;

            if (ema[i] < emaFilter[i]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
