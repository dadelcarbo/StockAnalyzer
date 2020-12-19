using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HMA : StockIndicatorMovingAvgBase
    {
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            this.Series[0] = closeSerie.CalculateHMA((int)this.parameters[0]);

            this.CalculateEvents(stockSerie);
        }
    }
}
