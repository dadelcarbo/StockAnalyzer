using StockAnalyzer.StockClasses;
using System.Collections.Generic;

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
        public string Screener { get; set; }
        public string Theme { get; set; }
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
        public string DisplayName { get; set; }
        public List<object> SelectedDistinctValues = new List<object>();
        public FilterDescriptorProxy Filter1 { get; set; }
        public Telerik.Windows.Data.FilterCompositionLogicalOperator FieldFilterLogicalOperator { get; set; }
        public FilterDescriptorProxy Filter2 { get; set; }
    }
}
