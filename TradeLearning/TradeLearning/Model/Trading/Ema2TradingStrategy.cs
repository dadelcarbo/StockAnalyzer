namespace TradeLearning.Model.Trading
{
    internal class Ema2TradingStrategy : TradingStrategyBase
    {
        public int EmaPeriod1 { get; set; }
        public int EmaPeriod2 { get; set; }

        public double[] ema1;
        public double[] ema2;

        public override TradeAction Decide(int index, bool inPosition)
        {
            if (index < 1)
                return TradeAction.Nop;

            if (inPosition)
            {
                if (ema1[index] < ema2[index])
                    return TradeAction.Sell;
            }
            else
            {
                if (ema1[index] > ema2[index])
                    return TradeAction.Buy;
            }

            return TradeAction.Nop;
        }

        public override void Initialize(double[] priceSeries)
        {
            this.Data = priceSeries;
            this.ema1 = priceSeries.CalculateEMA(EmaPeriod1);
            this.ema2 = priceSeries.CalculateEMA(EmaPeriod2);
        }
    }
}
