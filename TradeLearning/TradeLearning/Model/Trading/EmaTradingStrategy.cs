namespace TradeLearning.Model.Trading
{
    internal class EmaTradingStrategy : TradingStrategyBase
    {
        public int EmaPeriod { get; set; }

        public double[] ema;

        public override TradeAction Decide(int index, bool inPosition)
        {
            if (index < 1)
                return TradeAction.Nop;

            if (inPosition)
            {
                if (ema[index - 1] > ema[index])
                    return TradeAction.Sell;
            }
            else
            {
                if (ema[index - 1] < ema[index])
                    return TradeAction.Buy;
            }

            return TradeAction.Nop;
        }

        public override void Initialize(double[] priceSeries)
        {
            this.Data = priceSeries;
            this.ema = priceSeries.CalculateEMA(EmaPeriod);
        }
    }
}
