using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class SwitchStrategy : ITradeStrategy
    {
        public string Name => "Switch";
        public string Description => "Systematicaly buy or sell, just for testing";

        public TradeRequest TryToOpenPosition(DataSerie dataSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            StockDailyValue lastBar;
            if (index == -1)
            {
                index = dataSerie.LastIndex;
                lastBar = dataSerie.LastValue;
            }
            else
            {
                lastBar = dataSerie.Values[index];
            }

            return new TradeRequest { BuySell = BuySell.Buy, DataSerie = dataSerie, Value = lastBar.CLOSE };
        }

        public TradeRequest TryToClosePosition(DataSerie dataSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            StockDailyValue lastBar;
            if (index == -1)
            {
                index = dataSerie.LastIndex;
                lastBar = dataSerie.LastValue;
            }
            else
            {
                lastBar = dataSerie.Values[index];
            }

            return new TradeRequest { BuySell = BuySell.Sell, DataSerie = dataSerie, Value = lastBar.CLOSE };
        }
    }
}