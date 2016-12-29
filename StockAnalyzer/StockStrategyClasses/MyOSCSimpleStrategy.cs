using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzer.StockStrategyClasses
{
   public class MyOSCSimpleStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells My-OSC crossing signals"; }
      }
      #endregion
      #region StockStrategy Methods

      public MyOSCSimpleStrategy()
      {
         this.TriggerIndicator = StockIndicatorManager.CreateIndicator("MYOSC(25)");
      }

      FloatSerie oscSerie = null;
      IStockTrailStop trailStop = null;
      public override void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
      {
         base.Initialise(stockSerie, lastBuyOrder, supportShortSelling);
         this.oscSerie = null;
         this.trailStop = StockTrailStopManager.CreateTrailStop("TRAILHL(3)");
         this.trailStop.ApplyTo(stockSerie);
      }

      bool trailActivated = false;

      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (this.TriggerIndicator == null) { return null; }

         if (this.oscSerie == null) this.oscSerie = (this.TriggerIndicator as IStockIndicator).Series[0];

         if (this.oscSerie[index - 1] < 0 && this.oscSerie[index] > 0)
         {
            trailActivated = false;
            return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
         }
         return null;
      }

      override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (this.TriggerIndicator == null) { return null; }

         trailActivated = trailActivated ? true : !float.IsNaN(trailStop.Series[0][index]);

         if (!trailActivated && this.oscSerie[index - 1] < this.oscSerie[index]) return null; // Don't sell as long as it's growing up.

         if (trailActivated && float.IsNaN(trailStop.Series[0][index])) // Stop has been triggered
         {
            return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
         }
         return null;
      }
      #endregion
   }
}
