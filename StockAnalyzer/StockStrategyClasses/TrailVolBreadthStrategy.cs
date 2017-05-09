using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockStrategyClasses
{
    public class TrailVolBreadthStrategy : StockStrategyBase
    {
        #region StockStrategy Properties
        override public string Description
        {
            get { return "This strategy buys and sells using the TB breadth stragegy, is application only to indexes supporting this indicator"; }
        }
        #endregion
        #region StockStrategy Methods

        public TrailVolBreadthStrategy()
        {
            this.EntryTriggerIndicator = StockIndicatorManager.CreateIndicator("OSC(3,6)");
        }
        FloatSerie OSCSerie;
        public override void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
        {
            this.Serie = stockSerie;
            this.LastBuyOrder = lastBuyOrder;
            this.SupportShortSelling = supportShortSelling;
            if (StockDictionary.StockDictionarySingleton.ContainsKey("TB." + stockSerie.StockName))
            {
                StockSerie breadthSerie = StockDictionary.StockDictionarySingleton["TB." + stockSerie.StockName];
                if (breadthSerie.Initialise())
                {
                    IStockIndicator OSCIndicator = breadthSerie.GetIndicator(((IStockIndicator)EntryTriggerIndicator).Name);
                    if (OSCIndicator != null)
                    {
                        OSCSerie = OSCIndicator.Series[0];
                    }
                }
            }
        }

        override public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            if (this.OSCSerie == null)
            { return null; }

            #region Create Buy Order
            if (this.SupportShortSelling)
            {
                if (OSCSerie[index - 1] > 0 && OSCSerie[index] <= 0)
                {
                    return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, true);
                }
            }
            if (OSCSerie[index - 1] <= 0 && OSCSerie[index] > 0)
            {
                return StockOrder.CreateBuyAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), amount, false);
            }
            return null;
            #endregion
        }
        override public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
        {
            benchmark = dailyValue.CLOSE;

            #region Create Sell Order
            StockOrder stockOrder = null;
            // Review buy limit according to indicators
            if (LastBuyOrder.IsShortOrder)
            {
                if (OSCSerie[index - 1] <= 0 && OSCSerie[index] > 0)
                {
                    stockOrder = StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, true);
                }
            }
            else
            {
                if (OSCSerie[index - 1] > 0 && OSCSerie[index] <= 0)
                {
                    stockOrder = StockOrder.CreateSellAtMarketOpenStockOrder(dailyValue.NAME, dailyValue.DATE, dailyValue.DATE.AddDays(30), number, false);
                }
            }
            #endregion
            return stockOrder;
        }
        #endregion
    }
}
