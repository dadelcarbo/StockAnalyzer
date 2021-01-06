using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresLine
    {
        public string Serie { get; set; }
        public float Variation { get; set; }
        public float Value { get; set; }
        public float Indicator1 { get; set; }
        public float Indicator2 { get; set; }
    }
}
