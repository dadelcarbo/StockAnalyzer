using System;
using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
    public class _BBSTOCKStrategy : StockFilteredStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells on ADX Signals"; }
        }
        #endregion
        #region StockStrategy Methods
        
        public _BBSTOCKStrategy()
        {
            this.TriggerIndicator = StockIndicatorManager.CreateIndicator("STOKS(12,6,3)");

            this.FilterIndicator = StockTrailStopManager.CreateTrailStop("TRAILBB(50,2.5,-2.5)");

            this.BuyTriggerEventName =   "BullishCrossing";
            this.ShortTriggerEventName = "BearishCrossing";

            this.OkToBuyFilterEventName =    "UpTrend";
            this.OkToShortFilterEventName = "DownTrend";
        }
     
        #endregion
    }
}
