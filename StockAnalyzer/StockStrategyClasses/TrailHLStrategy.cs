using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockStrategyClasses
{
    public class TrailHLStrategy : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells using Trail High low indicator"; }
        }
        #endregion
        #region StockStrategy Methods

        private IStockTrailStop trailStop;
        private IStockIndicator movingAverage;
        public TrailHLStrategy()
        {
            this.TriggerIndicator = StockTrailStopManager.CreateTrailStop("TRAILHL(3)");
            trailStop = (IStockTrailStop)this.TriggerIndicator;
            movingAverage = (IStockIndicator)StockIndicatorManager.CreateIndicator("HMA(100)");
        }

        override public void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
        {
            base.Initialise(stockSerie, lastBuyOrder, supportShortSelling);

            movingAverage.ApplyTo(stockSerie);
        }

        override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            if (this.trailStop == null)
            { return null; }

            #region Create Buy Order
            if (this.SupportShortSelling)
            {
                if (movingAverage.Series[0][index] > dailyValue.CLOSE) // Check if long term in down trend
                {
                    if (trailStop.Events[5][index])
                    {
                        return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
                    }
                }
            }
            if (movingAverage.Series[0][index] < dailyValue.CLOSE) // Check if long term in up trend
            {
                if (trailStop.Events[4][index])
                {
                    return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
                }
            }
            #endregion
            return null;
        }
        override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            #region Create Sell Order
            // Review buy limit according to indicators
            if (LastBuyOrder.IsShortOrder)
            {
                if (float.IsNaN(trailStop.Series[0][index - 1])&& !float.IsNaN(trailStop.Series[0][index]))
                {
                    return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, true);
                }
            }
            else
            {
                if (float.IsNaN(trailStop.Series[1][index - 1]) && !float.IsNaN(trailStop.Series[1][index]))
                {
                    return StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
                }
            }
            #endregion
            return null;
        }
        #endregion
    }
}
