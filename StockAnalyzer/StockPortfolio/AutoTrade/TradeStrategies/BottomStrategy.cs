using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
using System;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class BottomStrategy : ITradeStrategy
    {
        public string Name => "Bottom";
        public string Description => "Buy when a bar closes higher than the previous body high and sells when closing below previous body low.";

        float R2 = 0;

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

            StockDailyValue previousBar = dataSerie.Values[index - 1];

            if (lastBar.CLOSE > previousBar.BodyHigh)
            {
                var stop = Math.Min(lastBar.LOW, previousBar.LOW);
                R2 = lastBar.CLOSE + 2f * (lastBar.CLOSE - stop);
                return new TradeRequest { BuySell = BuySell.Buy, DataSerie = dataSerie, Value = lastBar.CLOSE, Stop = stop };
            }
            return null;
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

            StockDailyValue previousBar = dataSerie.Values[index - 1];


            if (lastBar.CLOSE < previousBar.BodyLow)
            {
                return new TradeRequest { BuySell = BuySell.Sell, DataSerie = dataSerie, Value = lastBar.CLOSE };
            }

            return null;
        }
    }
}
