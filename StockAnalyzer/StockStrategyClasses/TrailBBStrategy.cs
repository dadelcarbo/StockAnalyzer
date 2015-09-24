using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;

namespace StockAnalyzer.StockStrategyClasses
{
   public class TrailBBStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells using trail bollinger band indicator"; }
      }
      #endregion
      #region StockStrategy Methods

      private IStockTrailStop trailStop;
      public TrailBBStrategy()
      {
         this.TriggerIndicator = StockTrailStopManager.CreateTrailStop("TRAILBB(11,2,-2)");
         trailStop = (IStockTrailStop)this.TriggerIndicator;
      }

      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (this.trailStop == null)
         { return null; }

         #region Create Buy Order
         if (this.SupportShortSelling)
         {
            if (float.IsNaN(trailStop.Series[1][index - 1]) && !float.IsNaN(trailStop.Series[1][index]))
            {
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
            }
         }
         if (float.IsNaN(trailStop.Series[0][index - 1]) && !float.IsNaN(trailStop.Series[0][index]))
         {
            return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
         }

         #endregion
         return null;
      }
      override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         #region Create Sell Order
         // Review buy limit according to indicators
         if (LastBuyOrder.IsShortOrder)
         {
            if (float.IsNaN(trailStop.Series[0][index - 1]) && !float.IsNaN(trailStop.Series[0][index]))
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, true);
            }
         }
         else
         {
            if (float.IsNaN(trailStop.Series[1][index - 1]) && !float.IsNaN(trailStop.Series[1][index]))
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
            }
         }
         #endregion
         return null;
      }
      #endregion
   }
}
