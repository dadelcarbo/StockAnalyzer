namespace TradeLearning.Model.Trading
{
    internal class BasicTradingStrategy : TradingStrategyBase
    {
        public override TradeAction Decide(int index, bool inPosition)
        {
            if (index < 1)
                return TradeAction.Nop;

            if (inPosition)
            {
                if (Data[index - 1] > Data[index])
                    return TradeAction.Sell;
            }
            else
            {
                if (Data[index - 1] < Data[index])
                    return TradeAction.Buy;
            }

            return TradeAction.Nop;
        }

        public override void Initialize(double[] priceSeries)
        {
            this.Data = priceSeries;
        }
    }
}
