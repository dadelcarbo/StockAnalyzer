using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class SaxoMappingViewModel
    {
        public SaxoInstrument SaxoInstrument { get; set; }

        public StockInstrument Instrument { get; set; }
    }
}
