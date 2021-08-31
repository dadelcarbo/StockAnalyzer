using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockPosition
    {
        public StockPosition()
        {
        }
        public long Id { get; set; }
        public string StockName { get; set; }

        public int EntryQty { get; set; }
        public float EntryValue { get; set; } // This includes transaction fees
        public float EntryCost => EntryValue * EntryQty;
        public DateTime EntryDate { get; set; }

        public float Stop { get; set; }

        public StockBarDuration BarDuration { get; set; }

        public string EntryComment { get; set; }
        public string Indicator { get; set; }
        public DateTime? ExitDate { get; set; }
        public float? ExitValue { get; set; }
        public bool IsClosed => ExitDate != null;

        internal void Dump()
        {
            if (this.IsClosed)
            {
                Console.WriteLine($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{ExitDate.Value.ToShortDateString()}");
            }
            else
            {
                Console.WriteLine($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened");
            }
        }

        public override string ToString()
        {
            return this.IsClosed ? $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{ExitDate.Value.ToShortDateString()}"
                : $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened";
        }
    }
}