using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockClasses
{
    public class StockFinancial
    {
        public string Dividend { get; set; }
        public string Indices { get; set; }
        public string Market { get; set; }
        public int MarketCap { get; set; }
        public string MarketPlace { get; set; }
        public string MeetingDate { get; set; }
        public string PEA { get; set; }
        public string Sector { get; set; }
        public int ShareNumber { get; set; }
        public string SRD { get; set; }
        public float Yield { get; set; }
    }
}
