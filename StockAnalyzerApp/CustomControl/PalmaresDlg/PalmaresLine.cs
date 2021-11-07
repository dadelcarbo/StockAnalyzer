using System.ComponentModel;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresLine
    {
        //public string Sector { get; set; }
        public string Group { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
        [DisplayName("Period %")]
        public float PeriodVariation { get; set; }
        [DisplayName("Bar %")]
        public float BarVariation { get; set; }
        [DisplayName("Volume M€")]
        public float Volume { get; set; }
        public int Highest { get; set; }
        public float Indicator1 { get; set; }
        public float Indicator2 { get; set; }
        public float Indicator3 { get; set; }
        public float Stop { get; set; }
    }
}
