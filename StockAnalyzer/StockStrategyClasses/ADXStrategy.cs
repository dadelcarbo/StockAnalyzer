using System.Linq;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;

namespace StockAnalyzer.StockStrategyClasses
{
   public class _ADXStrategy : StockStrategyBase
   {
      #region StockStrategy Properties
      override public string Description
      {
         get { return "This strategy buys and sells on ADX Signals"; }
      }
      #endregion
      #region StockStrategy Methods

      private IStockIndicator adxIndicator;
      private IStockDecorator adxDecorator;
      private IStockTrailStop SRTrailStop;

      private int upTrendIndex;
      private int downTrendIndex;


      private int exhaustionSellIndex;


      public int brokenDownEventIndex { get; set; }

      private int brokenUpEventIndex;

      private string triggerName = "ADX(30,25,6)";
      private string trailName = "TRAILHL(4)";

      public _ADXStrategy()
      {
         this.TriggerIndicator = StockIndicatorManager.CreateIndicator(triggerName);
         adxIndicator = (IStockIndicator)this.TriggerIndicator;

         this.adxDecorator = StockDecoratorManager.CreateDecorator("DIV(1)", triggerName);
         exhaustionSellIndex = adxDecorator.EventNames.ToList().IndexOf("ExhaustionBottom");

         this.SRTrailStop = StockTrailStopManager.CreateTrailStop(trailName);

         upTrendIndex = SRTrailStop.EventNames.ToList().IndexOf("UpTrend");
         downTrendIndex = SRTrailStop.EventNames.ToList().IndexOf("DownTrend");

         brokenUpEventIndex = SRTrailStop.EventNames.ToList().IndexOf("BrokenUp");
         brokenDownEventIndex = SRTrailStop.EventNames.ToList().IndexOf("BrokenDown");
      }

      public override void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
      {
         base.Initialise(stockSerie, lastBuyOrder, supportShortSelling);
         this.SRTrailStop = stockSerie.GetTrailStop(trailName);

         this.adxDecorator = stockSerie.GetDecorator("DIV(1)", ((IStockIndicator)TriggerIndicator).Name);
      }


      override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         benchmark = dailyValue.CLOSE;

         if (this.adxIndicator == null)
         { return null; }

         #region Create Buy

         if (this.adxDecorator.Events[this.exhaustionSellIndex][index - 1])
         {
            if (this.SupportShortSelling)
            {
               //if (this.adxIndicator.Events[downTrendIndex][index])
               if (this.SRTrailStop.Events[downTrendIndex][index])
               {
                  return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                     dailyValue.DATE.AddDays(60), amount, true);
               }
            }
            if (this.SRTrailStop.Events[upTrendIndex][index])
            {
               return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                  dailyValue.DATE.AddDays(60), amount, false);
            }
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
            if (this.SRTrailStop.Events[brokenUpEventIndex][index])
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
              dailyValue.DATE.AddDays(60), number, true);
            }
         }
         else
         {
            // Sell in case of Resistance detected
            if (this.SRTrailStop.Events[brokenDownEventIndex][index])
            {
               return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
              dailyValue.DATE.AddDays(60), number, false);
            }
         }

         #endregion
         return null;
      }
      #endregion
   }
}
