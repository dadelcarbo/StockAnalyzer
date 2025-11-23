using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    public class SaxoMappingStyleSelector : StyleSelector
    {
        public Style NotFoundStyle { get; set; }
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var mapping = item as SaxoUnderlying;
            if (mapping == null)
                return null;

            if (!string.IsNullOrEmpty(mapping.SerieName) && StockDictionary.Instance.ContainsKey(mapping.SerieName))
            {
                return base.SelectStyle(item, container);
            }
            else
            {
                return NotFoundStyle;
            }
        }
    }
}