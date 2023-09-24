using Saxo.OpenAPI.TradingServices;
using StockAnalyzer;
using System;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public class OrderViewModel : NotifyPropertyChangedBase
    {
        SaxoOrder order;
        public OrderViewModel(SaxoOrder order)
        {
            this.order = order;
        }

        public string BuySell => order.BuySell;
        public string StockName => order.StockName;

        public string OrderType => order.OrderType;
        public string Status => order.Status;
        public int Qty => order.Qty;
        public float Value => order.Price.HasValue ? order.Price.Value : order.ExecutionPrice.HasValue ? order.ExecutionPrice.Value : 0;
        public float Amount => Qty * Value;
        public DateTime CreationDate => order.ActivityTime;

    }
}
