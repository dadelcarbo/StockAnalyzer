using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class SaxoMappingStyleSelector : StyleSelector
    {
        public Style NotFoundStyle { get; set; }
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
            else
            {
                var saxoInstrument = item as SaxoInstrument;
                if (saxoInstrument == null)
                    return null;

                if (string.IsNullOrEmpty(saxoInstrument.Isin))
                {
                    if (!string.IsNullOrEmpty(saxoInstrument.Symbol))
                    {
                        var symbol = saxoInstrument.Symbol.Split(':')[0];
                        var instrument = StockDictionary.Instruments.Values.FirstOrDefault(s => s.Symbol == symbol);
                        if (instrument != null)
                        {
                            saxoInstrument.Isin = instrument.Isin;
                        }
                    }
                    return NotFoundStyle;
                }
                else
                {
                    return base.SelectStyle(item, container);
                }
            }
        }
    }
}