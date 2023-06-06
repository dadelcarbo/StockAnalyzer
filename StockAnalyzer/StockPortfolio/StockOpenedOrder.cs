using Newtonsoft.Json;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOrder : StockOrderBase
    {
        public StockOrder(OrderActivity activityOrder)
        {
            this.ISIN = "";
            this.Status = activityOrder.Status;
            this.Value = activityOrder.Price.Value;
            this.Uic = activityOrder.Uic;
            this.BuySell = activityOrder.BuySell;
            this.CreationDate = activityOrder.ActivityTime;
            this.Id = activityOrder.OrderId;
            this.OrderType = activityOrder.OrderType;
            this.Qty = (int)activityOrder.Amount;
        }
    }
    public class StockOpenedOrder : StockOrderBase
    {
        public StockOpenedOrder()
        {
            this.IsActive = true;
        }

        public float StopValue { get; set; }

        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }
    }
}