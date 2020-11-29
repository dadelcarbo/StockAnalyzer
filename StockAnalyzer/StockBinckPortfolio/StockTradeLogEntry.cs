using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockTradeLogEntry
    {
        public StockTradeLogEntry()
        {
        }
        public int Id { get; set; }
        public string StockName { get; set; }
        public int EntryQty { get; set; }
        public float EntryValue { get; set; }
        public float EntryCost => EntryQty * EntryValue;
        public float Stop { get; set; }
        public DateTime EntryDate { get; set; }
        public StockBarDuration BarDuration { get; set; }
        public string EntryComment { get; set; }
        public string Indicator { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsClosed => EndDate != null;
        internal void Dump()
        {
            if (this.IsClosed)
            {
                Console.WriteLine($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{EndDate?.ToShortDateString()}");
            }
            else
            {
                Console.WriteLine($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened");
            }
        }

        public override string ToString()
        {
            return this.IsClosed ? $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{EndDate?.ToShortDateString()}"
                : $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened";
        }
    }
}