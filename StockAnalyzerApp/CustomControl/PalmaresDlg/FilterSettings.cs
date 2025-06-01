using StockAnalyzer.StockClasses;
using System.Collections.Generic;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresSettings
    {
        public List<FilterSetting> FilterSettings { get; set; }
        public BarDuration BarDuration { get; set; }
        public StockSerie.Groups Group { get; set; }
        public string Indicator1 { get; set; }
        public string Indicator2 { get; set; }
        public string Indicator3 { get; set; }
        public float Indicator1Min { get; set; }
        public float Indicator2Min { get; set; }
        public float Indicator3Min { get; set; }
        public bool AthOnly { get; set; }
        public int Ath1 { get; set; } = 1;
        public int Ath2 { get; set; }
        public string Stop { get; set; }
        public bool BullOnly { get; set; }
        public bool ScreenerOnly { get; set; }
        public string Screener { get; set; }
        public string Theme { get; set; }
        public float Liquidity { get; set; }
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
