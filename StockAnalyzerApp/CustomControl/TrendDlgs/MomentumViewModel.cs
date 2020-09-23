using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public class MomentumViewModel
    {
        public StockBarDuration BarDuration { get; set; }
        public StockSerie StockSerie { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float Value { get; set; }
    }
}
