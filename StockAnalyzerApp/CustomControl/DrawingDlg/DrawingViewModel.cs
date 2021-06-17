using StockAnalyzer;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.DrawingDlg
{
    public class DrawingViewModel : NotifyPropertyChangedBase
    {
        public string StockName { get; internal set; }
        public StockBarDuration Duration { get; internal set; }

        public DrawingViewModel(string stockName, StockBarDuration duration)
        {
            StockName = stockName;
            Duration = duration;
        }
    }
}
