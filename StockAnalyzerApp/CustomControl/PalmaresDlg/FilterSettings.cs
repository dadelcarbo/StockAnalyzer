using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresSettings
    {
        public List<FilterSetting> FilterSettings { get; set; }
        public StockBarDuration BarDuration { get; set; }
        public StockSerie.Groups Group { get; set; }
        public string Indicator1 { get; set; }
        public string Indicator2 { get; set; }
        public string Indicator3 { get; set; }
        public string Stop { get; set; }
    }
    public class FilterDescriptorProxy
    {
        public Telerik.Windows.Data.FilterOperator Operator { get; set; }
        public object Value { get; set; }
        public bool IsCaseSensitive { get; set; }
    }

    public class FilterSetting
    {
        public string ColumnUniqueName { get; set; }
        public List<object> SelectedDistinctValues = new List<object>();
        public FilterDescriptorProxy Filter1 { get; set; }
        public Telerik.Windows.Data.FilterCompositionLogicalOperator FieldFilterLogicalOperator { get; set; }
        public FilterDescriptorProxy Filter2 { get; set; }
    }
}
