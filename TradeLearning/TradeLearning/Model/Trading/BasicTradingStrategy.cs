namespace TradeLearning.Model.Trading
{
    internal class BasicTradingStrategy : ITradingStrategy
    {
        public double[] Data { get; protected set; }

        public TradeAction Decide(int index, bool inPosition)
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

        public void Initialize(double[] priceSeries)
        {
            this.Data = priceSeries;
        }
    }
}
