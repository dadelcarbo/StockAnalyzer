using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Collections.Generic;

namespace StockAnalyzer.StockAgent
{
    public enum TradeAction
    {
        Nothing,
        Buy,
        Sell
    }

    public interface IStockPortfolioAgent : IStockAgent
    {
        FloatSerie RankSerie { get; set; }
    }

    public interface IStockAgent : IStockParameterized
    {
        string Description { get; }
        string DisplayIndicator { get; }
        StockTradeSummary TradeSummary { get; }

        StockSerie StockSerie { get; }

        bool Initialize(StockSerie stockSerie, StockBarDuration duration, IStockEntryStop entryStopAgent);
        TradeAction Decide(int index);

        bool CanOpen(int index);
        bool CanClose(int index);

        void OpenTrade(StockSerie serie, int entryIndex, int qty = 1, bool isLong = true);

        void CloseTrade(int exitIndex);

        void EvaluateOpenedPositions();


        void Randomize();
        IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren);

        string ToLog();
    }
}
