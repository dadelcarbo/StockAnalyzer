using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class TrailAtrStrategy : ITradeStrategy
    {
        public string Name => "TrailAtr";

        public string Description => "Buy Sell based on ATR Band Strategy";

        static int period = 12;

        public TradeRequest TryToOpenPosition(StockSerie stockSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var trailStop = stockSerie.GetTrailStop($"TRAILATR({12},6,0.75,-0.25,EMA)");

            var brokenUpEvents = trailStop.GetEvents("BrokenUp");


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

            if (brokenUpEvents[index])
            {
                return new TradeRequest { BuySell = BuySell.Buy, StockSerie = stockSerie, Value = lastBar.CLOSE, Stop = lastBar.LOW };
            }
            return null;
        }

        public TradeRequest TryToClosePosition(StockSerie stockSerie, BarDuration duration, int index = -1)
        {
            if (index == 0) return null;

            var emaSerie = stockSerie.GetIndicator($"EMA({12})").Series[0];

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

            if (lastBar.CLOSE < emaSerie[index])
            {
                return new TradeRequest { BuySell = BuySell.Sell, StockSerie = stockSerie, Value = lastBar.CLOSE };
            }

            return null;
        }
    }
}
