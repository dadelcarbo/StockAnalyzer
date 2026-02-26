using StockAnalyzer.StockClasses;
using System.Collections.Generic;

namespace StockAnalyzer.StockAgent
{

    public interface IStopStrategy
    {
        string Description { get; }
        string DisplayIndicator { get; }

        bool Initialize(StockSerie stockSerie, BarDuration duration);
        bool IsStopped();

        string GetParameterValues();
        void SetParams(IEnumerable<StockAgentParam> paramList);
        bool AreSameParams(IStockAgent other);

        string ToParamValueString();
        string ToLog();
    }
}
