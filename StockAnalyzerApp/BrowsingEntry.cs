using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp
{
    public class BrowsingEntry
    {
        public string StockName { get; set; }
        public StockBarDuration BarDuration { get; set; }
        public string Theme { get; set; }
    }
}
