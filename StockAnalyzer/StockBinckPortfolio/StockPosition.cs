using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockPosition
    {
        public StockPosition()
        {
            ExitDate = DateTime.MaxValue;
            this.Leverage = 1;
        }
        public int Id { get; set; }
        public string StockName { get; set; }
        public int EntryQty { get; set; }
        public float EntryValue { get; set; }
        public float EntryFee { get; set; }
        public float EntryNetValue => EntryCost / EntryQty;
        public float EntryCost => EntryValue * EntryQty + EntryFee;
        public DateTime EntryDate { get; set; }

        public float Stop { get; set; }
        public StockBarDuration BarDuration { get; set; }
        public string EntryComment { get; set; }
        public string Indicator { get; set; }
        public DateTime ExitDate { get; set; }
        public bool IsClosed => ExitDate != DateTime.MaxValue;
        public bool IsShort { get; set; }
        public float Leverage { get; set; }

        internal void Dump()
        {
            if (this.IsClosed)
            {
                Console.WriteLine($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{ExitDate.ToShortDateString()}");
            }
            else
            {
                Console.WriteLine($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened");
            }
        }

        public override string ToString()
        {
            return this.IsClosed ? $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{ExitDate.ToShortDateString()}"
                : $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened";
        }
    }
}