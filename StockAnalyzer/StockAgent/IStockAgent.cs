using StockAnalyzer.StockClasses;
using System.Collections.Generic;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public enum TradeAction
    {
        Nothing,
        Buy,
        Sell,
        PartSell
    }

    public interface IStockAgent
    {
        string Description { get; }
        StockTradeSummary TradeSummary { get; }

        void Initialize(StockSerie stockSerie, StockBarDuration duration);
        TradeAction Decide(int index);

        void OpenTrade(StockSerie serie, int entryIndex, bool isLong = true);

        void CloseTrade(int exitIndex);

        void PartlyCloseTrade(int exitIndex);

        void EvaluateOpenedPositions();


        void Randomize();
        IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren);

        string ToLog();

        string GetParameterValues();
        void SetParam(PropertyInfo property, StockAgentParamAttribute attribute, float newValue);
        bool AreSameParams(IStockAgent other);
    }
}
