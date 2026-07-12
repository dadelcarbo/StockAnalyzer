using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzerApp.CustomControl.DrawingDlg
{
    public class DrawingViewModel : NotifyPropertyChangedBase
    {
        public StockInstrument Instrument { get; internal set; }
        public BarDuration Duration { get; internal set; }

        public DrawingViewModel(StockInstrument instrument, BarDuration duration)
        {
            Instrument = instrument;
            Duration = duration;
        }
    }
}
