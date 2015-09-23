using System;
using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
    public class _TrailHLHigherLowStrategy : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells on Higher low and sells on next High detected"; }
        }
        #endregion
        #region StockStrategy Methods

        private IStockIndicator SRIndicator;

        private float previousLow = float.MaxValue;
        private float previousHigh = float.MaxValue;

        private int higherLowEventIndex;
        private int lowerHightEventIndex;

        private int resistanceEventIndex;
        private int supportEventIndex;

        public _TrailHLHigherLowStrategy()
        {
            this.TriggerIndicator = StockIndicatorManager.CreateIndicator("TRAILHLSR(3)");
            SRIndicator = (IStockIndicator)this.TriggerIndicator;

            higherLowEventIndex = SRIndicator.EventNames.ToList().IndexOf("HigherLow");
            lowerHightEventIndex = SRIndicator.EventNames.ToList().IndexOf("LowerHigh");

            supportEventIndex = SRIndicator.EventNames.ToList().IndexOf("SupportDetected");
            resistanceEventIndex = SRIndicator.EventNames.ToList().IndexOf("ResistanceDetected");

        }


        override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            if (this.SRIndicator == null)
            { return null; }

            #region Create Buy

            if (this.SupportShortSelling)
            {
                // If higher Low Detected
                if (this.SRIndicator.Events[lowerHightEventIndex][index])
                {
                    return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
                }
            }
            // If higher Low Detected
            if (this.SRIndicator.Events[higherLowEventIndex][index])
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
                if (this.SRIndicator.Events[supportEventIndex][index])
                {
                    return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                        dailyValue.DATE.AddDays(30), number, true);
                }
            }
            else
            {
                // Sell in case of Resistance detected
                if (this.SRIndicator.Events[resistanceEventIndex][index])
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
