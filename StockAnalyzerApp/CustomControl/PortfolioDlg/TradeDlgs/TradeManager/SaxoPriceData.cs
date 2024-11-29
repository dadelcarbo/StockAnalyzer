using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs.TradeManager
{

    public class SaxoPrice
    {
        public Section[] sections { get; set; }
        public object traceUrl { get; set; }
    }

    public class Section
    {
        public string section { get; set; }
        public Field[] fields { get; set; }
    }

    public class Field
    {
        public string field { get; set; }
        public dynamic value { get; set; }
        public string reference { get; set; }
    }

}
