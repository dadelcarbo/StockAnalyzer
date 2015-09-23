using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
    public class StockSimpleStrategy : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells Stockasticks crossing signals"; }
        }
        #endregion
        #region StockStrategy Methods
                
        public StockSimpleStrategy()
        {
            this.TriggerIndicator = StockIndicatorManager.CreateIndicator("STOKSSR(14,3,3,75,25)");
        }


        BoolSerie supportDetected = null;
        BoolSerie resistanceDetected = null;
        override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            if (this.TriggerIndicator == null) { return null; }
            if (this.supportDetected == null)
            {
                this.supportDetected = (this.TriggerIndicator as IStockIndicator).Events[0];
                this.resistanceDetected = (this.TriggerIndicator as IStockIndicator).Events[1];
            }

            if (this.supportDetected[index])
            {
                return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
            }
            return null;
        }
        override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            if (this.TriggerIndicator == null) { return null; }

            //if (LastBuyOrder.IsShortOrder)
            //{
            //    if (float.IsNaN(hilbertSR.Series[0][index - 1]) && !float.IsNaN(hilbertSR.Series[0][index]))
            //    {
            //        return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, true);
            //    }
            //}
            //else
            //{
            if (this.resistanceDetected[index])
            {
                return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
            }
            //}
            return null;
        }
        #endregion
    }
}
