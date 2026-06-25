using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzerApp.StockData;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

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

    public class ContextPersister : IDisposable
    {
        public ContextPersister()
        {
            MainFrameViewModel.Instance.SaveContext();
            MainFrameViewModel.Instance.IsHistoryActive = false;
        }
        public void Dispose()
        {
            MainFrameViewModel.Instance.RestoreContext();
            MainFrameViewModel.Instance.IsHistoryActive = true;
        }
    }

    public struct Context
    {
        public bool IsSaved;
        public BarDuration BarDuration;
        public string Theme;
        public StockInstrument Instrument;
    }

    public class MainFrameViewModel : NotifyPropertyChangedBase
    {
        private MainFrameViewModel() { }

        static public bool NotificationSuspended { get; set; } = false;

        #region Context helper

        private Context context;
        public void SaveContext()
        {
            context.BarDuration = barDuration;
            context.Theme = theme;
            context.Instrument = Instrument;
            context.IsSaved = true;
        }
        public void RestoreContext()
        {
            if (!context.IsSaved)
                return;
            this.SetBarDuration(context.BarDuration, false);
            this.SetTheme(context.Theme, false);
            this.Instrument = context.Instrument;
            context.IsSaved = false;
        }
        #endregion


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

        private string analysisFile;
        public string AnalysisFile { get { return analysisFile; } set { SetProperty(ref analysisFile, value); } }

        #region Theme
        private string theme;
        public string Theme { get { return theme; } set { SetProperty(ref theme, value); } }

        public void SetTheme(string newTheme, bool notifyPropertyChanged)
        {
            if (notifyPropertyChanged)
            {
                this.Theme = newTheme;
            }
            else
            {
                this.theme = newTheme;
            }
        }

        #endregion

        #region Instrument
        StockInstrument instrument;
        public StockInstrument Instrument { get { return instrument; } set { SetProperty(ref instrument, value); } }
        #endregion

        private int browsingHistoryIndex = 0;
        private readonly List<BrowsingEntry> browingHistory = new List<BrowsingEntry>();

        public bool IsHistoryActive { get; set; } = true;

        static MainFrameViewModel instance = new MainFrameViewModel();

        public static MainFrameViewModel Instance => instance;

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
