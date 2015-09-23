using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;

namespace StockAnalyzer.StockStrategyClasses
{
    public class HighLowBuySellRateStrategy : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells using trailing orders, there is no real strategy, just create a buy or sell order according to the parameters"; }
        }
        #endregion
        #region StockStrategy Methods

        override public StockOrder TryToBuy(int index, float amount, ref float benchmark)
        {
            StockDailyValue dailyValue = this.Serie.Values.ElementAt(index);
            benchmark = dailyValue.CLOSE;

            #region Create Buy Order
            StockOrder stockOrder = null;
            float buySellRate = this.Serie.GetSerie(StockIndicatorType.BUY_SELL_RATE).Values[index];
            if (buySellRate < 0.0f && this.SupportShortSelling)
            {
                stockOrder = StockOrder.CreateBuyAtLimitStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), amount, dailyValue.LOW, dailyValue, true);
            }
            else
            {
                if (buySellRate > 0.0f)
                {
                    stockOrder = StockOrder.CreateBuyAtThresholdStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), amount, dailyValue.HIGH, dailyValue, false);
                }
            }
            #endregion
            return stockOrder;
        }
        override public StockOrder TryToSell(int index, ref float benchmark)
        {
            StockDailyValue dailyValue = this.Serie.Values.ElementAt(index);
            benchmark = dailyValue.CLOSE;

            #region Create Sell Order
            StockOrder stockOrder = null;
            // Review sell limit according to indicators
            float buySellRate = this.Serie.GetSerie(StockIndicatorType.BUY_SELL_RATE).Values[index];
            if (LastBuyOrder.IsShortOrder)
            {
                if (buySellRate > 0.0f)
                {
                    stockOrder = StockOrder.CreateSellAtLimitStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), this.LastBuyOrder.Number, dailyValue.HIGH, dailyValue, true);
                }
            }
            else
            {
                if (buySellRate < 0.0f)
                {
                    stockOrder = StockOrder.CreateSellAtThresholdStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), this.LastBuyOrder.Number, dailyValue.LOW, dailyValue, false);
                }
            }
            #endregion
            return stockOrder;
        }
        #endregion
    }
}
