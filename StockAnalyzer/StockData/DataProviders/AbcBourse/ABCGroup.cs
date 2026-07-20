using System.Diagnostics;

namespace StockAnalyzer.StockData.DataProviders.AbcBourse
{
    [DebuggerDisplay("{Group}-{AbcGroup}")]
    class ABCGroup
    {
        public Groups Group { get; set; }

        public string AbcGroup { get; set; }

        public string RefSerie { get; set; }

        public Market Market { get; set; }

        public bool LabelOnly { get; set; }

        public string[] Prefixes { get; set; }
    }
}