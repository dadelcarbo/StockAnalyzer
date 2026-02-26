using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockLogging;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockPortfolio
{
    public class SaxoPosition : StockPositionBase
    {
        public SaxoPosition()
        {
        }
        public SaxoPosition(OrderActivity order)
        {
            var qty = (int)order.Amount;
            this.Entries.Add(new SaxoPositionChange { OrderId = order.OrderId, Date = order.ActivityTime, Value = order.AveragePrice.Value, Qty = qty });
            this.EntryDate = order.ActivityTime;
            this.EntryValue = order.AveragePrice.Value;
            this.EntryQty = qty;
            this.Id = order.PositionId.Value;
            this.Uic = order.Uic;
        }
        public List<SaxoPositionChange> Entries { get; set; } = new List<SaxoPositionChange>();
        public List<SaxoPositionChange> Exits { get; set; } = new List<SaxoPositionChange>();

        public float Gain => this.IsClosed ? this.Exits.Sum(e => e.Qty * e.Value) - this.Entries.Sum(e => e.Qty * e.Value) : 0;

        public void AddEntry(OrderActivity orderActivity)
        {
            var qty = (int)orderActivity.Amount;
            this.Entries.Add(new SaxoPositionChange { OrderId = orderActivity.OrderId, Date = orderActivity.ActivityTime, Value = orderActivity.AveragePrice.Value, Qty = qty });
            if (this.Entries.Count == 1)
            {
                this.EntryDate = orderActivity.ActivityTime;
                this.EntryValue = orderActivity.AveragePrice.Value;
                this.EntryQty = qty;
            }
            else
            {
                this.EntryValue = (this.EntryValue * this.EntryQty + orderActivity.AveragePrice.Value * qty) / (this.EntryQty + qty);
                this.EntryQty += qty;
            }
        }
        public void AddExit(OrderActivity orderActivity)
        {
            var qty = (int)orderActivity.Amount;
            if (this.EntryQty < qty)
            {
                StockLog.Write($"Selling not opened position: {this.StockName} qty:{qty}");
                qty = this.EntryQty;
            }
            this.Exits.Add(new SaxoPositionChange { OrderId = orderActivity.OrderId, Date = orderActivity.ActivityTime, Value = orderActivity.AveragePrice.Value, Qty = qty });
            this.EntryQty -= qty;
            if (this.EntryQty == 0)
            {
                this.ExitDate = orderActivity.ActivityTime;
                this.ExitValue = orderActivity.AveragePrice.Value;
            }
        }
    }
}