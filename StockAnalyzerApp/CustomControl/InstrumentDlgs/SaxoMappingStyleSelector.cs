using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio.Saxo;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class SaxoMappingStyleSelector : StyleSelector
    {
        public Style NotFoundStyle { get; set; }
        public Style NoMappingStyle { get; set; }

        InstrumentService instrumentService = new InstrumentService();
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item == null)
                return null;
            if (item is SaxoUnderlyingViewModel)
            {
                var mapping = item as SaxoUnderlyingViewModel;
                if (mapping == null)
                    return null;

                if (!string.IsNullOrEmpty(mapping.InstrumentId) && StockDictionary.Instruments.ContainsKey(mapping.InstrumentId))
                {
                    return base.SelectStyle(item, container);
                }
                else
                {
                    return NotFoundStyle;
                }
            }
            else if (item is SaxoInstrument saxoInstrument)
            {
                if (string.IsNullOrEmpty(saxoInstrument.AssetType))
                    return NotFoundStyle;

                var instrument = SaxoToInstrumentMapping.GetInstrument(saxoInstrument.Identifier);
                if (instrument == null)
                {
                    if (!string.IsNullOrEmpty(saxoInstrument.Symbol))
                    {
                        var symbol = saxoInstrument.Symbol.Split(':')[0];
                        instrument = StockDictionary.Instruments.Values.FirstOrDefault(s => s.Symbol == symbol);
                        if (instrument != null)
                        {
                            SaxoToInstrumentMapping.AddMapping(saxoInstrument.Identifier, instrument.Id);
                            return base.SelectStyle(item, container);
                        }
                        else
                        {
                            return NoMappingStyle;
                        }
                    }
                    return NotFoundStyle;
                }
            }
            return base.SelectStyle(item, container);
        }
    }
}