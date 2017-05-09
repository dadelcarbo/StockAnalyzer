using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzer.StockStrategyClasses
{
   public class _DONCHIANStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells on Higher low and sells on next High detected"; }
      }
      #endregion
      #region StockStrategy Methods

      public _DONCHIANStrategy()
      {
         this.EntryTriggerIndicator = StockIndicatorManager.CreateIndicator("DONCHIAN(52)");
      }

      private FloatSerie middleUpBandSerie;
      private FloatSerie middleBandSerie;
      private FloatSerie middleDownBandSerie;

      private FloatSerie lowSerie;
      private FloatSerie highSerie;
      private FloatSerie closeSerie;

      public override void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
      {
         base.Initialise(stockSerie, lastBuyOrder, supportShortSelling);

         IStockIndicator indicator = EntryTriggerIndicator as IStockIndicator;
         middleUpBandSerie = indicator.Series[1];
         middleBandSerie = indicator.Series[2];
         middleDownBandSerie = indicator.Series[3];

         lowSerie = stockSerie.GetSerie(StockDataType.LOW);
         highSerie = stockSerie.GetSerie(StockDataType.HIGH);
         closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
      }


      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         #region Create Buy

         if (this.SupportShortSelling)
         {
            if (highSerie[index] < middleDownBandSerie[index])
            {
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                   dailyValue.DATE.AddDays(30), amount, true);
            }
         }
         // If higher Low Detected
         if (lowSerie[index] > middleUpBandSerie[index])
         {
            return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
         }

         #endregion
         return null;
      }

      public override StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         #region Create Sell Order

         if (LastBuyOrder.IsShortOrder)
         {
            // Sell in case of Support detected
            if (closeSerie[index] > middleBandSerie[index])
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                   dailyValue.DATE.AddDays(30), number, true);
            }
         }
         else
         {
            // Sell in case of Resistance detected
            if (closeSerie[index] < middleBandSerie[index])
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                   dailyValue.DATE.AddDays(30), number, false);
            }
         }

         #endregion
         return null;
      }
      #endregion
   }
}
