using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;
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

        StockInstrument Instrument { get; }
        DataSerie DataSerie { get; }

        bool Initialize(StockInstrument instrument, BarDuration duration, IStockEntryStop entryStopAgent, IStockEntryTarget entryTargetAgent);
        TradeAction Decide(int index);

        bool CanOpen(int index);
        bool CanClose(int index);

        void OpenTrade(int entryIndex, int qty = 1);

        void CloseTrade(int exitIndex);

        void EvaluateOpenedPositions();


        void Randomize();
        IList<IStockAgent> Reproduce(IStockAgent partner, int nbChildren);

        string ToLog();
    }
}
