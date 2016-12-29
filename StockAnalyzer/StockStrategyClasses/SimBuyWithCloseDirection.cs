using System.Linq;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockStrategyClasses
{
    public class SimBuyWithCloseDirection : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells using market open, there is no real strategy, just create a buy or sell order according to the daily close compared to previous day"; }
        }
        #endregion
        #region StockStrategy Methods

        override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            StockDailyValue previousValue = this.Serie.Values.ElementAt(index - 1);
            benchmark = dailyValue.CLOSE;

            #region Create Buy Order
            StockOrder stockOrder = null;
            if (dailyValue.CLOSE > previousValue.CLOSE)
            {
                stockOrder = StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
                LastBuyOrder = stockOrder;
            }
            #endregion
            return stockOrder;
        }

        override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
        {
            StockDailyValue previousValue = this.Serie.Values.ElementAt(index - 1);
            benchmark = dailyValue.CLOSE;

            #region Create Sell Order
            StockOrder stockOrder = null;

            if (dailyValue.CLOSE < previousValue.CLOSE)
            {
                stockOrder = StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
            }
            #endregion
            return stockOrder;
        }
        #endregion
    }
}
