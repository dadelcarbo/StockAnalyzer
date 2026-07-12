using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
using StockAnalyzer.StockData;
using System.Collections.Generic;
using System.Data;

namespace StockAnalyzer.StockAgent
{
    public interface IStockEntryStop
    {
        string Description { get; }

        DataSerie DataSerie { get; }

        bool Initialize(StockInstrument instrument, BarDuration duration, int minIndex);

        float GetStop(int index);


        string GetParameterValues();
        void SetParams(IEnumerable<StockAgentParam> paramList);
        bool AreSameParams(IStockAgent other);

        string ToParamValueString();
        string ToLog();
    }
    public interface IStockEntryTarget
    {
        string Description { get; }

        DataSerie DataSerie { get; }

        bool Initialize(StockInstrument instrument, BarDuration duration, int minIndex);

        float GetTarget(int index);

        string GetParameterValues();
        void SetParams(IEnumerable<StockAgentParam> paramList);
        bool AreSameParams(IStockAgent other);

        string ToParamValueString();
        string ToLog();
    }
}
