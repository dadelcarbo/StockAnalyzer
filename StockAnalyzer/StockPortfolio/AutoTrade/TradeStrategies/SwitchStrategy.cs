using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class SwitchStrategy : ITradeStrategy
    {
        public string Name => "Switch";
        public string Description => "Systematicaly buy or sell, just for testing";

        public TradeRequest TryToOpenPosition(StockSerie stockSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            StockDailyValue lastBar;
            if (index == -1)
            {
                index = stockSerie.LastIndex;
                lastBar = stockSerie.LastValue;
            }
            else
            {
                lastBar = stockSerie.ValueArray[index];
            }

            return new TradeRequest { BuySell = BuySell.Buy, StockSerie = stockSerie, Value = lastBar.CLOSE };
        }

        public TradeRequest TryToClosePosition(StockSerie stockSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            StockDailyValue lastBar;
            if (index == -1)
            {
                index = stockSerie.LastIndex;
                lastBar = stockSerie.LastValue;
            }
            else
            {
                lastBar = stockSerie.ValueArray[index];
            }

            return new TradeRequest { BuySell = BuySell.Sell, StockSerie = stockSerie, Value = lastBar.CLOSE };
        }
    }
}