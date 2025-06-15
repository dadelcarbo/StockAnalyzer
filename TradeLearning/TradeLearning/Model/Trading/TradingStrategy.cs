namespace TradeLearning.Model.Trading
{
    public enum TradeAction
    {
        Nop,
        Buy,
        Sell
    }

    public interface ITradingStrategy
    {
        public double[] Data { get; }
        TradeAction Decide(int currentIndex, bool inPosition);

        public void Initialize(double[] priceSeries);
    }
}
