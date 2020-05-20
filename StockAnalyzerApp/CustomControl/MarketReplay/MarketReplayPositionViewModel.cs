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
        public float Entry { get; set; }
        public float Value { get; set; }
        public float Stop { get; set; }
        public float Target1 { get; set; }
    }
}
