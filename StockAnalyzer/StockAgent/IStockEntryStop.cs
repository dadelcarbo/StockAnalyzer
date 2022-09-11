using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Collections.Generic;

namespace StockAnalyzer.StockAgent
{


    public interface IStockEntryStop
    {
        string Description { get; }

        bool Initialize(StockSerie stockSerie, StockBarDuration duration);

        float GetStop(int index);


        string GetParameterValues();
        void SetParams(IEnumerable<StockAgentParam> paramList);
        bool AreSameParams(IStockAgent other);

        string ToParamValueString();
        string ToLog();
    }
}
