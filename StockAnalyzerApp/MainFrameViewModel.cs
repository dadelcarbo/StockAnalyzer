using StockAnalyzer;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;

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

        #region Bar Duration
        private BarDuration barDuration;
        public BarDuration BarDuration { get { return barDuration; } set { SetProperty(ref barDuration, value); } }

        public void SetBarDuration(BarDuration bd, bool notifyPropertyChanged)
        {
            if (notifyPropertyChanged)
            {
                this.BarDuration = bd;
            }
            else
            {
                this.barDuration = bd;
            }
        }
        #endregion

        #region Theme
        private string theme;
        public string Theme { get { return theme; } set { SetProperty(ref theme, value); } }

        #endregion

        #region StockSerie
        #endregion

        private int browsingHistoryIndex = 0;
        private readonly List<BrowsingEntry> browingHistory = new List<BrowsingEntry>();

        public bool IsHistoryActive { get; set; } = true;

        internal void AddHistory(string stockName, string theme)
        {
            if (!IsHistoryActive)
                return;
            int index = -1;
            foreach (var item in this.browingHistory)
            {
                index++;
                if (item.StockName == stockName && item.Theme == theme && item.BarDuration == this.BarDuration)
                {
                    return; // Already in history
                }
            }
            this.browingHistory.Add(new BrowsingEntry
            {
                StockName = stockName,
                BarDuration = this.BarDuration,
                Theme = theme,
            });
            browsingHistoryIndex = this.browingHistory.Count - 1;
        }

        internal void BrowseNext()
        {
            if (browsingHistoryIndex < browingHistory.Count - 1)
            {
                browsingHistoryIndex++;
                var browsingEntry = browingHistory[browsingHistoryIndex];
                StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationAndThemeChanged(browsingEntry.StockName, browsingEntry.BarDuration, browsingEntry.Theme, false);
            }
        }
        internal void BrowseBack()
        {
            if (browsingHistoryIndex > 0)
            {
                browsingHistoryIndex--;
                var browsingEntry = browingHistory[browsingHistoryIndex];
                StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationAndThemeChanged(browsingEntry.StockName, browsingEntry.BarDuration, browsingEntry.Theme, false);
            }
        }
    }
}
