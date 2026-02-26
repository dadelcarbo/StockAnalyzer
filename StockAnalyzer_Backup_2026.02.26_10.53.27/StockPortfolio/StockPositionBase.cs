using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StockAnalyzer.StockPortfolio
{
    public abstract class StockPositionBase
    {
        // Identification
        public long Id { get; set; }
        public string StockName { get; set; }
        public string ISIN { get; set; }
        public long Uic { get; set; }

        // Entry
        public int EntryQty { get; set; }
        public float EntryValue { get; set; } // This doesn't include transaction fees
        [JsonIgnore]
        public float EntryCost => EntryValue * EntryQty;
        public DateTime EntryDate { get; set; }

        // Exit
        public DateTime? ExitDate { get; set; }
        public float? ExitValue { get; set; }
        [JsonIgnore]
        public bool IsClosed => ExitDate != null;

        // Risk Management
        public float PortfolioValue { get; set; } // PortfolioValue at time of entry.


        public List<TrailStopHistory> TrailStopHistory { get; set; } = new List<TrailStopHistory>();
        public float TrailStop { get; set; }
        public long TrailStopId { get; set; }
        public float Stop { get; set; }

        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }

        internal void Dump()
        {
            if (this.IsClosed)
            {
                StockLog.Write($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{ExitDate.Value.ToShortDateString()}");
            }
            else
            {
                StockLog.Write($"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened");
            }
        }

        public override string ToString()
        {
            return this.IsClosed ? $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} EndDate:{ExitDate.Value.ToShortDateString()} ExitValue:{ExitValue.Value}"
                : $"Name: {StockName} Qty:{EntryQty} StartDate:{EntryDate.ToShortDateString()} Opened";
        }

    }
}