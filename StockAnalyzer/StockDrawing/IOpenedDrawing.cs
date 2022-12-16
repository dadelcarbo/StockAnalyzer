using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockDrawing
{
    public interface IOpenedDrawing
    {
        bool IsOpened { get; set; }

        bool TryClose(FloatSerie closeSerie);
    }
}
