using Saxo.OpenAPI.TradingServices;
using StockAnalyzer;
using StockAnalyzer.StockPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public class ActivityOrderViewModel : NotifyPropertyChangedBase
    {
        OrderActivity order;
        public ActivityOrderViewModel(OrderActivity order)
        {
            this.order = order;
        }

        public DateTime ActivityTime => order.ActivityTime;
        public long Uic => order.Uic;
        public string Name {get;set;}
        public float Amount => order.Amount;
        public string AssetType => order.AssetType;
        public string BuySell => order.BuySell;
        public string Duration => order.Duration?.DurationType;
        public long LogId => order.LogId;
        public long OrderId => order.OrderId;
        public string OrderRelation => order.OrderRelation;
        public string OrderType => order.OrderType;
        public string RelatedOrders => order.RelatedOrders == null ? null : string.Join(Environment.NewLine, order.RelatedOrders);
        public string Status => order.Status;
        public string SubStatus => order.SubStatus;
        public float? Price => order.Price;
        public float? AveragePrice => order.AveragePrice;
        public float? ExecutionPrice => order.ExecutionPrice;
        public float? FillAmount => order.FillAmount;
        public float? FilledAmount => order.FilledAmount;
        public long? PositionId => order.PositionId;
    }
}
