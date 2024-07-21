using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class BottomStrategy : ITradeStrategy
    {
        public string Name => "Bottom";
        public string Description => "Buy when a bar close higher than the previous one and set a stop a the low of the two bars. Sell on close above R2";

        float R2 = 0;

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

            StockDailyValue previousBar = stockSerie.ValueArray[index - 1];

            if (lastBar.CLOSE > previousBar.CLOSE)
            {
                var stop = Math.Min(lastBar.LOW, previousBar.LOW);
                R2 = lastBar.CLOSE + 2f * (lastBar.CLOSE - stop);
                return new TradeRequest { BuySell = BuySell.Sell, StockSerie = stockSerie, Value = lastBar.CLOSE, Stop = stop };
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
                return new TradeRequest { BuySell = BuySell.Buy, StockSerie = stockSerie, Value = dailyValue.CLOSE };
            }

            return null;
        }
    }
}
