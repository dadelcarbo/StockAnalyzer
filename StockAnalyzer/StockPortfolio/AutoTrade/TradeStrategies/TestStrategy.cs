using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class TestStrategy : ITradeStrategy
    {
        public string Name => "Test";

        public string Description => "Buy Sell on EMA cross over";

        public TradeRequest TryToOpenPosition(DataSerie dataSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var emaLong = dataSerie.GetIndicator("EMA(75)").Series[0];
            var emashort = dataSerie.GetIndicator("EMA(5)").Series[0];

            StockDailyValue dailyValue;
            if (index == -1)
            {
                index = dataSerie.LastIndex;
                dailyValue = dataSerie.LastValue;
            }
            else
            {
                dailyValue = dataSerie.Values[index];
            }

            if (emaLong[index - 1] < emashort[index - 1] && emaLong[index - 1] > emashort[index])
            {
                return new TradeRequest { BuySell = BuySell.Buy, DataSerie = dataSerie, Value = dailyValue.CLOSE, Stop = dailyValue.LOW };
            }
            return null;
        }

        public TradeRequest TryToClosePosition(DataSerie dataSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var emaLong = dataSerie.GetIndicator("EMA(75)").Series[0];
            var emashort = dataSerie.GetIndicator("EMA(5)").Series[0];

            StockDailyValue dailyValue;
            if (index == -1)
            {
                index = dataSerie.LastIndex;
                dailyValue = dataSerie.LastValue;
            }
            else
            {
                dailyValue = dataSerie.Values[index];
            }

            if (emaLong[index - 1] > emashort[index - 1] && emaLong[index - 1] < emashort[index])
            {
                return new TradeRequest { BuySell = BuySell.Sell, DataSerie = dataSerie , Value = dailyValue.CLOSE };
            }

            return null;
        }
    }
}
