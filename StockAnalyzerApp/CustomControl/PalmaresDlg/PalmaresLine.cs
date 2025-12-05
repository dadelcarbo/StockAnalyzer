using System;
using System.ComponentModel;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresLine
    {
        //public string Sector { get; set; }
        public string Group { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public bool Match { get; set; }
        public float Value { get; set; }
        [DisplayName("Bar %")]
        public float BarVariation { get; set; }
        [DisplayName("Volume M€")]
        public float Volume { get; set; }
        public int Highest { get; set; }
        public float Indicator1 { get; set; }
        public float Indicator2 { get; set; }
        public float Indicator3 { get; set; }
        public float Stok { get; set; }
        public float Stop { get; set; }
        public DateTime LastDate { get; set; }
    }
}
