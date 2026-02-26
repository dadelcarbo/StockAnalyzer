using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public interface IStockAutoDrawing : IStockViewableSeries, IStockEvent, IStockText
    {
        FloatSerie[] Series { get; }

        StockDrawingItems DrawingItems { get; }
    }
}
