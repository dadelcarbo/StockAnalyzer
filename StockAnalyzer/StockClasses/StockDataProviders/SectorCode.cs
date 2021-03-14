using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class SectorCode
    {
        public SectorCode(int code, string sector)
        {
            Code = code;
            Sector = sector;
        }

        public string Sector { get; set; }
        public int Code { get; set; }
    }
}
