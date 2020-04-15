using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        string Description { get; }
        StockTradeSummary TradeSummary { get; }

        void Initialize(StockSerie stockSerie, StockBarDuration duration);
        TradeAction Decide(int index);

        void OpenTrade(StockSerie serie, int entryIndex, bool isLong = true);

        void CloseTrade(int exitIndex);

        void EvaluateOpenedPositions();


        void Randomize();
        IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren);

        string ToLog();

        string GetParameterValues();
        void SetParam(PropertyInfo property, StockAgentParamAttribute attribute, float newValue);
        bool AreSameParams(IStockAgent other);
    }
}
