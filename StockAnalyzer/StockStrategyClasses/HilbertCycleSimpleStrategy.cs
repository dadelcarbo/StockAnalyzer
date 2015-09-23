using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockStrategyClasses
{
    public class HilbertCycleSimpleStrategy : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells using trailing orders, just create a buy or sell order when a cyclical turn occurs"; }
        }
        #endregion
        #region StockStrategy Methods

        override public StockOrder TryToBuy(int index, float amount, ref float benchmark)
        {
            StockDailyValue dailyValue = this.Serie.Values.ElementAt(index);
            benchmark = dailyValue.CLOSE;

            #region Analyse indicators
            FloatSerie hilbertSMSerie = this.Serie.GetSerie(StockDataType.HILBERT_SR);
            bool trendChangedUp = hilbertSMSerie[index - 1] > 0 && hilbertSMSerie[index] < 0; 
            #endregion
            #region Create Buy Order
            StockOrder stockOrder = null;
            if (trendChangedUp)
            {
                // Review buy limit according to indicators
                stockOrder = StockOrder.CreateBuyAtMarketStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), amount, false);
            }
            #endregion
            return stockOrder;
        }
        override public StockOrder TryToSell(int index, ref float benchmark)
        {
            StockDailyValue dailyValue = this.Serie.Values.ElementAt(index);
            benchmark = dailyValue.CLOSE;

            #region Analyse indicators
            FloatSerie hilbertSMSerie = this.Serie.GetSerie(StockDataType.HILBERT_SR);
            bool trendChangedDown = hilbertSMSerie[index - 1] < 0 && hilbertSMSerie[index] > 0;
            #endregion
            #region Create Buy Order
            StockOrder stockOrder = null;
            if (trendChangedDown)
            {
                // Review buy limit according to indicators
                stockOrder = StockOrder.CreateSellAtMarketStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), this.LastBuyOrder.Number, false);
            }
            #endregion
            return stockOrder;
        }
        #endregion
    }
}
