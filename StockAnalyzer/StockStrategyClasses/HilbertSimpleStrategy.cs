using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
   public class HilbertSimpleStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells Hilbert support/resistance signals"; }
      }
      #endregion
      #region StockStrategy Methods

      IStockIndicator hilbertSR;
      public HilbertSimpleStrategy()
      {
         this.TriggerIndicator = StockIndicatorManager.CreateIndicator("HILBERTSR(1,8)");
         hilbertSR = (IStockIndicator)this.TriggerIndicator;
      }
      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (this.TriggerIndicator == null) { return null; }

         if (this.SupportShortSelling)
         {
            if (float.IsNaN(hilbertSR.Series[1][index - 1]) && !float.IsNaN(hilbertSR.Series[1][index]))
            {
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
            }
         }
         if (float.IsNaN(hilbertSR.Series[0][index - 1]) && !float.IsNaN(hilbertSR.Series[0][index]))
         {
            return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
         }
         return null;
      }
      override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (this.TriggerIndicator == null) { return null; }

         if (LastBuyOrder.IsShortOrder)
         {
            if (float.IsNaN(hilbertSR.Series[0][index - 1]) && !float.IsNaN(hilbertSR.Series[0][index]))
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, true);
            }
         }
         else
         {
            if (float.IsNaN(hilbertSR.Series[1][index - 1]) && !float.IsNaN(hilbertSR.Series[1][index]))
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
            }
         }
         return null;
      }
      #endregion
   }
}
