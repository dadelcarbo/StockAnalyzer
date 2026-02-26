using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public interface IStockIndicator : IStockViewableSeries, IStockEvent, IStockText
    {
        FloatSerie[] Series { get; }
        string[] SerieFormats { get; }
        HLine[] HorizontalLines { get; }
        Area[] Areas { get; }
        FloatSerie GetSerie(string eventName);
    }
}
