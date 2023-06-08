using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOpenedOrder : StockOrderBase
    {
        public StockOpenedOrder()
        {
            this.IsActive = true;
        }

        public StockOpenedOrder(OrderActivity o)
        {
            this.ISIN = o.Isin;
            this.StockName = o.StockName;
            this.BarDuration = o.BarDuration;
            this.StopValue = 0;
            this.Value = o.Price.Value;
            this.Uic = o.Uic;
            this.BarDuration = o.BarDuration;
            this.Theme = o.Theme;
            this.EntryComment = o.EntryComment;
            this.CreationDate = o.CreationTime;
            this.BuySell = o.BuySell;
            this.Id = o.OrderId;
            this.OrderType = o.OrderType;
            this.Qty = (int)o.Amount;
            this.Status = o.Status;
        }

        public float StopValue { get; set; }

        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }
    }
}