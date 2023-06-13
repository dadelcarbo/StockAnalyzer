using Saxo.OpenAPI.TradingServices;
using StockAnalyzer;
using StockAnalyzer.StockPortfolio;
using System;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public class OrderViewModel : NotifyPropertyChangedBase
    {
        OrderActivity order;
        public OrderViewModel(OrderActivity order, StockPortfolio portfolio)
        {
            this.order = order;
        }

        public string BuySell => order.BuySell;
        public string StockName => order.StockName;

        public string OrderType => order.OrderType;
        public string Status => order.Status;
        public int Qty => (int)order.Amount;
        public float Value => order.Price.HasValue ? order.Price.Value : order.ExecutionPrice.Value;
        public float Amount => Qty * Value;
        public DateTime CreationDate => order.ActivityTime;

    }
}
