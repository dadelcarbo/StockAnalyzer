using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockPortfolio
{
    public class StockPosition
    {
        public StockPosition()
        {
        }
        public long Id { get; set; }
        public string StockName { get; set; }
        public string ISIN { get; set; }

        public int EntryQty { get; set; }
        public float EntryValue { get; set; } // This includes transaction fees
        public float EntryCost => EntryValue * EntryQty;
        public DateTime EntryDate { get; set; }

        public float TrailStop { get; set; }
        public float Stop { get; set; }

        public StockBarDuration BarDuration { get; set; }

        public string EntryComment { get; set; }
        public string Theme { get; set; }
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
            return this.IsClosed ? $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{ExitDate.Value.ToShortDateString()} ExitValue:{ExitValue.Value}"
                : $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened";
        }
    }
}