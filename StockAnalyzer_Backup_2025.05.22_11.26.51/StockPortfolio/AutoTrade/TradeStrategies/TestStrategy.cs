using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class TestStrategy : ITradeStrategy
    {
        public string Name => "Test";

        public string Description => "Buy Sell on EMA cross over";

        public TradeRequest TryToOpenPosition(StockSerie stockSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var emaLong = stockSerie.GetIndicator("EMA(75)").Series[0];
            var emashort = stockSerie.GetIndicator("EMA(5)").Series[0];

            StockDailyValue dailyValue;
            if (index == -1)
            {
                index = stockSerie.LastIndex;
                dailyValue = stockSerie.LastValue;
            }
            else
            {
                dailyValue = stockSerie.ValueArray[index];
            }

            if (emaLong[index - 1] < emashort[index - 1] && emaLong[index - 1] > emashort[index])
            {
                return new TradeRequest { BuySell = BuySell.Buy, StockSerie = stockSerie, Value = dailyValue.CLOSE, Stop = dailyValue.LOW };
            }
            return null;
        }

        public TradeRequest TryToClosePosition(StockSerie stockSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var emaLong = stockSerie.GetIndicator("EMA(75)").Series[0];
            var emashort = stockSerie.GetIndicator("EMA(5)").Series[0];

            StockDailyValue dailyValue;
            if (index == -1)
            {
                index = stockSerie.LastIndex;
                dailyValue = stockSerie.LastValue;
            }
            else
            {
                dailyValue = stockSerie.ValueArray[index];
            }

            if (emaLong[index - 1] > emashort[index - 1] && emaLong[index - 1] < emashort[index])
            {
                return new TradeRequest { BuySell = BuySell.Sell, StockSerie = stockSerie, Value = dailyValue.CLOSE };
            }

            return null;
        }
    }
}
