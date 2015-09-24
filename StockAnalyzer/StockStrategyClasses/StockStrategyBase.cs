using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;

namespace StockAnalyzer.StockStrategyClasses
{
   public abstract class StockStrategyBase : IStockStrategy
   {
      #region StockStrategy Properties
      abstract public string Description
      {
         get;
      }
      public bool IsBuyStrategy
      {
         get { return true; }
      }
      public bool IsSellStrategy
      {
         get { return true; }
      }
      public bool SupportShortSelling { get; protected set; }
      public StockSerie Serie { get; set; }
      public StockOrder LastBuyOrder { get; set; }
      public IStockEvent TriggerIndicator { get; set; }
      #endregion
      #region StockStrategy Methods
      virtual public void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling)
      {
         this.Serie = stockSerie;
         this.LastBuyOrder = lastBuyOrder;
         this.SupportShortSelling = supportShortSelling;

         IStockViewableSeries triggerSerie = this.TriggerIndicator as IStockViewableSeries;
         if (triggerSerie != null)
         {
            if (stockSerie.HasVolume || !triggerSerie.RequiresVolumeData)
            {
               triggerSerie.ApplyTo(stockSerie);
            }
            else
            {
               throw new System.Exception("This serie has probably no volume");
            }
         }
      }

      virtual public StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         return null;
      }
      virtual public StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         return null;
      }
      virtual public void AmendBuyOrder(ref StockOrder stockOrder, StockDailyValue dailyValue, int index, float amount, ref float benchmark)
      {
         stockOrder = TryToBuy(dailyValue, index, amount, ref benchmark);
      }
      virtual public void AmendSellOrder(ref StockOrder stockOrder, StockDailyValue dailyValue, int index, int number, ref float benchmark)
      {
         stockOrder = TryToSell(dailyValue, index, number, ref benchmark);
      }
      #endregion
   }
}
