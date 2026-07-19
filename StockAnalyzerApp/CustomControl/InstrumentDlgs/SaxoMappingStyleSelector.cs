using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
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
                var instrument = item as SaxoInstrument;
                if (instrument == null)
                    return null;

                if (string.IsNullOrEmpty(instrument.Isin))
                {
                    if (!string.IsNullOrEmpty(instrument.Symbol))
                    {
                        var symbol = instrument.Symbol.Split(':')[0];
                        var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.Symbol == symbol);
                        if (stockSerie != null)
                        {
                            instrument.Isin = stockSerie.ISIN;
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