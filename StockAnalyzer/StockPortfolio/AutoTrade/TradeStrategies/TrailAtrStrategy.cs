using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class TrailAtrStrategy : ITradeStrategy
    {
        public string Name => "TrailAtr";

        public string Description => "Buy Sell based on ATR Band Strategy";

        static int period = 12;

        public TradeRequest TryToOpenPosition(DataSerie dataSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var trailStop = dataSerie.GetTrailStop($"TRAILATR({period},6,0.75,-0.25,EMA)");

            var brokenUpEvents = trailStop.GetEvents("BrokenUp");


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

            if (brokenUpEvents[index])
            {
                return new TradeRequest { BuySell = BuySell.Buy, DataSerie = dataSerie, Value = lastBar.CLOSE, Stop = lastBar.LOW };
            }
            return null;
        }

        public TradeRequest TryToClosePosition(DataSerie dataSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var emaSerie = dataSerie.GetIndicator($"EMA({12})").Series[0];

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

            if (lastBar.CLOSE < emaSerie[index])
            {
                return new TradeRequest { BuySell = BuySell.Sell, DataSerie = dataSerie, Value = lastBar.CLOSE };
            }

            return null;
        }
    }
}
