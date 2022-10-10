using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi.Core.Extensions;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    public class PalmaresViewModel : NotifyPropertyChangedBase
    {
        static public Array Groups
        {
            get { return Enum.GetValues(typeof(StockSerie.Groups)); }
        }

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

        private StockBarDuration barDuration;
        public StockBarDuration BarDuration
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

        private DateTime fromDate;
        public DateTime FromDate
        {
            get { return fromDate; }
            set
            {
                if (value != fromDate)
                {
                    fromDate = value;
                    OnPropertyChanged("FromDate");
                }
            }
        }

        private DateTime toDate;
        public DateTime ToDate
        {
            get { return toDate; }
            set
            {
                if (value != toDate)
                {
                    toDate = value;
                    OnPropertyChanged("ToDate");
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

        public bool ExportEnabled => this.Lines != null && this.Lines.Count > 0;

        public bool DownloadIntraday { get; set; }

        public List<PalmaresLine> Lines { get; set; }

        public PalmaresViewModel()
        {
            this.Lines = new List<PalmaresLine>();
            this.ToDate = DateTime.Now;
            this.FromDate = new DateTime(this.ToDate.Year, 1, 1);

            string path = Folders.Palmares;
            this.Settings = new ObservableCollection<string>(Directory.EnumerateFiles(path).Select(s => Path.GetFileNameWithoutExtension(s)).OrderBy(s => s));
            this.Setting = this.Settings.FirstOrDefault();

            DownloadIntraday = false;
        }
        public bool Calculate()
        {
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
            #endregion

            if (this.DownloadIntraday)
            {
                if (this.group != StockSerie.Groups.INTRADAY)
                {
                    var dataProvider = StockDataProviderBase.GetDataProvider(StockDataProvider.ABC) as ABCDataProvider;
                    dataProvider.DownloadAllGroupsIntraday();
                }
            }

            Lines = new List<PalmaresLine>();
            foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(this.group)))
            {
                if (this.DownloadIntraday && this.group == StockSerie.Groups.INTRADAY)
                {
                    StockDataProviderBase.DownloadSerieData(stockSerie);
                }
                if (!stockSerie.Initialise() || stockSerie.Count < 50)
                    continue;

                var previousDuration = stockSerie.BarDuration;
                stockSerie.BarDuration = this.barDuration;

                #region Calculate variation
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                var openSerie = stockSerie.GetSerie(StockDataType.OPEN);

                var startIndex = stockSerie.IndexOfFirstGreaterOrEquals(this.FromDate);
                if (startIndex == -1)
                {
                    continue;
                }
                var endIndex = stockSerie.IndexOfFirstLowerOrEquals(this.ToDate);
                if (endIndex < 50)
                {
                    continue;
                }

                float lastValue = closeSerie[endIndex];
                float firstValue = closeSerie[startIndex];
                float periodVariation = (lastValue - firstValue) / firstValue;
                firstValue = closeSerie[endIndex - 1];
                float barVariation = (lastValue - firstValue) / firstValue;
                var lastBar = stockSerie.Values.ElementAt(endIndex);
                var bodyHigh = stockSerie.GetSerie(StockDataType.BODYHIGH);

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
                float stopValue = float.NaN;
                if (trailStopSerie != null)
                {
                    try
                    {
                        trailStopSerie.ApplyTo(stockSerie);
                        stopValue = trailStopSerie.Series[0][endIndex];
                        if (float.IsNaN(stopValue))
                            stopValue = trailStopSerie.Series[1][endIndex];
                        stopValue = (lastValue - stopValue) / lastValue;
                    }
                    catch { }
                }
                #endregion

                Lines.Add(new PalmaresLine
                {
                    Sector = stockSerie.SectorId == 0 ? null : ABCDataProvider.SectorCodes.FirstOrDefault(s => s.Code == stockSerie.SectorId).Sector,
                    Group = stockSerie.StockGroup.ToString(),
                    ShortName = stockSerie.ShortName,
                    //ShortName = "=HYPERLINK(\"https://www.abcbourse.com/graphes/eod/" + stockSerie.ShortName + "p\";\"" + stockSerie.StockName + "\")",
                    Name = stockSerie.StockName,
                    Value = lastValue,
                    Highest = highest,
                    Volume = lastBar.EXCHANGED,
                    Indicator1 = stockIndicator1,
                    Indicator2 = stockIndicator2,
                    Indicator3 = stockIndicator3,
                    Stop = stopValue,
                    PeriodVariation = periodVariation,
                    BarVariation = barVariation,
                    LastDate = lastBar.DATE
                    // Link = stockSerie.DataProvider == StockAnalyzer.StockClasses.StockDataProviders.StockDataProvider.ABC ? $"https://www.abcbourse.com/graphes/eod/{stockSerie.ShortName}p" : null
                });

                stockSerie.BarDuration = previousDuration;
            }

            OnPropertyChanged("Lines");
            OnPropertyChanged("ExportEnabled");
            return true;
        }
    }
}
