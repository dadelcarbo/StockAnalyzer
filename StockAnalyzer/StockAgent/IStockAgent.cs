using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockAgent
{
    public enum TradeAction
    {
        Nothing,
        Buy,
        Sell
    }

    public interface IStockAgent
    {
        void Randomize();

        TradeAction Decide();

        IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren);

        string ToLog();
    }
}
