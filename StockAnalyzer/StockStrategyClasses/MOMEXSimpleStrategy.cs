using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;

namespace StockAnalyzer.StockStrategyClasses
{
   public class MOMEXSimpleStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells using Exhaustion Buying or selling"; }
      }
      #endregion
      #region StockStrategy Methods

      private string indicatorName = "BUYMOMEX(12,False,2)";

      private IStockDecorator momex;
      public MOMEXSimpleStrategy()
      {
         this.TriggerIndicator = StockDecoratorManager.CreateDecorator("DIV(2)", "BUYMOMEX(12,False,2)") as IStockEvent;
         momex = (IStockDecorator)this.TriggerIndicator;
      }

      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;
         if (!this.Serie.HasVolume)
         {
            return null;
         }

         IStockDecorator momex = this.Serie.GetDecorator("DIV(2)", indicatorName);

         if (momex.Events[1][index - 1] || momex.Events[3][index - 1])
         {
            return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
         }
         return null;
      }
      override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (!this.Serie.HasVolume)
         {
            return null;
         }

         IStockDecorator momex = this.Serie.GetDecorator("DIV(2)", indicatorName);

         if (momex.Events[0][index - 1] || momex.Events[2][index - 1])
         {
            return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
         }
         return null;
      }
      #endregion
   }
}
