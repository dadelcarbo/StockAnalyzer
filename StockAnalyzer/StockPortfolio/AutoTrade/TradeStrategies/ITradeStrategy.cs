using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public interface ITradeStrategy
    {
        string Name { get; }

        TradeRequest TryToClosePosition(StockSerie stockSerie, BarDuration duration, int index = -1);

        TradeRequest TryToOpenPosition(StockSerie stockSerie, BarDuration duration, int index = -1);
    }
}
