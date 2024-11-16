using StockAnalyzer.StockMath;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public interface IStockDecorator : IStockViewableSeries, IStockEvent
    {
        FloatSerie[] Series { get; }

        string DecoratedItem { get; set; }

        bool[] EventVisibility { get; }

        Pen[] EventPens { get; }
    }
}
