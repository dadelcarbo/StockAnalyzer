using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockScreeners;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresViewModel : NotifyPropertyChangedBase
    {
        static public Array Groups => Enum.GetValues(typeof(StockSerie.Groups));

        private StockSerie.Groups group;
        public StockSerie.Groups Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {
                    group = value;
                    OnPropertyChanged("Group");
                    this.Lines?.Clear();
                    OnPropertyChanged("Lines");
                    OnPropertyChanged("ExportEnabled");
                }
            }
        }

        private BarDuration barDuration;
        public BarDuration BarDuration
        {
            get { return barDuration; }
            set
            {
                if (value != barDuration)
                {
                    barDuration = value; OnPropertyChanged("BarDuration");
                }
            }
        }

        private string indicator1;
        public string Indicator1
        {
            get { return indicator1; }
            set
            {
                if (value != indicator1)
                {
                    indicator1 = value;
                    OnPropertyChanged("Indicator1");
                }
            }
        }
        private string indicator2;
        public string Indicator2
        {
            get { return indicator2; }
            set
            {
                if (value != indicator2)
                {
                    indicator2 = value;
                    OnPropertyChanged("Indicator2");
                }
            }
        }
        private string indicator3;
        public string Indicator3
        {
            get { return indicator3; }
            set
            {
                if (value != indicator3)
                {
                    indicator3 = value;
                    OnPropertyChanged("Indicator3");
                }
            }
        }

        private bool athOnly = false;
        public bool AthOnly
        {
            get { return athOnly; }
            set
            {
                if (value != athOnly)
                {
                    athOnly = value;
                    OnPropertyChanged("AthOnly");
                }
            }
        }

        private int ath1 = 175;
        public int Ath1
        {
            get { return ath1; }
            set
            {
                if (value != ath1)
                {
                    ath1 = value;
                    OnPropertyChanged("Ath1");
                }
            }
        }

        private int ath2 = 35;
        public int Ath2
        {
            get { return ath2; }
            set
            {
                if (value != ath2)
                {
                    ath2 = value;
                    OnPropertyChanged("Ath2");
                }
            }
        }

        private bool bullOnly = true;
        public bool BullOnly
        {
            get { return bullOnly; }
            set
            {
                if (value != bullOnly)
                {
                    bullOnly = value;
                    OnPropertyChanged("BullOnly");
                }
            }
        }
        private string stop;
        public string Stop
        {
            get { return stop; }
            set
            {
                if (value != stop)
                {
                    stop = value;
                    OnPropertyChanged("Stop");
                }
            }
        }
        private bool screenerOnly;
        public bool ScreenerOnly
        {
            get { return screenerOnly; }
            set
            {
                if (value != screenerOnly)
                {
                    screenerOnly = value;
                    OnPropertyChanged("ScreenerOnly");
                }
            }
        }
        private string screener;
        public string Screener
        {
            get { return screener; }
            set
            {
                if (value != screener)
                {
                    screener = value;
                    OnPropertyChanged("Screener");
                }
            }
        }

        private string theme;
        public string Theme
        {
            get { return theme; }
            set
            {
                if (value != theme)
                {
                    theme = value;
                    OnPropertyChanged("Theme");
                }
            }
        }

        public ObservableCollection<string> Settings { get; set; }

        private string setting;
        public string Setting
        {
            get { return setting; }
            set
            {
                if (value != setting)
                {
                    setting = value;
                    OnPropertyChanged("Setting");
                }
            }
        }

        private int nbStocks;
        public int NbStocks
        {
            get { return nbStocks; }
            set
            {
                if (value != nbStocks)
                {
                    nbStocks = value;
                    OnPropertyChanged("NbStocks");
                }
            }
        }

        private string runStatus = "Run";
        public string RunStatus
        {
            get { return runStatus; }
            set
            {
                if (value != runStatus)
                {
                    runStatus = value;
                    OnPropertyChanged("RunStatus");
                }
            }
        }

        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                if (value != progress)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }
        private Visibility progressVisibility;
        public Visibility ProgressVisibility
        {
            get { return progressVisibility; }
            set
            {
                if (value != progressVisibility)
                {
                    progressVisibility = value;
                    OnPropertyChanged("ProgressVisibility");
                }
            }
        }

        public bool ExportEnabled => this.Lines != null && this.Lines.Count > 0;

        public bool DownloadIntraday { get; set; }

        public ObservableCollection<PalmaresLine> Lines { get; set; }

        public IEnumerable<string> Themes { get; set; }
        public PalmaresViewModel()
        {
            this.Lines = new ObservableCollection<PalmaresLine>();

            string path = Folders.Palmares;
            this.Settings = new ObservableCollection<string>(Directory.EnumerateFiles(path).Select(s => Path.GetFileNameWithoutExtension(s)).OrderBy(s => s));
            this.Setting = this.Settings.FirstOrDefault();

            this.Themes = StockAnalyzerForm.MainFrame.Themes.Append(string.Empty);
            this.Theme = StockAnalyzerForm.MainFrame.CurrentTheme;
            if (this.Theme.Contains("*"))
                this.Theme = this.Themes.FirstOrDefault();

            DownloadIntraday = false;
            ProgressVisibility = Visibility.Collapsed;
        }

        private bool canceled = false;
        public async Task CalculateAsync()
        {
            if (ProgressVisibility == Visibility.Visible)
            {
                canceled = true;
                return;
            }
            else
            {
                this.RunStatus = "Cancel";
                canceled = false;
            }
            ProgressVisibility = Visibility.Visible;
            this.Progress = 0;

            #region Sanity Check
            IStockIndicator viewableSeries1 = null;
            if (!string.IsNullOrEmpty(this.indicator1))
            {
                try
                {
                    viewableSeries1 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator1) as IStockIndicator;
                }
                catch { }
            }
            IStockIndicator viewableSeries2 = null;
            if (!string.IsNullOrEmpty(this.indicator2))
            {
                try
                {
                    viewableSeries2 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator2) as IStockIndicator;
                }
                catch { }
            }
            IStockIndicator viewableSeries3 = null;
            if (!string.IsNullOrEmpty(this.indicator3))
            {
                try
                {
                    viewableSeries3 = StockViewableItemsManager.GetViewableItem("Indicator|" + this.indicator3) as IStockIndicator;
                }
                catch { }
            }
            IStockTrailStop trailStopSerie = null;
            if (!string.IsNullOrEmpty(this.stop))
            {
                try
                {
                    trailStopSerie = StockViewableItemsManager.GetViewableItem("TRAILSTOP|" + this.stop) as IStockTrailStop;
                }
                catch { }
            }
            IStockScreener screenerSerie = null;
            if (!string.IsNullOrEmpty(this.screener))
            {
                try
                {
                    screenerSerie = StockScreenerManager.CreateScreener(this.screener);
                }
                catch { }
            }
            #endregion
            if (this.DownloadIntraday)
            {
                if (this.group != StockSerie.Groups.INTRADAY && this.group != StockSerie.Groups.TURBO)
                {
                    var dataProvider = StockDataProviderBase.GetDataProvider(StockDataProvider.ABC) as ABCDataProvider;
                    dataProvider.DownloadAllGroupsIntraday();
                }
            }

            Lines = new ObservableCollection<PalmaresLine>();
            OnPropertyChanged("Lines");
            await Task.Delay(10);

            try
            {
                var stockList = StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.group)).ToList();
                this.Progress = 0;
                this.NbStocks = stockList.Count;
                int count = 0;
                foreach (var stockSerie in stockList)
                {
                    if (canceled)
                    {
                        break;
                    }
                    count++;
                    if (count % 10 == 0)
                        this.Progress = count;
                    if (this.DownloadIntraday && (this.group == StockSerie.Groups.INTRADAY || this.group == StockSerie.Groups.TURBO))
                    {
                        StockDataProviderBase.DownloadSerieData(stockSerie);
                    }
                    if (!stockSerie.Initialise())
                        continue;


                    var previousDuration = stockSerie.BarDuration;
                    stockSerie.BarDuration = this.BarDuration;
                    if (stockSerie.Count < 40)
                    {
                        stockSerie.BarDuration = previousDuration;
                        continue;
                    }

                    var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                    var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                    var openSerie = stockSerie.GetSerie(StockDataType.OPEN);

                    var endIndex = stockSerie.LastIndex;


                    #region Calculate ATH
                    var ath = stockSerie.GetIndicator($"ATH({ath1},{ath2})").Series[0][endIndex] > 0.5f;
                    if (!ath && athOnly)
                        continue;
                    #endregion



                    var lastBar = stockSerie.Values.ElementAt(endIndex);
                    float lastValue = lastBar.CLOSE;
                    var firstValue = closeSerie[endIndex - 1];
                    float barVariation = (lastValue - firstValue) / firstValue;
                    var bodyHigh = stockSerie.GetSerie(StockDataType.BODYHIGH);



                    float stopValue = float.NaN;
                    if (trailStopSerie != null)
                    {
                        try
                        {
                            trailStopSerie = stockSerie.GetTrailStop(this.stop);
                            stopValue = trailStopSerie.Series[0][endIndex];
                            if (float.IsNaN(stopValue))
                            {
                                if (bullOnly)
                                    continue;
                                stopValue = trailStopSerie.Series[1][endIndex];
                            }
                            else
                            {
                                stopValue = (lastValue - stopValue) / lastValue;
                            }
                        }
                        catch { }
                    }
                    bool match = true;
                    if (screenerSerie != null)
                    {
                        screenerSerie.ApplyTo(stockSerie);
                        match = screenerSerie.Match[endIndex];
                        if (screenerOnly && !match)
                        {
                            continue;
                        }
                    }

                    #region Calculate Highest
                    int highest = 0;
                    for (int i = endIndex - 1; i > 0; i--)
                    {
                        if (lastValue >= bodyHigh[i])
                        {
                            highest++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    #endregion

                    #region Calculate Indicators
                    float stockIndicator1 = float.NaN;
                    if (viewableSeries1 != null)
                    {
                        try { viewableSeries1.ApplyTo(stockSerie); stockIndicator1 = viewableSeries1.Series[0][endIndex]; } catch { }
                    }
                    float stockIndicator2 = float.NaN;
                    if (viewableSeries2 != null)
                    {
                        try { viewableSeries2.ApplyTo(stockSerie); stockIndicator2 = viewableSeries2.Series[0][endIndex]; } catch { }
                    }
                    float stockIndicator3 = float.NaN;
                    if (viewableSeries3 != null)
                    {
                        try { viewableSeries3.ApplyTo(stockSerie); stockIndicator3 = viewableSeries3.Series[0][endIndex]; } catch { }
                    }
                    #endregion


                    Lines.Add(new PalmaresLine
                    {
                        Match = match,
                        //Sector = stockSerie.SectorId == 0 ? null : ABCDataProvider.SectorCodes.FirstOrDefault(s => s.Code == stockSerie.SectorId)?.Sector,
                        Group = stockSerie.StockGroup.ToString(),
                        Symbol = stockSerie.Symbol,
                        Name = stockSerie.StockName,
                        Value = lastValue,
                        Highest = highest,
                        Volume = lastBar.EXCHANGED / 1000000f,
                        Indicator1 = stockIndicator1,
                        Indicator2 = stockIndicator2,
                        Indicator3 = stockIndicator3,
                        Stop = stopValue,
                        BarVariation = barVariation,
                        LastDate = lastBar.DATE
                    });

                    stockSerie.BarDuration = previousDuration;
                }
            }
            catch (Exception exception)
            {
                StockLog.Write(exception);
                StockAnalyzerException.MessageBox(exception);
            }

            OnPropertyChanged("Lines");
            OnPropertyChanged("ExportEnabled");
            await Task.Delay(0);

            ProgressVisibility = Visibility.Collapsed;
            this.RunStatus = "Run";
        }
    }
}
