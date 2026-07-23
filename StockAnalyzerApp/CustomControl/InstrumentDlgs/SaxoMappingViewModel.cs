using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockData;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class SaxoMappingViewModel
    {
        public SaxoInstrument SaxoInstrument { get; set; }

        public StockInstrument Instrument { get; set; }
    }
}
