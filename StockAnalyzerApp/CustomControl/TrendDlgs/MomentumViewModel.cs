using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public class MomentumViewModel
    {
        public StockBarDuration BarDuration { get; set; }
        public StockSerie StockSerie { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float Value { get; set; }
        public override string ToString()
        {
            return BarDuration.ToString() + " " + StockSerie.StockGroup.ToString().PadRight(10) + " " + StockSerie.StockName.PadRight(30) + " " + StartDate.ToShortDateString() + "=>" + EndDate.ToShortDateString() + ": " + Value.ToString();
        }
    }
}
