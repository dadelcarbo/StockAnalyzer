using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockStrategyClasses
{
   public class HilbertAdvStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells Hilbert support/resistance signals"; }
      }
      #endregion
      #region StockStrategy Methods

      IStockIndicator hilbertSR;
      public HilbertAdvStrategy()
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

         // If we have a support
         if (!float.IsNaN(hilbertSR.Series[0][index]) && dailyValue.CLOSE > hilbertSR.Series[0][index])
         {
            if (float.IsNaN(hilbertSR.Series[0][index - 1]))
            {
               // This is the first bar after the support
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
            }

            FloatSerie CloseSerie = Serie.GetSerie(StockDataType.CLOSE);
            if (CloseSerie[index - 1] < hilbertSR.Series[0][index - 1] && CloseSerie[index] > hilbertSR.Series[0][index])
            {
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
            }
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
            // If we have a resistance
            if (!float.IsNaN(hilbertSR.Series[1][index]))
            {
               if (float.IsNaN(hilbertSR.Series[1][index - 1]))
               {
                  return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
               }
            }
            // If we have a support
            if (!float.IsNaN(hilbertSR.Series[0][index]))
            {
               FloatSerie CloseSerie = Serie.GetSerie(StockDataType.CLOSE);
               if (CloseSerie[index - 1] > hilbertSR.Series[0][index - 1] && CloseSerie[index] < hilbertSR.Series[0][index])
               {
                  return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
               }
            }
         }
         return null;
      }
      #endregion
   }
}
