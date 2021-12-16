using StockAnalyzer.StockClasses;
using System.Collections.Generic;

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
        string DisplayIndicator { get; }
        StockTradeSummary TradeSummary { get; }

        bool Initialize(StockSerie stockSerie, StockBarDuration duration, float stopATR);
        TradeAction Decide(int index);

        bool CanOpen(int index);
        bool CanClose(int index);

        void OpenTrade(StockSerie serie, int entryIndex, int qty = 1, bool isLong = true);

        void CloseTrade(int exitIndex);

        void PartlyCloseTrade(int exitIndex);

        void EvaluateOpenedPositions();


        void Randomize();
        IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren);


        string GetParameterValues();
        void SetParams(IEnumerable<StockAgentParam> paramList);
        bool AreSameParams(IStockAgent other);

        string ToParamValueString();
        string ToLog();
    }
}
