using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
    public class SimBuyTrailingOrder : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells using trailing orders, there is no real strategy, just create a buy or sell order according to the parameters"; }
        }
        #endregion

         private IStockIndicator atr;
         public SimBuyTrailingOrder()
         {
             this.TriggerIndicator = StockIndicatorManager.CreateIndicator("ATR(5)");
             atr = (IStockIndicator)this.TriggerIndicator;
        }

        override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            float gapInPoints = atr.Series[0][index]*2;

            StockOrder stockOrder = StockOrder.CreateBuyTrailingStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), amount, dailyValue.CLOSE, gapInPoints, dailyValue);

            return stockOrder;
        }
        override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            float gapInPoints = atr.Series[0][index]*2;
            StockOrder stockOrder = StockOrder.CreateSellTrailingStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(5), number, dailyValue.CLOSE, gapInPoints, dailyValue);

            return stockOrder;
        }
    }
}
