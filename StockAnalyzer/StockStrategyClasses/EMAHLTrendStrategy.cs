using System;
using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
    public class _EMAHLTrendStrategy : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells on Higher low happening above a EMA"; }
        }
        #endregion
        #region StockStrategy Methods

        private IStockEvent emaHLTrendPaintBar;

        private readonly int upTrendEventIndex;
        private readonly int downTrendEventIndex;

        public _EMAHLTrendStrategy()
        {
            this.TriggerIndicator = StockPaintBarManager.CreatePaintBar("EMAHLTrend(200,3)");
            emaHLTrendPaintBar = (IStockEvent)this.TriggerIndicator;

            upTrendEventIndex = emaHLTrendPaintBar.EventNames.ToList().IndexOf("UpTrend");
            downTrendEventIndex = emaHLTrendPaintBar.EventNames.ToList().IndexOf("DownTrend");
        }


        override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            if (this.emaHLTrendPaintBar == null)
            { return null; }

            #region Create Buy

            if (this.SupportShortSelling)
            {
                // If higher Low Detected
                if (this.emaHLTrendPaintBar.Events[downTrendEventIndex][index] && !this.emaHLTrendPaintBar.Events[downTrendEventIndex][index - 1])
                {
                    return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
                }
            }
            // If higher Low Detected
            if (this.emaHLTrendPaintBar.Events[upTrendEventIndex][index] && !this.emaHLTrendPaintBar.Events[upTrendEventIndex][index - 1])
            {
                return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
            }

            #endregion
            return null;
        }

        public override StockOrder TryToSell(StockDailyValue dailyValue, int index, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            #region Create Sell Order

            if (LastBuyOrder.IsShortOrder)
            {
                // Sell in case of Support detected
                if (!this.emaHLTrendPaintBar.Events[downTrendEventIndex][index])
                {
                    return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                        dailyValue.DATE.AddDays(30), this.LastBuyOrder.Number, true);
                }
            }
            else
            {
                // Sell in case of Resistance detected
                if (!this.emaHLTrendPaintBar.Events[upTrendEventIndex][index])
                {
                    return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE,
                        dailyValue.DATE.AddDays(30), this.LastBuyOrder.Number, false);
                }
            }

            #endregion
            return null;
        }
        #endregion
    }
}
