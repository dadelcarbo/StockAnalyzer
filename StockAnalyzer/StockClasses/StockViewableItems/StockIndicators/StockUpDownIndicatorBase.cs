namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public abstract class StockUpDownIndicatorBase : StockIndicatorBase, IStockUpDownState
   {
      public StockUpDownIndicatorBase()
      {
      }

      #region IStockUpDownState
      protected StockSerie.Trend[] upDownStates;
      public StockSerie.Trend[] UpDownState
      {
         get { return this.upDownStates; }
      }

      protected override void CreateEventSeries(int count)
      {
         base.CreateEventSeries(count);
         this.upDownStates = new StockSerie.Trend[count];
      }

      public static StockSerie.Trend BoolToTrend(bool? upTrend)
      {
         if (upTrend == null) return StockSerie.Trend.NoTrend;
         if (upTrend.Value) return StockSerie.Trend.UpTrend;
         return StockSerie.Trend.DownTrend;
      }

      #endregion
   }
}
