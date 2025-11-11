using System.Diagnostics;
using static StockAnalyzer.StockClasses.StockSerie;

namespace StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider
{
    [DebuggerDisplay("{Group}-{AbcGroup}")]
    class ABCGroup
    {
        public Groups Group { get; set; }

        public string AbcGroup { get; set; }

        public string RefSerie { get; set; }

        public string Market { get; set; }

        public bool LabelOnly { get; set; }

        public string[] Prefixes { get; set; }
    }
}