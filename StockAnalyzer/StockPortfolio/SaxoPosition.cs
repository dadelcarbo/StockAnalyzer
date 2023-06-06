using Saxo.OpenAPI.TradingServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockPortfolio
{
    public class SaxoPosition : StockPositionBase
    {
        public List<SaxoPositionChange> Entries { get; set; } = new List<SaxoPositionChange>();
        public List<SaxoPositionChange> Exits { get; set; } = new List<SaxoPositionChange>();

        public float Gain => this.IsClosed ? this.Exits.Sum(e => e.Qty * e.Value) - this.Entries.Sum(e => e.Qty * e.Value) : 0;

        public void AddEntry(OrderActivity orderActivity)
        {
            var qty = (int)orderActivity.Amount;
            this.Entries.Add(new SaxoPositionChange { OrderId = orderActivity.OrderId, Date = orderActivity.ActivityTime, Value = orderActivity.Price.Value, Qty = qty });
            if (this.Entries.Count == 1)
            {
                this.EntryDate = orderActivity.ActivityTime;
                this.EntryValue = orderActivity.Price.Value;
                this.EntryQty = qty;
            }
            else
            {
                this.EntryValue = (this.EntryValue * this.EntryQty + orderActivity.Price.Value * qty) / (this.EntryQty + qty);
                this.EntryQty += qty;
            }
        }
        public void AddExit(OrderActivity orderActivity)
        {
            var qty = (int)orderActivity.Amount;
            if (this.EntryQty < qty)
            {
                throw new InvalidOperationException("Closing more than position size");
            }
            this.Exits.Add(new SaxoPositionChange { OrderId = orderActivity.OrderId, Date = orderActivity.ActivityTime, Value = orderActivity.Price.Value, Qty = qty });
            this.EntryQty -= qty;
            if (this.EntryQty == 0)
            {
                this.ExitDate = orderActivity.ActivityTime;
                this.ExitValue = orderActivity.Price.Value;
            }
        }
    }
}