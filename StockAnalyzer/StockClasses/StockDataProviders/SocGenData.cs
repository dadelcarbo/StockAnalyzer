using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class SocGenData
    {
        public List<SocGenDataEntry> Series { get; set; }
    }
    public class SocGenDataEntry
    {
        public float? Ask { get; set; }

        public float? Bid { get; set; }

        public DateTimeOffset? Date { get; set; }

        public float? IndexPrice { get; set; }

        public float? UnderlyingPrice { get; set; }
    }
}
