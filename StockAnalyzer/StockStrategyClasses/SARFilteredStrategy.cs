using System.Linq;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
   public class SARFilteredStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells using trailing orders, there is no real strategy, just create a buy or sell order according to the parameters"; }
      }
      #endregion
      #region StockStrategy Methods

      IStockIndicator SAR;
      private int sarBrokenUpEventIndex;
      private int sarBrokenDownEventIndex;

      public SARFilteredStrategy()
      {
         this.TriggerIndicator = StockIndicatorManager.CreateIndicator("SAR(.02,.2)");
         SAR = (IStockIndicator)this.TriggerIndicator;

         sarBrokenUpEventIndex = SAR.EventNames.ToList().IndexOf("BrokenUp");
         sarBrokenDownEventIndex = SAR.EventNames.ToList().IndexOf("BrokenDown");
      }

      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (this.TriggerIndicator == null) { return null; }

         if (this.SupportShortSelling)
         {
            if (SAR.Events[sarBrokenDownEventIndex][index])
            {
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
            }
         }
         if (SAR.Events[sarBrokenUpEventIndex][index])
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
            if (SAR.Events[sarBrokenUpEventIndex][index])
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, true);
            }
         }
         else
         {
            if (SAR.Events[sarBrokenDownEventIndex][index])
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
            }
         }
         return null;
      }
      #endregion
   }
}
