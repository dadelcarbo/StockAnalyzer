using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockScreeners
{
    public interface IStockScreener
    {
        BoolSerie Match { get; }

        void Initialise(string[] parameters);

        void ApplyTo(StockSerie stockSerie);
    }
}
