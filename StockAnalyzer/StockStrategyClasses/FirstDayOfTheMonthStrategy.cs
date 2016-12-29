using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzer.StockStrategyClasses
{
   public class FirstDayOfTheMonthStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys at the close of the last day of the month and sells at the close of the first day of the month"; }
      }
      #endregion
      #region StockStrategy Methods

      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;
         if (index >= this.Serie.Count - 1)
         {
            return null;
         }

         StockDailyValue tomorrowValue = this.Serie.Values.ElementAt(index + 1);
         if (tomorrowValue.DATE.Month == dailyValue.DATE.Month)
         {
            return null;
         }

         return StockOrder.CreateBuyAtMarketCloseStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);

      }
      override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         return StockOrder.CreateSellAtMarketCloseStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
      }
      #endregion
   }
}
