using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockNameMapping
    {
        public StockNameMapping()
        {
            this.Leverage = 1;
        }
        public string BinckName { get; set; }
        public string StockName { get; set; }
        public float Leverage { get; set; }
    }
}
