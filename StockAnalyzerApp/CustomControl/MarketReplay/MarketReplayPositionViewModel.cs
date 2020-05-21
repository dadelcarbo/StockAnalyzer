using StockAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public class MarketReplayPositionViewModel : NotifyPropertyChangedBase
    {
        public int Qty { get; set; }
        public float Entry { get; set; }
        public float Value { get; private set; }
        public float Stop { get; set; }
        public float Target1 { get; set; }
        public string Gain => ((Value - Entry) / Entry).ToString("P2");

        public void SetValue(float value)
        {
            this.Value = value;
            this.OnPropertyChanged("Value");
            this.OnPropertyChanged("Gain");
        }
    }
}
