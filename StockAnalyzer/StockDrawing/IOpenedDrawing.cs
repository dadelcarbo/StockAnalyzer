using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockDrawing
{
    public interface IOpenedDrawing
    {
        bool IsOpened { get; set; }

        bool TryClose(FloatSerie closeSerie);
    }
}
