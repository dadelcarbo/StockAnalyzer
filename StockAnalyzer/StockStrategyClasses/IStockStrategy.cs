using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses.StockViewableItems;

namespace StockAnalyzer.StockStrategyClasses
{
    public interface IStockStrategy
    {
        string Description { get; }
        bool IsBuyStrategy { get; }
        bool IsSellStrategy { get; }
        bool SupportShortSelling { get; }
        StockSerie Serie { get; set; }
        StockOrder LastBuyOrder { get; set; }
        IStockEvent TriggerIndicator { get; set; }

        void Initialise(StockSerie stockSerie, StockOrder lastBuyOrder, bool supportShortSelling);

        StockOrder TryToBuy(StockDailyValue dailyValue, int index, float amount, ref float benchmark);
        StockOrder TryToSell(StockDailyValue dailyValue, int index, int number, ref float benchmark);

        void AmendBuyOrder(ref  StockOrder stockOrder, StockDailyValue dailyValue, int index, float amount, ref float benchmark);
        void AmendSellOrder(ref StockOrder stockOrder, StockDailyValue dailyValue, int index, int number, ref float benchmark);
    }

    public interface IStockFilteredStrategy : IStockStrategy
    {
        IStockEvent FilterIndicator { get; set; }

        string OkToBuyFilterEventName { get; set; }
        string OkToShortFilterEventName { get; set; }
    }
}
