using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOpenedOrder : StockOrderBase
    {
        public StockOpenedOrder()
        {
        }

        public StockOpenedOrder(SaxoOrder o)
        {
            this.ISIN = o.Isin;
            this.StockName = o.StockName;
            this.BarDuration = o.BarDuration;
            this.StopValue = 0;
            this.Value = o.Price.HasValue ? o.Price.Value : 0;
            this.Uic = o.Uic;
            this.CreationDate = o.CreationTime;
            this.BuySell = o.BuySell;
            this.Id = o.OrderId;
            this.OrderType = o.OrderType;
            this.Qty = o.Qty;
            this.Status = o.Status;
            this.SubStatus = o.SubStatus;

            this.BarDuration = o.BarDuration;
            this.Theme = o.Theme;
            this.EntryComment = o.EntryComment;
            this.StopValue = o.Stop;
        }

        public string SubStatus { get; set; }

        public float StopValue { get; set; }

        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }
    }
}