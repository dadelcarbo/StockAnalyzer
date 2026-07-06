using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public interface ITradeStrategy
    {
        string Name { get; }
        string Description { get; }

        TradeRequest TryToClosePosition(DataSerie dataSerie, BarDuration duration, int index = -1);

        TradeRequest TryToOpenPosition(DataSerie dataSerie, BarDuration duration, int index = -1);
    }
}
