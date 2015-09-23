using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;

namespace StockAnalyzer.StockStrategyClasses
{
    public class HighLowBuySellRateReversePositionStrategy: StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells using trailing orders, there is no real strategy, just create a buy or sell order according to the parameters"; }
        }
        #endregion
        #region StockStrategy Methods

        StockOrder lastSellOrder = null;
        override public StockOrder TryToBuy(int index, float amount, ref float benchmark)
        {
            StockDailyValue dailyValue = this.Serie.Values.ElementAt(index);
            benchmark = dailyValue.CLOSE;

            #region Create Buy Order
            StockOrder stockOrder = null;
            float buySellRate = this.Serie.GetSerie(StockIndicatorType.BUY_SELL_RATE).Values[index];
            if (!this.SupportShortSelling || lastSellOrder == null)
            {
                if (buySellRate > 0.0f)
                {
                    return StockOrder.CreateBuyAtThresholdStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), amount, dailyValue.HIGH, dailyValue, false);
                }
                else
                {
                    return null;
                }
            }
            if (lastSellOrder.IsShortOrder)
            {
               stockOrder = StockOrder.CreateBuyAtThresholdStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), amount, dailyValue.HIGH, dailyValue, false);
            }
            else
            {
                stockOrder = StockOrder.CreateBuyAtLimitStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), amount, dailyValue.LOW, dailyValue, true);
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

            // Check if we are in profit
            bool inProfit = false;
            float limit;
            if (LastBuyOrder.IsShortOrder)
            {
                if (LastBuyOrder.UnitCost > dailyValue.CLOSE)
                {
                    inProfit = true;
                    if (this.Serie.GetSerie(StockDataType.EMA3)[index] > dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.EMA3)[index];
                    }
                    else if (this.Serie.GetSerie(StockDataType.EMA6)[index] > dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.EMA6)[index];
                    }
                    else if (this.Serie.GetSerie(StockDataType.EMA12)[index] > dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.EMA12)[index];
                    }
                    else if (this.Serie.GetSerie(StockDataType.SAREX_FOLLOWER)[index] > dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.SAREX_FOLLOWER)[index];
                    }
                    else
                    {
                        limit = dailyValue.HIGH;
                    }
                }
                else
                {
                    limit = dailyValue.HIGH;
                }
            }
            else
            {
                if (LastBuyOrder.UnitCost < dailyValue.CLOSE)
                {
                    inProfit = true;
                    if (this.Serie.GetSerie(StockDataType.EMA3)[index] < dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.EMA3)[index];
                    }
                    else if (this.Serie.GetSerie(StockDataType.EMA6)[index] < dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.EMA6)[index];
                    }
                    else if (this.Serie.GetSerie(StockDataType.EMA12)[index] < dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.EMA12)[index];
                    }
                    else if (this.Serie.GetSerie(StockDataType.SAREX_FOLLOWER)[index] < dailyValue.CLOSE)
                    {
                        limit = this.Serie.GetSerie(StockDataType.SAREX_FOLLOWER)[index];
                    }
                    else
                    {
                        limit = dailyValue.LOW;
                    }
                }
                else
                {
                    limit = dailyValue.LOW;
                }
            }

            // Review sell limit according to indicators
            float buySellRate = this.Serie.GetSerie(StockIndicatorType.BUY_SELL_RATE).Values[index];
            if (LastBuyOrder.IsShortOrder)
            {
                if (inProfit || buySellRate > 0.0f)
                {
                    stockOrder = StockOrder.CreateSellAtLimitStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), this.LastBuyOrder.Number, limit, dailyValue, true);
                }
            }
            else
            {
                if (inProfit || buySellRate < 0.0f)
                {
                    stockOrder = StockOrder.CreateSellAtThresholdStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), this.LastBuyOrder.Number, limit, dailyValue, false);
                }
            }
            #endregion
            lastSellOrder = stockOrder;
            return stockOrder;
        }
        #endregion
    }
}
