using System.Collections.Generic;

namespace StockAnalyzer.StockAgent
{
    public interface IStockParameterized
    {
        string GetParameterValues();
        void SetParams(IEnumerable<StockAgentParam> paramList);
        bool AreSameParams(IStockAgent other);
        string ToParamValueString();
    }
}