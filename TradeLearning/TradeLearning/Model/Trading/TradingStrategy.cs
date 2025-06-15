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
        public string Name { get; }
        public double[] Data { get; }
        TradeAction Decide(int currentIndex, bool inPosition);

        public void Initialize(double[] priceSeries);
    }

    public abstract class TradingStrategyBase : ITradingStrategy
    {
        public string Name => this.GetType().Name.Replace("TradingStrategy", string.Empty);

        public double[] Data { get; set; }

        public abstract TradeAction Decide(int currentIndex, bool inPosition);

        public abstract void Initialize(double[] priceSeries);
    }
}
