using StockAnalyzer.StockClasses;
using StockAnalyzerApp.StockData;
using System;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public class MomentumViewModel
    {
        public BarDuration BarDuration { get; set; }
        public StockInstrument Instrument { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float Value { get; set; }
    }
}
