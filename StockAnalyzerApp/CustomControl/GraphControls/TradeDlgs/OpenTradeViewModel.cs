using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;

namespace StockAnalyzerApp.CustomControl.GraphControls.TradeDlgs
{
    public class OpenTradeViewModel : NotifyPropertyChangedBase
    {
        public string StockName { get; set; }
        public int Qty { get; set; }
        public float EntryValue { get; set; }
        public float EntryCost => Qty * EntryCost;
        public DateTime EntryDate { get; set; }
        public StockBarDuration BarDuration { get; set; }
        public string IndicatorType { get; set; }
        public string IndicatorName { get; set; }
        public string EntryComment { get; set; }
        public static Array BarDurations => Enum.GetValues(typeof(BarDuration));
        public static IList<int> LineBreaks => new List<int> { 0, 1, 2, 3, 4, 5 };
        public static IList<string> IndicatorTypes => new List<string>() { "Indicator", "PaintBar", "TrailStop", "Trail", "Cloud" };
        public IList<string> IndicatorNames { get; set; }
    }
}
