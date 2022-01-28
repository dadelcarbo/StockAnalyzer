using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp
{
    public class NotificationSuspender : IDisposable
    {
        public NotificationSuspender()
        {
            MainFrameViewModel.NotificationSuspended = true;
        }
        public void Dispose()
        {
            MainFrameViewModel.NotificationSuspended = false;
        }
    }
    public class MainFrameViewModel : NotifyPropertyChangedBase
    {
        static public bool NotificationSuspended { get; set; } = false;

        private StockBarDuration barDuration;
        public StockBarDuration BarDuration
        {
            get
            {
                return barDuration;
            }
            set
            {
                SetProperty(ref barDuration, value);
            }
        }

        public void SetBarDuration(BarDuration barDuration, int smoothing, bool heikenAshi, int lineBreak, bool notifyPropertyChanged)
        {
            StockBarDuration bd = new StockBarDuration()
            {
                Duration = barDuration,
                Smoothing = smoothing,
                HeikinAshi = heikenAshi,
                LineBreak = lineBreak
            };
            if (notifyPropertyChanged)
            {
                this.BarDuration = bd;
            }
            else
            {
                this.barDuration = bd;
            }
        }
    }
}
